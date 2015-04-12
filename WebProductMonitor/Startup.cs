using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Owin;
using WebProductMonitor.Services;

namespace WebProductMonitor
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new CamelCaseJsonContractResolver();
            var serializer = JsonSerializer.Create(settings);
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);

		    app
                .UseStaticFiles("/Scripts", "../../Scripts")
                .UseStaticFiles("/App", "../../App")
                .MapSignalR()
                .UseNancy();
		}
	}
}
