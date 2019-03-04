using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using WebGateway.Domain.Models;

namespace WebGateway.Domain.Interfaces
{
    public interface IWebGatewayService : IService
    {
        Task<PingResult> Ping();
    }
}
