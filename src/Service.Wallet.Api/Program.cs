﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.Wallet.Api.Settings;
using SimpleTrading.SettingsReader;

namespace Service.Wallet.Api
{
    public class Program
    {
        public const string SettingsFileName = ".myjetwallet";

        public static SettingsModel Settings { get; private set; }

        public static Func<T> ReloadedSettings<T>(Func<SettingsModel, T> getter)
        {
            return () =>
            {
                var settings = SettingsReader.ReadSettings<SettingsModel>(SettingsFileName);
                var value = getter.Invoke(settings);
                return value;
            };
        }


        public static void Main(string[] args)
        {
            Console.Title = "MyJetWallet Service.Wallet.Api";

            Settings = SettingsReader.ReadSettings<SettingsModel>(SettingsFileName);

            using var loggerFactory = LogConfigurator.Configure("MyJetWallet", Settings.SeqServiceUrl);

            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                logger.LogInformation("Application is being started");

                CreateHostBuilder(loggerFactory, args).Build().Run();

                logger.LogInformation("Application has been stopped");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Application has been terminated unexpectedly");
            }
        }

        public static IHostBuilder CreateHostBuilder(ILoggerFactory loggerFactory, string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        Console.WriteLine("Http port: 8080");
                        options.Listen(IPAddress.Any, 8080, o => o.Protocols = HttpProtocols.Http1);
                    });

                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(loggerFactory);
                    services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                });
    }
}
