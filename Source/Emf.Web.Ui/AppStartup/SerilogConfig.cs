using Serilog;

namespace Emf.Web.Ui.AppStartup
{
    internal class SerilogConfig
    {
        public static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich
                .FromLogContext()
                .WriteTo.ColoredConsole()
                .CreateLogger();
        }
    }
}
