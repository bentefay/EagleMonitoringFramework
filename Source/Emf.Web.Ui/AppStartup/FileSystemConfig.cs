using System.IO;
using System.Linq;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Serilog;

namespace Emf.Web.Ui.AppStartup
{
    public static class FileSystemConfig
    {
        // Serve files out of the first directory that exists
        private static readonly string[] _relativePathsToRoot = { "./public", "../../public" };

        public static void Initialize(IAppBuilder app)
        {
            var relativePathToRoot = _relativePathsToRoot.First(Directory.Exists);
            var pathToRoot = Path.GetFullPath(relativePathToRoot);
            Log.Information("Serving files from root: {fullPath}", pathToRoot);

            app.UseFileServer(new FileServerOptions
            {
                EnableDirectoryBrowsing = false,
                FileSystem = new PhysicalFileSystem(relativePathToRoot),
                StaticFileOptions = { OnPrepareResponse = context =>
                {
                    var filePath = context.File.PhysicalPath;

                    if (filePath == null)
                        return;

                    if (filePath.StartsWith(pathToRoot))
                        filePath = $".{filePath.Substring(pathToRoot.Length)}";

                    Log.Information("Serving: {fileName}", filePath);
                }
                }
            });
        }
    }
}
