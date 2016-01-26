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
        private readonly IHubManager<TClient> _manager;

        protected SubscriptionHub(IHubManager<TClient> manager)
        {
            _manager = manager;
        }

        protected IDisposable LogConnectionId()
        {
            return LogContext.PushProperty("ConnectionId", Context.ConnectionId);
        }

        public override Task OnConnected()
        {
            using (LogConnectionId())
            {
                _manager.OnUserConnected(this);

                _logger.Debug("Client {ConnectionId} connected", Context.ConnectionId);

                UpdateConnectionCount(connected: true);

                return base.OnConnected();
            }
        }

        public override Task OnReconnected()
        {
            _manager.OnUserReconnected(this);

            _logger.Debug("Client {connectionId} reconnected", Context.ConnectionId);

            UpdateConnectionCount(connected: true);

            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _manager.OnUserDisconnected(this);

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

    public abstract class SubscriptionHub<TClient, TSubscribeParameters> : SubscriptionHub<TClient>
        where TClient : class, ISubscriptionHubClient
        where TSubscribeParameters : class, IEquatable<TSubscribeParameters>
    {
        private readonly SubscriptionHubManager<TClient, TSubscribeParameters> _manager;

        protected SubscriptionHub(SubscriptionHubManager<TClient, TSubscribeParameters> manager) : base(manager)
        {
            _manager = manager;
        }

        public void Subscribe(TSubscribeParameters parameters)
        {
            using (LogConnectionId())
            {
                _manager.OnUserSubscribing(this, parameters);
            }
        }
    }
}