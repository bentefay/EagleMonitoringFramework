using System;
using Emf.Web.Ui.Models;
using Emf.Web.Ui.Services.Settings;

namespace Emf.Web.Ui.Services
{
    public class ConnectionSettingsService
    {
        private readonly SettingStore<ConnectionSettings> _connectionSettingStore;

        public ConnectionSettingsService(SettingStore<ConnectionSettings> connectionSettingStore)
        {
            _connectionSettingStore = connectionSettingStore;
        }

        public ConnectionSettings Get()
        {
            ConnectionSettings connectionSettings;
            if (!_connectionSettingStore.TryGet(out connectionSettings))
            {
                Console.WriteLine("Enter TFS collection url (e.g. \"http://tfs.gr.local:8080/tfs/GRCollection\"):");
                var tfsCollectionUrl = Console.ReadLine();
                Console.WriteLine("Enter TFS project (e.g. \"GlobalRoam\"):");
                var tfsProject = Console.ReadLine();
                connectionSettings = new ConnectionSettings(tfsCollectionUrl, tfsProject);
                _connectionSettingStore.Set(connectionSettings);
            }

            Console.WriteLine($"Using {connectionSettings.TfsCollectionUrl} {connectionSettings.TfsProject}");

            return connectionSettings;
        }

        public void Delete()
        {
            _connectionSettingStore.Delete();
        }
    }
}