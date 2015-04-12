using System.Collections.Generic;
using Microsoft.AspNet.SignalR;

namespace Eagle.Server.UI.Web.Hubs
{
	public class ProductMonitorHub : Hub
	{
	    public IEnumerable<CheckDisplayDto> GetChecks()
	    {
	        return ProductMonitorHubService.Instance.Checks;
	    }
	}
}