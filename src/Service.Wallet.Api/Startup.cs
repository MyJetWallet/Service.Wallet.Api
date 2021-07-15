using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac;
using Microsoft.Extensions.Configuration;
using MyJetWallet.Sdk.Authorization.Http;
using MyJetWallet.Sdk.RestApiTrace;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.WalletApi;
using MyJetWallet.Sdk.WalletApi.Common;
using MyJetWallet.Sdk.WalletApi.Middleware;
using MyServiceBus.TcpClient;
using Prometheus;
using Service.Wallet.Api.Modules;
using SimpleTrading.BaseMetrics;
using SimpleTrading.ServiceStatusReporterConnector;
using SimpleTrading.TokensManager;

namespace Service.Wallet.Api
{
    public class Startup
    {
        private const string SessionEncodingKeyEnv = "SESSION_ENCODING_KEY";
        private const string EnvInfo = "ENV_INFO";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            StartupUtils.SetupWalletServices(services);

            services.AddHostedService<ApplicationLifetimeManager>();

            services.AddMyTelemetry("SP-", Program.Settings.ZipkinUrl);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            StartupUtils.SetupWalletApplication(app, env, Program.Settings.EnableApiTrace, "api");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
            builder.RegisterModule(new ClientsModule());
        }
    }
}
