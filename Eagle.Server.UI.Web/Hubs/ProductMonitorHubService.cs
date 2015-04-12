using System;
using System.Collections.Generic;
using System.Linq;
using Eagle.Server.Framework;
using Microsoft.AspNet.SignalR;

namespace Eagle.Server.UI.Web.Hubs
{
    public class ProductMonitorHubService
    {
        private ProductMonitorHubService()
        {
            _checks = new ICheckDisplay[0];
        }

        private readonly Lazy<dynamic> _clients = new Lazy<dynamic>(() => GlobalHost.ConnectionManager.GetHubContext<ProductMonitorHub>().Clients.All);
        private IReadOnlyList<ICheckDisplay> _checks;

        public static readonly ProductMonitorHubService Instance = new ProductMonitorHubService();

        public IEnumerable<CheckDisplayDto> Checks
        {
            get { return _checks.Select(check => new CheckDisplayDto(check)); }
        }

        public void UpdateChecks(IReadOnlyList<ICheckDisplay> checks)
        {
            _checks = checks;
            _clients.Value.UpdateChecks(Checks);
        }

        public void UpdateCheck(ICheckDisplay check)
        {
            _clients.Value.UpdateChecks(new[] { new CheckDisplayDto(check) });
        }
    }
}
