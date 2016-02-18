using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;

namespace Emf.Web.Ui.Hubs.Core
{
    public abstract class SubscriptionManager<TClient, TSubscribeParameters> : IDisposable, IHubManager<TClient>
        where TClient : class, ISubscriptionHubClient
        where TSubscribeParameters : class, IEquatable<TSubscribeParameters>
    {
        private readonly Dictionary<string, ConnectionSubscriptions> _subscriptionsByConnectionId = new Dictionary<string, ConnectionSubscriptions>();
        private readonly ISubscriptionFactory<TClient, TSubscribeParameters> _subscriptionFactory;

        protected SubscriptionManager(ISubscriptionFactory<TClient, TSubscribeParameters> subscriptionFactory)
        {
            _subscriptionFactory = subscriptionFactory;
        }

        public void OnUserConnected(Hub<TClient> hub)
        {
            hub.Clients.Caller.InitializeSubscriptions();
        }

        public void OnUserReconnected(Hub<TClient> hub)
        {
            hub.Clients.Caller.InitializeSubscriptions();
        }
        
        public void OnUserSubscribing(Hub<TClient> hub, int subscriptionId, TSubscribeParameters parameters)
        {
            var newSubscription = _subscriptionFactory.CreateSubscription(hub.Clients.Caller, parameters);

            var existingSubscriptions = GetClientSubscriptions(hub.Context.ConnectionId);

            lock (existingSubscriptions)
            {
                if (existingSubscriptions.Subscriptions.ContainsKey(subscriptionId))
                    throw new InvalidOperationException($"Subscription already exists for subscriptionId '{subscriptionId}'");

                existingSubscriptions.Subscriptions.Add(subscriptionId, newSubscription);

                if (existingSubscriptions.Deleted)
                {
                    existingSubscriptions.Deleted = false;

                    lock (_subscriptionsByConnectionId)
                        _subscriptionsByConnectionId.Add(hub.Context.ConnectionId, existingSubscriptions);
                }
            }
        }

        private ConnectionSubscriptions GetClientSubscriptions(string connectionId)
        {
            lock (_subscriptionsByConnectionId)
                return _subscriptionsByConnectionId.GetValueOrAdd(connectionId, () => new ConnectionSubscriptions(connectionId));
        }

        public void OnUserUnsubscribing(Hub<TClient> hub, int subscriptionId)
        {
            var existingSubscriptions = GetClientSubscriptions(hub.Context.ConnectionId);

            RemoveClientSubscriptions(existingSubscriptions, hub.Context.ConnectionId, new[] { subscriptionId });
        }

        private void RemoveClientSubscriptions(ConnectionSubscriptions existingSubscriptions, string connectionId, IEnumerable<int> subscriptionIds = null)
        {
            lock (existingSubscriptions)
            {
                if (existingSubscriptions.Deleted)
                {
                    return;
                }

                foreach (var subscriptionId in subscriptionIds ?? existingSubscriptions.Subscriptions.Keys.ToArray())
                {
                    existingSubscriptions.Subscriptions.GetValueAndDo(subscriptionId, oldSubscription => oldSubscription.Dispose());

                    existingSubscriptions.Subscriptions.Remove(subscriptionId);
                }

                if (existingSubscriptions.Subscriptions.Count == 0)
                {
                    existingSubscriptions.Deleted = true;

                    lock (_subscriptionsByConnectionId)
                        _subscriptionsByConnectionId.Remove(connectionId);
                }
            }
        }

        public void OnUserDisconnected(Hub<TClient> hub)
        {
            var subscriptions = GetClientSubscriptions(hub.Context.ConnectionId);

            RemoveClientSubscriptions(subscriptions, hub.Context.ConnectionId);
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptionsByConnectionId.Values)
                subscription.Dispose();
        }
    }

    public class ConnectionSubscriptions : IDisposable
    {
        public ConnectionSubscriptions(string connectionId)
        {
            ConnectionId = connectionId;
        }

        public bool Deleted { get; set; }
        public string ConnectionId { get; }
        public Dictionary<int, IDisposable> Subscriptions { get; } = new Dictionary<int, IDisposable>();

        public void Dispose()
        {
            foreach (var subscription in Subscriptions)
                subscription.Value.Dispose();
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
