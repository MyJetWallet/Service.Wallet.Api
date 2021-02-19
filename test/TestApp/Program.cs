using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Service.Wallet.Api.Hubs;
using Service.Wallet.Api.Hubs.Dto;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Press enter to start.");
            Console.ReadLine();

            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:8080/signalr")
                //.WithUrl("http://wallet-api.services.svc.cluster.local:8080/signalr")
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
                Console.WriteLine($"--> [{HubNames.Welcome}] {message.Data}\r\n");
            });

            connection.On<WalletListMessage>(HubNames.WalletList, message =>
            {
                Console.WriteLine($"--> [{HubNames.WalletList}] {JsonConvert.SerializeObject(message)}\r\n");
            });

            connection.On<AssetListMessage>(HubNames.AssetList, message =>
            {
                Console.WriteLine($"--> [{HubNames.AssetList}] {JsonConvert.SerializeObject(message)}\r\n");
            });

            connection.On<SpotInstrumentListMessage>(HubNames.SpotInstrumentList, message =>
            {
                Console.WriteLine($"--> [{HubNames.SpotInstrumentList}] {JsonConvert.SerializeObject(message)}\r\n");
            });

            connection.On<PongMessage>(HubNames.Pong, message =>
            {
                Console.WriteLine($"--> [{HubNames.Pong}] {JsonConvert.SerializeObject(message)}\r\n");
            });

            //connection.On<BidAskMessage>(HubNames.BidAsk, message =>
            //{
            //    foreach (var price in message.Prices)
            //    {
            //        Console.WriteLine($"--> [{HubNames.BidAsk}] {price.Id} {price.Bid} {price.Ask} {price.DateTime.TimeOfDay}\r\n");
            //    }
            //});

            connection.On<WalletBalancesMessage>(HubNames.WalletBalances, message =>
            {
                Console.WriteLine("Balances: ");
                foreach (var asset in message.Balances)
                {
                    Console.WriteLine($"  * {asset.AssetId}: {asset.Balance - asset.Reserve} ({asset.Reserve})");
                }
            });


            await connection.StartAsync();

            await connection.SendAsync(HubNames.Init, "alex");

            var run = true;

            var task = Task.Run(async () =>
            {
                while (run)
                {
                    await Task.Delay(5000);
                    await connection.SendAsync(HubNames.Ping);
                }
            });


            Console.WriteLine("Press enter to exit");
            Console.ReadLine();

            run = false;
            await task;

            Console.WriteLine("End of app");
        }
    }
}
