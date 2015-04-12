using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using ProductMonitor.Framework;

namespace WebProductMonitor.Hubs
{
    public class ProductMonitorHubService
    {
        private ProductMonitorHubService()
        {
            Checks = new ICheckDisplay[0];
        }

        private readonly Lazy<dynamic> _clients = new Lazy<dynamic>(() => GlobalHost.ConnectionManager.GetHubContext<ProductMonitorHub>().Clients.All); 

        public static readonly ProductMonitorHubService Instance = new ProductMonitorHubService();

        public IReadOnlyList<ICheckDisplay> Checks { get; private set; }

        public void UpdateChecks(IReadOnlyList<ICheckDisplay> checks)
        {
            Checks = checks;
            _clients.Value.UpdateChecks(checks.Select(check => new CheckDisplayDto(check)));
        }

        public void UpdateCheck(ICheckDisplay check)
        {
            _clients.Value.UpdateChecks(new[] { new CheckDisplayDto(check) });
        }
    }
}
