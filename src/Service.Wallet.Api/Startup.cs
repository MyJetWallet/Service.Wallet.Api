using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac;
using Microsoft.Extensions.Configuration;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
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
        private MyNoSqlTcpClient _myNoSqlClient;
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

            services.AddSignalR(option =>
            {
                option.EnableDetailedErrors = true;

            }).AddMessagePackProtocol();
            
            services
                .AddAuthentication(o => { o.DefaultScheme = "Bearer"; })
                .AddScheme<WalletAuthenticationOptions, WalletAuthHandler>("Bearer", o => { });

            //services.AddSingleton<WalletAuthHandler>();

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


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<WalletHub>("/signalr");
            });


            _myNoSqlClient.Start();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
            builder.RegisterModule(new ClientsModule(_myNoSqlClient));
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
