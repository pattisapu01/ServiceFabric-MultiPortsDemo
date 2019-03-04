using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebGateway.Domain.Interfaces;
using WebGateway.Domain.Models;

namespace WebGateway
{
    internal sealed class WebGateway : StatefulService, IWebGatewayService
    {
        public WebGateway(StatefulServiceContext context)
            : base(context)
        { }

        public Task<PingResult> Ping()
        {
            return Task.FromResult(new PingResult
            {
                Message = $"Ping recived @ {DateTime.Now} " +
                $"on Partition Id # {((NamedPartitionInformation)this.Partition.PartitionInfo).Name} " +
                $"and Version # {this.Context.CodePackageActivationContext.CodePackageVersion}",
                ReplicaId = this.Context.ReplicaOrInstanceId
            });
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            
            //return new ServiceReplicaListener[]
            //{
            //    new ServiceReplicaListener(serviceContext =>
            //        new KestrelCommunicationListener(serviceContext, (url, listener) =>
            //        {
            //            ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

            //            return new WebHostBuilder()
            //                        .UseKestrel()
            //                        .ConfigureServices(
            //                            services => services
            //                                .AddSingleton<StatefulServiceContext>(serviceContext)
            //                                .AddSingleton<IReliableStateManager>(this.StateManager))
            //                        .UseContentRoot(Directory.GetCurrentDirectory())
            //                        .UseStartup<Startup>()
            //                        .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl)
            //                        .UseUrls(url)
            //                        .Build();
            //        }))
            //};
            var settings = new FabricTransportRemotingListenerSettings
            {
                MaxConcurrentCalls = 100,
                MaxMessageSize = 1000000
            };
            return new ServiceReplicaListener[]
            {
            new ServiceReplicaListener((context) => new FabricTransportServiceRemotingListener(context,this,settings),"ServiceEndpointRemotingListener"),
            new ServiceReplicaListener(serviceContext =>
                new HttpSysCommunicationListener(serviceContext, "ServiceEndpointHttpListener", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting WebListener on {url}");
                        return new WebHostBuilder()
                        .UseHttpSys(
                        options =>
                            {
                                options.Authentication.Schemes = AuthenticationSchemes.Negotiate; // Microsoft.AspNetCore.Server.HttpSys
                                        options.Authentication.AllowAnonymous = false;
                                        /* Additional options */
                                        //options.MaxConnections = 100;
                                        //options.MaxRequestBodySize = 30000000;
                                        //options.UrlPrefixes.Add("http://localhost:5000");
                            })
                        .ConfigureServices(
                                services => services
                                    .AddSingleton<StatefulServiceContext>(serviceContext))
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseStartup<Startup>()
                        .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                        .UseUrls(url)
                        .Build();
                    }
                ), "ServiceEndpointHttpListener"
            )
            };
        }
    }
}
