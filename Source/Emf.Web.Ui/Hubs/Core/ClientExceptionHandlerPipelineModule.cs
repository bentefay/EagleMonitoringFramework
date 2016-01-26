using Microsoft.AspNet.SignalR.Hubs;

namespace Emf.Web.Ui.Hubs.Core
{
    public class ClientExceptionHandlerPipelineModule : HubPipelineModule
    {
        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            dynamic caller = invokerContext.Hub.Clients.Caller;
            caller.ExceptionHandler(exceptionContext.Error.Message);
        }
    }

    
}