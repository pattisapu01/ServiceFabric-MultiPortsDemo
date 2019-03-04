using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebGateway.Domain.Interfaces;

namespace Client
{
    class Program
    {
        private static readonly ServiceProxyFactory ServiceProxyFactory = GetServiceProxyFactory();

        static void Main(string[] args)
        {
            Task.WaitAll(Main());
        }

        static async Task Main()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            long counter = 0;
            long previous = -1;            
            var proxy = ServiceProxyFactory.CreateServiceProxy<IWebGatewayService>(new Uri("fabric:/ServiceFabricDemo/WebGateway"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey("0"), TargetReplicaSelector.PrimaryReplica, "ServiceEndpointRemotingListener");
            while (true)
            {
                if (counter++ % 20 == 0)
                {
                    Console.Clear();
                    //counter = 0;
                }

                try
                {
                    // TODO 6
                    var result = await proxy.Ping();
                    var current = result.ReplicaId;
                    Console.WriteLine($"{current}: {result.Message}. Called {counter} times");
                    if (previous != -1 && previous != current)
                    {
                        var currentColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"!! Failed over from {previous} to {current} !!");
                        Console.ForegroundColor = currentColor;
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                    previous = current;
                }
                catch (Exception ex)
                {
                    var currentColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("!");
                    Console.ForegroundColor = currentColor;
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            //}
        }

        public static ServiceProxyFactory GetServiceProxyFactory()
        {
            FabricTransportRemotingSettings transportSettings = new FabricTransportRemotingSettings
            {
                OperationTimeout = TimeSpan.FromSeconds(600),
                MaxMessageSize = 1000000,
                MaxConcurrentCalls = 100
            };
            var retrySettings = new OperationRetrySettings(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(3), 5);
            var clientFactory = new FabricTransportServiceRemotingClientFactory(transportSettings);
            return new ServiceProxyFactory((c) => clientFactory, retrySettings);
        }

    }
}
