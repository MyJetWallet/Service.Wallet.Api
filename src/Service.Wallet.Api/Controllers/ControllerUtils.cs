using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SimpleTrading.TokensManager;
using SimpleTrading.TokensManager.Tokens;

namespace Service.Wallet.Api.Controllers
{
    public static class ControllerUtils
    {
        /// <summary>
        /// PrintToken
        /// </summary>
        /// <param name="tokenString"></param>
        /// <returns></returns>
        public static string PrintToken(this string tokenString)
        {
            try
            {
                var (result, token) = TokensManager.ParseBase64Token<AuthorizationToken>(tokenString, Startup.SessionEncodingKey, DateTime.UtcNow);

                return JsonConvert.SerializeObject(new
                {
                    TraderId = token.Id,
                    Expires = token.Expires.ToString("s"),
                });
            }
            catch (Exception)
            {
                return "Invalid token";
            }
        }

        private static readonly string[] IpHeaders =
        {
            "CF-Connecting-IP", "X-Forwarded-For"
        };

        private static string GetIp(this HttpRequest httpRequest)
        {
            foreach (var ipHeader in IpHeaders)
            {
                if (httpRequest.Headers.ContainsKey(ipHeader))
                    return httpRequest.Headers[ipHeader].ToString();
            }

            return httpRequest.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        /// <summary>
        /// Get Ip of request
        /// </summary>
        /// <param name="ctx">Request context</param>
        /// <returns></returns>
        public static string GetIp(this HttpContext ctx)
        {
            return ctx.Request.GetIp();
        }

        /// <summary>
        /// User agent of request
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string GetUserAgent(this HttpContext ctx)
        {
            try
            {
                return ctx.Request.Headers["User-Agent"];
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get scheme of request
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static string GetScheme(this HttpContext ctx)
        {
            const string headerName = "X-Forwarded-Proto";
            if (ctx.Request.Headers.ContainsKey(headerName))
                return ctx.Request.Headers[headerName];
            return ctx.Request.Scheme;
        }
    }
}