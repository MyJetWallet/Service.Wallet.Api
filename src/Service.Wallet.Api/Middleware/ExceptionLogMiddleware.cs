using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyTcpSockets.Extensions;
using Newtonsoft.Json;
using Service.Wallet.Api.Controllers.Contracts;
using Service.Wallet.Api.Domain.Contracts;

// ReSharper disable UnusedMember.Global

namespace Service.Wallet.Api.Middleware
{
    public class ExceptionLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionLogMiddleware> _logger;

        public ExceptionLogMiddleware(RequestDelegate next, ILogger<ExceptionLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (WalletApiHttpException ex)
            {
                ex.StatusCode.AddToActivityAsTag("ErrorCode");
                ex.FailActivity();
                _logger.LogInformation(ex,"Receive WalletApiHttpException with status code: {StatusCode}; path: {Path}", ex.StatusCode, context.Request.Path);

                context.Response.StatusCode = (int) ex.StatusCode;
                await context.Response.WriteAsJsonAsync(new {ex.Message});
            }
            catch (WalletApiErrorException ex)
            {
                ex.Code.AddToActivityAsTag("ErrorCode");
                ex.FailActivity();
                _logger.LogInformation(ex, "Receive WalletApiErrorException with status code: {codeText}; path: {Path}", ex.Code.ToString(), context.Request.Path);

                context.Response.StatusCode = (int) HttpStatusCode.OK;
                await context.Response.WriteAsJsonAsync(new Response(ex.Code));
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }

    public class DebugMiddleware
    {
        private readonly RequestDelegate _next;

        public DebugMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/api/Debug/make-signature" && context.Request.Method == "POST")
            {
                var request = context.Request;

                if (!context.Request.Headers.TryGetValue("private-key", out var key))
                {
                    context.Response.StatusCode = 400;
                    return;
                }

                string bodyStr;
                using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                    Console.WriteLine(bodyStr);

                    var rsa = RSA.Create(2048);

                    rsa.ImportRSAPrivateKey(Convert.FromBase64String(key), out _);

                    var signature = rsa.SignData(Encoding.UTF8.GetBytes(bodyStr), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                    var response = new
                    {
                        Signature = Convert.ToBase64String(signature)
                    };

                    await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response)));
                }


                return;
            }

            await _next(context);
        }

    }
}