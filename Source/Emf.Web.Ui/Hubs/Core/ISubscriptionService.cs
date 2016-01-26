using System;

namespace Emf.Web.Ui.Hubs.Core
{
    public interface ISubscriptionService<in TClient>
    where TClient : class
    {
        IDisposable CreateSubscription(TClient client);
    }

    public interface ISubscriptionService<in TClient, in TSubscribeParameters>
        where TClient : class
        where TSubscribeParameters : class, IEquatable<TSubscribeParameters>
    {
        IDisposable CreateSubscription(TClient client, TSubscribeParameters parameters);
    }
}