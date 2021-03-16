using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using MyServiceBus.TcpClient;
using Newtonsoft.Json;
using Prometheus;
using Service.Wallet.Api.Authentication;
using Service.Wallet.Api.Hubs;
using Service.Wallet.Api.Middleware;
using Service.Wallet.Api.Modules;
using Service.Wallet.Api.Settings;
using SimpleTrading.BaseMetrics;
using SimpleTrading.ServiceStatusReporterConnector;
using SimpleTrading.SettingsReader;

namespace Service.Wallet.Api
{
    public class Startup
    {
        private readonly MyNoSqlTcpClient _myNoSqlClient;
        private readonly MyServiceBusTcpClient _serviceBusClient;
        private const string SessionEncodingKeyEnv = "SESSION_ENCODING_KEY";
        private const string EnvInfo = "ENV_INFO";

        public IConfiguration Configuration { get; }

        /// <summary>
        /// SessionEncodingKey
        /// </summary>
        public static byte[] SessionEncodingKey { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _myNoSqlClient = new MyNoSqlTcpClient(
                () => SettingsReader.ReadSettings<SettingsModel>(Program.SettingsFileName).MyNoSqlReaderHostPort,
                ApplicationEnvironment.HostName ?? $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");

            _serviceBusClient = new MyServiceBusTcpClient(Program.ReloadedSettings(model => model.SpotServiceBusHostPort), ApplicationEnvironment.HostName);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<ApplicationLifetimeManager>();

            services.AddApplicationInsightsTelemetry(Configuration);
            services.SetupSwaggerDocumentation();
            services.ConfigurateHeaders();
            services.AddControllers(options => 
            {
                
            });//.AddNewtonsoftJson(); //todo: ask why we use NewtonsoftJson?

            services
                .AddSignalR(option =>
                {
                    option.EnableDetailedErrors = true;

                })
                //.AddMessagePackProtocol()
                ;
            
            services
                .AddAuthentication(o => { o.DefaultScheme = "Bearer"; })
                .AddScheme<WalletAuthenticationOptions, WalletAuthHandler>("Bearer", o => { });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            //app.UseMiddleware<ExceptionLogMiddleware>();

            app.BindMetricsMiddleware();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseStaticFiles();

            app.UseMetricServer();

            app.BindServicesTree(Assembly.GetExecutingAssembly());

            SessionEncodingKey = Encoding.UTF8.GetBytes(GetSessionEncodingKey());

            app.BindIsAlive(GetEnvVariables());

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.BindDebugMiddleware();

            app.UseMiddleware<ExceptionLogMiddleware>();

            app.UseCors(builder =>
            {
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowAnyOrigin();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    Console.WriteLine($"Receive request to /ws. context.WebSockets.IsWebSocketRequest: {context.WebSockets.IsWebSocketRequest}");

                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                        {
                            await Echo(context, webSocket);
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }

            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<WalletHub>("/signalr");
            });

            app.UseFileServer();


            _myNoSqlClient.Start();
            _serviceBusClient.Start();
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            //var buffer = new byte[1024 * 4];
            //WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            var rnd = new Random();

            var prices = new Dictionary<string, decimal>()
            {
                { "BTCUSD", 50000m },
                { "ETHUSD", 1800m },
                { "LTCUSD", 200m },
                { "BTCEUR", 49000m },
                { "ETHEUR", 1600m },
                { "LTCEUR", 180m }
            };

            var isActive = true;
            while (isActive)
            {
                try
                {
                    foreach (var symbol in prices.Keys.ToList())
                    {
                        // 0.00001
                        // 0.00050
                        // 0.00100
                        var d = rnd.Next(100)/10000m - 0.0050m;

                        var price = prices[symbol];

                        price = price + price * d;
                        price = Math.Round(price, 4);
                        prices[symbol] = price;

                        var msg = JsonConvert.SerializeObject(new { S = symbol, P = price, Ts = DateTime.UtcNow });
                        var buf = Encoding.UTF8.GetBytes(msg);
                        await webSocket.SendAsync(buf, WebSocketMessageType.Text, true, CancellationToken.None);
                    }

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    isActive = false;
                }
            }

            //await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
            builder.RegisterModule(new ClientsModule(_myNoSqlClient));
            builder.RegisterModule(new ServiceBusModule(_serviceBusClient));
        }

        private static IDictionary<string, string> GetEnvVariables()
        {
            var autoLoginKey = GetSessionEncodingKey();

            return new Dictionary<string, string>
            {
                { SessionEncodingKeyEnv, autoLoginKey.EncodeToSha1().ToHexString() }
            };
        }

        private static string GetSessionEncodingKey()
        {
            var key = Environment.GetEnvironmentVariable(SessionEncodingKeyEnv);
            
            if (string.IsNullOrEmpty(key))
                throw new Exception($"Env Variable {SessionEncodingKeyEnv} is not found");

            return key;
        }
    }
}
