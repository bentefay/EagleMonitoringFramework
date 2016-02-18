using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Serilog;
using Serilog.Context;

namespace Emf.Web.Ui.Hubs.Core
{
    public abstract class SubscriptionHub<TClient, TSubscribeParameters> : Hub<TClient>
            where TClient : class, ISubscriptionHubClient
            where TSubscribeParameters : class, IEquatable<TSubscribeParameters>

    {
        private static readonly ILogger _logger = Log.ForContext<SubscriptionHub<TClient, TSubscribeParameters>>();
        private static readonly HashSet<string> _activeConnectionIds = new HashSet<string>();
        private readonly SubscriptionManager<TClient, TSubscribeParameters> _subscriptionManager;

        protected SubscriptionHub(SubscriptionManager<TClient, TSubscribeParameters> subscriptionManager)
        {
            _subscriptionManager = subscriptionManager;
        }

        public void Subscribe(int subscriptionId, TSubscribeParameters parameters)
        {
            using (ConnectionIdInLogContext())
            {
                _subscriptionManager.OnUserSubscribing(this, subscriptionId, parameters);
            }
        }

        public void Unsubscribe(int subscriptionId)
        {
            using (ConnectionIdInLogContext())
            {
                _subscriptionManager.OnUserUnsubscribing(this, subscriptionId);
            }
        }

        protected IDisposable ConnectionIdInLogContext()
        {
            return LogContext.PushProperty("ConnectionId", Context.ConnectionId);
        }

        public override Task OnConnected()
        {
            using (ConnectionIdInLogContext())
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
}