using System;
using Emf.Web.Ui.Hubs.Core;

namespace Emf.Web.Ui.Hubs
{
    public class BuildDefinitionsSubscriptionService : ISubscriptionService<IBuildDefinitionsHubClient, BuildDefinitionsHubParams>
    {
        public BuildDefinitionsSubscriptionService()
        {
        }

        public IDisposable CreateSubscription(IBuildDefinitionsHubClient client, BuildDefinitionsHubParams parameters)
        {
            throw new AccessViolationException();
        }
    }
}

