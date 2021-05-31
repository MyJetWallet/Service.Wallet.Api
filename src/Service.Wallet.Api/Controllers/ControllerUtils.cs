using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Domain;
using MyJetWallet.Sdk.Authorization.Http;
using Newtonsoft.Json;
using Service.Wallet.Api.Domain.Wallets;

namespace Service.Wallet.Api.Controllers
{
    public static class ControllerUtils
    {
        public static IWalletService WalletService { get; set; }

        /// <summary>
        /// PrintToken
        /// </summary>
        /// <param name="tokenString"></param>
        /// <returns></returns>
        public static string PrintToken(this string tokenString)
        {
            try
            {
                var (result, token) = MyControllerBaseHelper.ParseToken(tokenString);

                return JsonConvert.SerializeObject(token);
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

            return httpRequest?.HttpContext.Connection.RemoteIpAddress?.ToString();
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

        public static JetClientIdentity GetClientIdentity(this ControllerBase controller)
        {
            var id = new JetClientIdentity(controller.GetBrokerId(), controller.GetBrandId(), controller.GetClientId());
            return id;
        }

    }
}