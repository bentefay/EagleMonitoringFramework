using Emf.Web.Ui.Hubs.Core;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Emf.Web.Ui.Models;
using Microsoft.AspNet.SignalR.Hubs;
using Serilog;

namespace Emf.Web.Ui.Hubs
{
    [HubName("Repositories")]
    public class ObservableRepositoryHub<TKey, TValue> : SubscriptionHub<IObservableRepositoryHubClient<TKey, TValue>, ObservableRepositoryHubParams>
    {
        public ObservableRepositoryHub(ObservableRepositoryHubSubscriptionManager<TKey, TValue> subscriptionManager)
            : base(subscriptionManager)
        {
        }
    }

    public class ObservableRepositoryHubParams : IEquatable<ObservableRepositoryHubParams>
    {
        public bool Equals(ObservableRepositoryHubParams other)
        {
            return true;
        }
    }

    public interface IObservableRepositoryHubClient<TKey, TValue> : ISubscriptionHubClient
    {
        void OnNewEvent(ObservableRepositoryEvent<TKey, TValue> repositoryEvent);
        void OnError(string message);
    }

    public class ObservableRepositoryHubSubscriptionManager<TKey, TValue> : SubscriptionManager<IObservableRepositoryHubClient<TKey, TValue>, ObservableRepositoryHubParams>
    {
        public ObservableRepositoryHubSubscriptionManager(ObservableRepositoryHubSubscriptionFactory<TKey, TValue> subscriptionFactory)
            : base(subscriptionFactory)
        {
        }
    }

    public class ObservableRepositoryHubSubscriptionFactory<TKey, TValue> : ISubscriptionFactory<IObservableRepositoryHubClient<TKey, TValue>, ObservableRepositoryHubParams>
    {
        private readonly ObservableRepository<TKey, TValue> _repository;

        public ObservableRepositoryHubSubscriptionFactory(ObservableRepository<TKey, TValue> repository)
        {
            _repository = repository;
        }

        public IDisposable CreateSubscription(IObservableRepositoryHubClient<TKey, TValue> client, ObservableRepositoryHubParams parameters)
        {
            return _repository
                .GetChanges()
                .Do(_ => { },
                    exception =>
                    {
                        Log.Error(exception, "GetChanges error");
                        client.OnError(exception.Message);
                    })
                .Retry()
                .Subscribe(client.OnNewEvent);
        }
    }
}
