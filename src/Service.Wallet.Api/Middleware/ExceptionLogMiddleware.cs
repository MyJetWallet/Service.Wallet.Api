using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
                _logger.LogInformation(ex, "Receive WalletApiHttpException with status code: {StatusCode}; path: {Path}", ex.StatusCode, context.Request.Path);

                context.Response.StatusCode = (int) ex.StatusCode;
                await context.Response.WriteAsJsonAsync(new {ex.Message});
            }
            catch (WalletApiErrorException ex)
            {
                _logger.LogInformation(ex, "Receive WalletApiErrorException with status code: {codeText}; path: {Path}", ex.Code.ToString(), context.Request.Path);

                context.Response.StatusCode = (int) HttpStatusCode.OK;
                await context.Response.WriteAsJsonAsync(new Response(ex.Code));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

    }
}