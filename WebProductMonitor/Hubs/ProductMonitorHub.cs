using System.CodeDom;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using ProductMonitor.Framework;
using ProductMonitor.Framework.Generic;

namespace WebProductMonitor.Hubs
{
	public class ProductMonitorHub : Hub
	{
	    public IEnumerable<CheckDisplayDto> GetChecks()
	    {
	        return ProductMonitorHubService.Instance.Checks;
	    }
	}
}