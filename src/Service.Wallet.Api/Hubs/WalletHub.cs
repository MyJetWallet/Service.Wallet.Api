using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Service.Wallet.Api.Hubs.Dto;

namespace Service.Wallet.Api.Hubs
{
    /// <summary>
    /// Signal-R hub to send changes
    /// </summary>
    public class WalletHub: Hub
    {
        private readonly ILogger<WalletHub> _logger;

        internal static readonly HubClientConnections HubConnections = new HubClientConnections();
        
        public const string AccessTokenParamName = "access_token";

        public WalletHub(ILogger<WalletHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();

            _logger.LogInformation("HUB [WalletHub] is connected. ConnectionId: {ConnectionId}", httpContext.Connection.Id);

            if (httpContext.Request.Query.ContainsKey(AccessTokenParamName))
            {
                var token = httpContext.Request.Query[AccessTokenParamName];
                if (!string.IsNullOrEmpty(token))
                    await Init(token);
            }

            await base.OnConnectedAsync();
        }

        [SignalRIncomingRequest]
        public async Task Init(string token)
        {
            Console.WriteLine($"--> [Init] {token}");

            var ctx = new HubClientConnection(Context, Clients.Caller, token);

            HubConnections.Connected(ctx);

            var message = WelcomeMessage.Create($"Hello {token}");

            await Clients.Caller.SendAsync(HubNames.Welcome, message);
        }

        




        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var httpContext = Context.GetHttpContext();
            _logger.LogInformation("HUB [WalletHub] is disconnected. ConnectionId: {ConnectionId}. Exception: {}", httpContext?.Connection?.Id, exception?.ToString());

            await base.OnDisconnectedAsync(exception);
        }
    }
}