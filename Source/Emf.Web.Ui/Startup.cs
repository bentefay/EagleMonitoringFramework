using Emf.Web.Ui.AppStartup;
using Microsoft.Owin.Cors;
using Owin;

namespace Emf.Web.Ui
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            SignalRConfig.Initialize(app);
            FileSystemConfig.Initialize(app);
            app.UseNancy();
        }        
    }
}