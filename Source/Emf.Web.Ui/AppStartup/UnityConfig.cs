using System;
using System.Collections.Generic;
using Emf.Web.Ui.Hubs;
using Emf.Web.Ui.Models;
using Emf.Web.Ui.Services;
using Emf.Web.Ui.Services.CredentialManagement;
using Emf.Web.Ui.Services.Settings;
using Microsoft.Practices.Unity;
using Serilog;

namespace Emf.Web.Ui.AppStartup
{
    public class UnityConfig
    {
        private static readonly Lazy<IUnityContainer> _container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        public static IUnityContainer GetConfiguredContainer()
        {
            return _container.Value;
        }

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            Log.Information("Registering IoC dependencies with container");

            var settingStore = new SettingStore();
            var credentialService = new CredentialsService(settingStore.ForKey(SettingKeys.Credentials));

            var credentials = credentialService.Get();

            var connectionSettingsService = new ConnectionSettingsService(settingStore.ForKey(SettingKeys.ConnectionSettings));
            container.RegisterInstance(connectionSettingsService);

            var connectionSettings = connectionSettingsService.Get();

            var tfsBuildDefinitionRepository = new TfsBuildDefinitionRepository(credentials, settingStore.ForKey(SettingKeys.BuildDefinitions), settingStore.ForKey(SettingKeys.Builds), connectionSettings);
            var tfsMonitoringService = new TfsMonitoringService(tfsBuildDefinitionRepository, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(30));
            tfsMonitoringService.Start();
            var observableCollections = new Dictionary<string, IObservableRepository>
            {
                {"buildDefinitions", tfsMonitoringService.BuildDefinitions },
                {"builds", tfsMonitoringService.Builds },
                {"settings", tfsMonitoringService.Settings }
            };

            var observableRepositoryHubSubscriptionFactory = new ObservableRepositoryHubSubscriptionFactory(observableCollections);

            container.RegisterInstance(observableRepositoryHubSubscriptionFactory);
            container.RegisterInstance(credentialService);
        }
    }
}
