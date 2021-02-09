using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using NSwag;
using Service.Wallet.Api.Hubs;

namespace Service.Wallet.Api
{
    public static class StartupUtils
    {
        /// <summary>
        /// Setup swagger ui ba
        /// </summary>
        /// <param name="services"></param>
        public static void SetupSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerDocument(o =>
            {
                o.Title = "MyJetWallet API";
                o.GenerateEnumMappingDescription = true;

                o.AddSecurity("Bearer", Enumerable.Empty<string>(),
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Description = "Bearer Token",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Name = "Authorization"
                    });
            });
        }

        /// <summary>
        /// Headers settings
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigurateHeaders(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        /// <summary>
        /// Bind debug middleware
        /// </summary>
        /// <param name="app"></param>
        public static void BindDebugMiddleware(this IApplicationBuilder app)
        {
            app.Use((context, next) =>
            {
                if (context.Request.Path.Value == "/signalr-doc")
                {
                    context.Response.ContentType = "text/html";
                    return context.Response.WriteAsync(
                        SignalrDocumentationGenerator.GenerateDocumentation(typeof(WalletHub))
                        );
                }

                if (context.Request.Path.Value == "/reset-cache")
                {
                    //foreach (var tradingEngineServiceLocator in ServiceLocator.LiveDemo.Values)
                    //{
                    //    tradingEngineServiceLocator.ActivePositions.ClearCache();
                    //    tradingEngineServiceLocator.PendingOrders.ClearCache();
                    //}

                    return context.Response.WriteAsync("All Cache data is reset now");
                }

                if (context.Request.Path.Value == "/debug-user")
                {
                    //ServiceLocator.DebugTraderId = context.Request.Query["id"];
                    //return context.Response.WriteAsync("Debug user is " + ServiceLocator.DebugTraderId);

                    return context.Response.WriteAsync("Show user data");
                }

                return next.Invoke();
            });
        }
    }
}