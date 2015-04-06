using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.Infrastructure;
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
