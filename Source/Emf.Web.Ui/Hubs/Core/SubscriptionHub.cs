using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Serilog;
using Serilog.Context;

namespace Emf.Web.Ui.Hubs.Core
{
    public abstract class SubscriptionHub<TClient> : Hub<TClient>
        where TClient : class
    {
        private static readonly ILogger _logger = Log.ForContext<SubscriptionHub<TClient>>();
        private static readonly HashSet<string> _activeConnectionIds = new HashSet<string>();
        private readonly IHubManager<TClient> _subscriptionManager;

        protected SubscriptionHub(IHubManager<TClient> subscriptionManager)
        {
            _subscriptionManager = subscriptionManager;
        }

        protected IDisposable LogConnectionId()
        {
            return LogContext.PushProperty("ConnectionId", Context.ConnectionId);
        }

        public override Task OnConnected()
        {
            using (LogConnectionId())
            {
                _subscriptionManager.OnUserConnected(this);

                _logger.Debug("Client {ConnectionId} connected", Context.ConnectionId);

                UpdateConnectionCount(connected: true);

                return base.OnConnected();
            }
        }

        public override Task OnReconnected()
        {
            _subscriptionManager.OnUserReconnected(this);

            _logger.Debug("Client {connectionId} reconnected", Context.ConnectionId);

            UpdateConnectionCount(connected: true);

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _subscriptionManager.OnUserDisconnected(this);

            _logger.Debug(stopCalled ?
                "Client {connectionId} explicitly closed the connection" :
                "Client {connectionId} timed out", Context.ConnectionId);

            UpdateConnectionCount(connected: false);

            return base.OnDisconnected(stopCalled);
        }

        private void UpdateConnectionCount(bool connected)
        {
            lock (_activeConnectionIds)
            {
                if (connected)
                    _activeConnectionIds.Add(Context.ConnectionId);
                else
                    _activeConnectionIds.Remove(Context.ConnectionId);
            }
        }
    }

    public class SubscriptionId : IEquatable<SubscriptionId>
    {
        public static readonly SubscriptionId Default = new SubscriptionId("Default");

        public SubscriptionId(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public bool Equals(SubscriptionId other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SubscriptionId);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public abstract class SubscriptionHub<TClient, TSubscribeParameters> : SubscriptionHub<TClient>
        where TClient : class, ISubscriptionHubClient
        where TSubscribeParameters : class, IEquatable<TSubscribeParameters>
    {
        private readonly SubscriptionManager<TClient, TSubscribeParameters> _subscriptionManager;

        protected SubscriptionHub(SubscriptionManager<TClient, TSubscribeParameters> subscriptionManager) : base(subscriptionManager)
        {
            _subscriptionManager = subscriptionManager;
        }

        public void Unsubscribe(SubscriptionId subscriptionId)
        {
            using (LogConnectionId())
            {
                _subscriptionManager.OnUserUnsubscribing(this, subscriptionId);
            }
        }

        public SubscriptionId Subscribe(TSubscribeParameters parameters)
        {
            using (LogConnectionId())
            {
                return _subscriptionManager.OnUserSubscribing(this, parameters);
            }
        }
    }
}