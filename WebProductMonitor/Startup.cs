using Owin;

namespace WebProductMonitor
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
		    app
                .UseStaticFiles("/Scripts", "../../Scripts")
                .UseStaticFiles("/App", "../../App")
                .MapSignalR()
                .UseNancy();
		}
	}
}
