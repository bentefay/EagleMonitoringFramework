using Microsoft.AspNet.SignalR;

namespace Emf.Web.Ui.Hubs.Core
{
    public interface IHubManager<TClient> where TClient : class
    {
        void OnUserConnected(Hub<TClient> hub);
        void OnUserReconnected(Hub<TClient> hub);
        void OnUserDisconnected(Hub<TClient> hub);
    }
}