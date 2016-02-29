using Emf.Web.Ui.Services.CredentialManagement;
using Emf.Web.Ui.Services.Settings;

namespace Emf.Web.Ui
{
    public static class SettingKeys
    {
        public static readonly SettingKey<CredentialsSettings> Credentials = new SettingKey<CredentialsSettings>("Credentials");
    }
}