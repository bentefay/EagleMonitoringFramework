namespace Emf.Web.Ui.Services.CredentialManagement
{
    public class CredentialsSettings
    {
        public static CredentialsSettings CreateForCurrentCredentials()
        {
            return new CredentialsSettings(CredentialsSource.CurrentCredentials, null);
        }

        public static CredentialsSettings CreateForCredentialStore(string userName)
        {
            return new CredentialsSettings(CredentialsSource.CredentialStore, userName);
        }

        public CredentialsSettings(CredentialsSource source, string userName)
        {
            Source = source;
            UserName = userName;
        }

        public CredentialsSource Source { get; }
        public string UserName { get; }
    }
}