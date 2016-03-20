using System.Collections.Generic;
using Emf.Web.Ui.Models;
using Emf.Web.Ui.Services.CredentialManagement;
using Emf.Web.Ui.Services.Settings;

namespace Emf.Web.Ui
{
    public static class SettingKeys
    {
        public static readonly SettingKey<CredentialsSettings> Credentials = new SettingKey<CredentialsSettings>("Credentials");
        public static readonly SettingKey<Dictionary<int, BuildDefinition>> BuildDefinitions = new SettingKey<Dictionary<int, BuildDefinition>>("BuildDefinitions");
        public static readonly SettingKey<BuildCollection> Builds = new SettingKey<BuildCollection>("Builds");
    }
}