using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;

namespace Emf.Web.Ui.Hubs.Core
{
    public class ConnectionSubscriptions : IDisposable
    {
        public ConnectionSubscriptions(string connectionId)
        {
            ConnectionId = connectionId;
        }

        public string ConnectionId { get; }
        public Dictionary<SubscriptionId, IDisposable> Subscriptions { get; } = new Dictionary<SubscriptionId, IDisposable>();

        public void Dispose()
        {
            foreach (var subscription in Subscriptions)
                subscription.Value.Dispose();
        }
    }

    public abstract class BaseSubscriptionManager<TClient> : IDisposable, IHubManager<TClient>
        where TClient : class
    {
        protected readonly Dictionary<string, ConnectionSubscriptions> SubscriptionsByConnectionId = new Dictionary<string, ConnectionSubscriptions>();

        public void OnUserConnected(Hub<TClient> hub)
        {
            OnUserConnected(hub, reconnected: false);
        }

        public void OnUserReconnected(Hub<TClient> hub)
        {
            OnUserConnected(hub, reconnected: true);
        }

        protected abstract void OnUserConnected(Hub<TClient> hub, bool reconnected);

        protected void AddSubscriptions(Hub<TClient> hub, SubscriptionId subscriptionId, IDisposable subscription)
        {
            lock (SubscriptionsByConnectionId)
            {
                var existingSubscriptions = SubscriptionsByConnectionId.GetValueOrAdd(hub.Context.ConnectionId, () => new ConnectionSubscriptions(hub.Context.ConnectionId));

                existingSubscriptions.Subscriptions.GetValueAndDo(subscriptionId, oldSubscription => oldSubscription.Dispose());

                existingSubscriptions.Subscriptions.Add(subscriptionId, subscription);
            }
        }

        public void OnUserDisconnected(Hub<TClient> hub)
        {
            lock (SubscriptionsByConnectionId)
            {
                SubscriptionsByConnectionId.GetValueAndDo(hub.Context.ConnectionId, subscriptions => subscriptions.Dispose());
                SubscriptionsByConnectionId.Remove(hub.Context.ConnectionId);
            }
        }

        public void Dispose()
        {
            foreach (var subscription in SubscriptionsByConnectionId.Values)
                subscription.Dispose();
        }
    }

    public abstract class SubscriptionManager<TClient> : BaseSubscriptionManager<TClient>
        where TClient : class
    {
        private readonly ISubscriptionService<TClient> _subscriptionService;

        protected SubscriptionManager(ISubscriptionService<TClient> subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        protected override void OnUserConnected(Hub<TClient> hub, bool reconnected)
        {
            AddSubscriptions(hub, _subscriptionService.CreateSubscription(hub.Clients.Caller));
        }
    }

    public abstract class SubscriptionManager<TClient, TSubscribeParameters> : BaseSubscriptionManager<TClient>
        where TClient : class, ISubscriptionHubClient
        where TSubscribeParameters : class, IEquatable<TSubscribeParameters>
    {
        private readonly ISubscriptionFactory<TClient, TSubscribeParameters> _subscriptionFactory;

        protected SubscriptionManager(ISubscriptionFactory<TClient, TSubscribeParameters> subscriptionFactory)
        {
            _subscriptionFactory = subscriptionFactory;
        }
        
        protected override void OnUserConnected(Hub<TClient> hub, bool reconnected)
        {
            hub.Clients.Caller.InitializeSubscriptions();
        }

        public void OnUserSubscribing(Hub<TClient> hub, TSubscribeParameters parameters)
        {
            AddSubscriptions(hub, _subscriptionFactory.CreateSubscription(hub.Clients.Caller, parameters));
        }

        public void OnUserUnsubscribing(SubscriptionHub<object, object> subscriptionHub, IEquatable<object> parameters)
        {
            throw new NotImplementedException();
        }
    }

    public static class DictionaryExtensions
    {
        public static TValue GetValueOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> factory)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
                return value;

            var newValue = factory();
            dictionary.Add(key, newValue);
            return newValue;
        }

        public static void GetValueAndDo<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Action<TValue> action)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
                action(value);
        }
    }
}
