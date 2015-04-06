using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace WebProductMonitor
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseStaticFiles(this IAppBuilder app, string requestPath, string fileSystemPath)
        {
            return app.UseStaticFiles(new StaticFileOptions
            {
                FileSystem = new PhysicalFileSystem(fileSystemPath),
                RequestPath = new PathString(requestPath)
            });
        }
    }
}