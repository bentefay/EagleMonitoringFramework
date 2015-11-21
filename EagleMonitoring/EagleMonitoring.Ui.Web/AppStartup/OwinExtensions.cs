using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.Owin.Builder;
using Owin;

namespace EagleMonitoring.Ui.Web.AppStartup
{
    public static class OwinExtensions
    {
        public static void UseSignalR(this IApplicationBuilder app)
        {
            app.UseOwin(addToPipeline =>
            {
                addToPipeline(next =>
                {
                    var appBuilder = new AppBuilder();
                    appBuilder.Properties[OwinConstants.BuilderDefaultApp] = next;
                    appBuilder.MapSignalR();

                    return appBuilder.Build<Func<IDictionary<string, object>, Task>>();
                });
            });
        }
    }

    public static class OwinConstants
    {
        internal const string BuilderDefaultApp = "builder.DefaultApp";
    }
}