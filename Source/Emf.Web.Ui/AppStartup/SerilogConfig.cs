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
#if DEBUG
                .MinimumLevel.Debug()
                .WriteTo.Seq("http://localhost:5341/") // Not required, but seq (from https://getseq.net/) is great for development logging
#else
                .WriteTo.Seq("https://seq.datastripe1.com/", apiKey: "Zr2LZbZBaK651b3s9Pqn")
#endif
                .WriteTo.ColoredConsole()
                .CreateLogger();
        }
    }
}
