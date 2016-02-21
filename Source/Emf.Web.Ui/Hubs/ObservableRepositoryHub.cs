using Emf.Web.Ui.Hubs.Core;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Emf.Web.Ui.Models;
using Microsoft.AspNet.SignalR.Hubs;
using Serilog;

namespace Emf.Web.Ui.Hubs
{
    [HubName("repositories")]
    public class ObservableRepositoryHub : SubscriptionHub<IObservableRepositoryHubClient, ObservableRepositoryHubParams>
    {
        public ObservableRepositoryHub(ObservableRepositoryHubSubscriptionManager subscriptionManager)
            : base(subscriptionManager)
        {
        }
    }

    public class ObservableRepositoryHubParams : IEquatable<ObservableRepositoryHubParams>
    {
        public string RepositoryId { get; set; }

        public bool Equals(ObservableRepositoryHubParams other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return RepositoryId == other.RepositoryId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ObservableRepositoryHubParams);
        }

        public override int GetHashCode()
        {
            return RepositoryId.GetHashCode();
        }
    }

    public interface IObservableRepositoryHubClient : ISubscriptionHubClient
    {
        void OnNewEvent(string repositoryId, IObservableRepositoryEvent repositoryEvent);
        void OnError(string message);
    }

    public class ObservableRepositoryHubSubscriptionManager : SubscriptionManager<IObservableRepositoryHubClient, ObservableRepositoryHubParams>
    {
        public ObservableRepositoryHubSubscriptionManager(ObservableRepositoryHubSubscriptionFactory subscriptionFactory)
            : base(subscriptionFactory)
        {
        }
    }

    public class ObservableRepositoryHubSubscriptionFactory : ISubscriptionFactory<IObservableRepositoryHubClient, ObservableRepositoryHubParams>
    {
        private readonly Dictionary<string, IObservableRepository> _repositoryMap;

        public ObservableRepositoryHubSubscriptionFactory(Dictionary<string, IObservableRepository> repositoryMap)
        {
            _repositoryMap = repositoryMap;
        }

        public IDisposable CreateSubscription(IObservableRepositoryHubClient client, ObservableRepositoryHubParams parameters)
        {
            IObservableRepository repository;
            if (_repositoryMap.TryGetValue(parameters.RepositoryId, out repository))
            {
                return repository
                    .GetChanges()
                    .Do(_ => { },
                        exception =>
                        {
                            Log.Error(exception, "GetChanges error");
                            client.OnError(exception.Message);
                        })
                    .Retry()
                    .Subscribe(e => client.OnNewEvent(parameters.RepositoryId, e), e => client.OnError(e.Message));
            }
            else
            {
                throw new ArgumentException($"Repository with id '{nameof(parameters.RepositoryId)}' does not exist");
            }
        }
    }
}
