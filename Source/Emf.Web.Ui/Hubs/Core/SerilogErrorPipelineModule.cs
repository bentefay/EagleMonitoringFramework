using Microsoft.AspNet.SignalR.Hubs;
using Serilog;
using Serilog.Context;

namespace Emf.Web.Ui.Hubs.Core
{
    public class SerilogErrorPipelineModule : HubPipelineModule
    {
        private static readonly ILogger _logger = Log.ForContext<SerilogErrorPipelineModule>();

        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            using (LogContext.PushProperty("ConnectionId", invokerContext.Hub.Context.ConnectionId))
            {
                _logger.Error(exceptionContext.Error, "An error occurred on SignalR pipeline while calling {Name} {1}", invokerContext.MethodDescriptor.Name);
            }
        }
    }
}