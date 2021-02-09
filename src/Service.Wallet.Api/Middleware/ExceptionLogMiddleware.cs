using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

    }
}