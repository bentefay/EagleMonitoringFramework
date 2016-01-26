using System;
using Emf.Web.Ui.Hubs.Core;
using Microsoft.AspNet.SignalR.Hubs;

namespace Emf.Web.Ui.Hubs
{
    [HubName("buildDefinitionsHub")]
    public class BuildDefinitionsHub : SubscriptionHub<IBuildDefinitionsHubClient, BuildDefinitionsHubParams>
    {
        public BuildDefinitionsHub(BuildDefinitionsHubManager manager)
            : base(manager)
        {
        }
    }

    public class BuildDefinitionsHubManager : SubscriptionHubManager<IBuildDefinitionsHubClient, BuildDefinitionsHubParams>
    {
        public BuildDefinitionsHubManager(BuildDefinitionsSubscriptionService subscriptionService)
            : base(subscriptionService)
        {
        }
    }

    public interface IBuildDefinitionsHubClient : ISubscriptionHubClient
    {
        void OnNewBuildDefinition();
    }

    public class BuildDefinitionsHubParams : IEquatable<BuildDefinitionsHubParams>
    {
        public BuildDefinitionsHubParams()
        {
        }

        public bool Equals(BuildDefinitionsHubParams other)
        {
            throw new NotImplementedException();
        }
    }
}
