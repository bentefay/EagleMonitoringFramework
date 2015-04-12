using Eagle.Server.UI.Web.Services;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Owin;

namespace Eagle.Server.UI.Web
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCaseJsonContractResolver()
            };
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
