using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace JabbR.Client
{
    /// <summary>
    /// Interface that wraps SignalR's IClientTransport and provides a way to add authentication information
    /// </summary>
    public interface IJabbRTransport
    {
        Task<HubConnection> Connect(string userName, string password);
    }
}
