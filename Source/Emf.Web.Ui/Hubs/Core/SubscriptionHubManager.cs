using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.SignalR;

namespace Emf.Web.Ui.Hubs.Core
{
    public abstract class BaseSubscriptionHubManager<TClient> : IDisposable, IHubManager<TClient>
        where TClient : class
    {
        protected readonly ConcurrentDictionary<string, IDisposable> _subscriptions = new ConcurrentDictionary<string, IDisposable>();

        public void OnUserConnected(Hub<TClient> hub)
        {
            OnUserConnected(hub, reconnected: false);
        }

        public void OnUserReconnected(Hub<TClient> hub)
        {
            OnUserConnected(hub, reconnected: true);
        }

        protected abstract void OnUserConnected(Hub<TClient> hub, bool reconnected);

        protected void AddSubscriptions(Hub<TClient> hub, IDisposable subscriptions)
        {
            _subscriptions.AddOrUpdate(hub.Context.ConnectionId, subscriptions, (key, existingSubscriptions) =>
            {
                existingSubscriptions.Dispose();
                return subscriptions;
            });
        }

        public void OnUserDisconnected(Hub<TClient> hub)
        {
            IDisposable subscriptions;

            if (_subscriptions.TryRemove(hub.Context.ConnectionId, out subscriptions))
                subscriptions.Dispose();
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions.Values)
                subscription.Dispose();
        }
    }

    public abstract class SubscriptionHubManager<TClient> : BaseSubscriptionHubManager<TClient>
        where TClient : class
    {
        private readonly ISubscriptionService<TClient> _subscriptionService;

        protected SubscriptionHubManager(ISubscriptionService<TClient> subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        protected override void OnUserConnected(Hub<TClient> hub, bool reconnected)
        {
            AddSubscriptions(hub, _subscriptionService.CreateSubscription(hub.Clients.Caller));
        }
    }

    public abstract class SubscriptionHubManager<TClient, TSubscribeParameters> : BaseSubscriptionHubManager<TClient>
        where TClient : class, ISubscriptionHubClient
        where TSubscribeParameters : class, IEquatable<TSubscribeParameters>
    {
        private readonly ISubscriptionService<TClient, TSubscribeParameters> _subscriptionService;

        protected SubscriptionHubManager(ISubscriptionService<TClient, TSubscribeParameters> subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }
        
        protected override void OnUserConnected(Hub<TClient> hub, bool reconnected)
        {
            hub.Clients.Caller.InitializeSubscriptions();
        }

        public void OnUserSubscribing(Hub<TClient> hub, TSubscribeParameters parameters)
        {
            AddSubscriptions(hub, _subscriptionService.CreateSubscription(hub.Clients.Caller, parameters));
        }
    }
}
