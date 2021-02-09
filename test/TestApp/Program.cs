using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Service.Wallet.Api.Hubs;
using Service.Wallet.Api.Hubs.Dto;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:8080/signalr")
                .AddMessagePackProtocol()
                .Build();


            connection.Closed += async (error) =>
            {
                Console.WriteLine($"Connection is closed. Exception: {error.ToString()}");
            };

            connection.Reconnected += async (s) =>
            {
                Console.WriteLine($"Connection is Reconnected. ConnectionId: {s}");
            };

            connection.Reconnecting += async (error) =>
            {
                Console.WriteLine($"Connection is Reconnected. Exception: {error}");
            };
            
            connection.On<WelcomeMessage>(HubNames.Welcome, (message) =>
            {
                Console.WriteLine($"--> [{HubNames.Welcome}] {message.Data}");
            });

            

            await connection.StartAsync();

            await connection.InvokeAsync("Init", "Alex");
            


            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
            Console.ReadLine();
        }
    }
}
