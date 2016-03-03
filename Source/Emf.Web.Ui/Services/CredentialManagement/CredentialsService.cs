using System;
using System.Net;
using System.Threading;
using Windows.CredentialManagement;
using Emf.Web.Ui.Services.Settings;
using Microsoft.VisualStudio.Services.Common;
using Serilog;

namespace Emf.Web.Ui.Services.CredentialManagement
{
    public class CredentialsService
    {
        private const string SettingName = "BuildMonitor";
        private readonly SettingStore<CredentialsSettings> _store;

        public CredentialsService(SettingStore<CredentialsSettings> store)
        {
            _store = store;
        }

        public VssCredentials Get()
        {
            var settings = _store.GetOrCreate(() => null);

            if (settings != null && settings.Source == CredentialsSource.CurrentCredentials)
            {
                Log.Information("Using your current Windows credentials to access TFS");
                return new VssCredentials(new WindowsCredential());
            }

            using (var credentials = new Credential(SettingName))
            {
                if (credentials.Load())
                {
                    Log.Information("Loaded saved TFS credentials from Windows credential store (setting name: {settingName})", SettingName);
                    return new VssCredentials(new WindowsCredential(new NetworkCredential(credentials.Username, credentials.SecurePassword)));
                }

                using (var prompt = new Prompt())
                {
                    prompt.ShowSaveCheckBox = true;

                    if (settings != null)
                        prompt.Username = settings.UserName;

                    while (true)
                    {
                        switch (prompt.ShowDialog())
                        {
                            case DialogResult.None:
                                Log.Warning("Failed to access credentials due to error code: {code}", prompt.ErrorCode);
                                Thread.Sleep(1000);
                                break;
                            case DialogResult.OK:
                                credentials.Username = prompt.Username;
                                credentials.SecurePassword = prompt.SecurePassword;

                                _store.Set(CredentialsSettings.CreateForCredentialStore(prompt.Username));

                                if (prompt.SaveChecked)
                                {
                                    if (credentials.Save())
                                        Log.Information("Saved TFS credentials succcessfully in Windows credential store (setting name: {settingName})", SettingName);
                                    else
                                        Log.Warning("Failed to save TFS credentials in Windows credential store (setting name: {settingName})", SettingName);
                                }

                                return new VssCredentials(new WindowsCredential(new NetworkCredential(credentials.Username, credentials.SecurePassword)));
                            case DialogResult.Cancel:

                                if (prompt.SaveChecked)
                                {
                                    Log.Information("Using your current Windows credentials to access TFS (and remembering for next run)");
                                    _store.Set(CredentialsSettings.CreateForCurrentCredentials());
                                }
                                else
                                {
                                    Log.Information("Using your current Windows credentials to access TFS");
                                }

                                return new VssCredentials(new WindowsCredential());
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }

        public void Delete()
        {
            var deletedAnything = false;

            if (_store.Delete())
            {
                Log.Information("Deleted saved settings");
                deletedAnything = true;
            }

            using (var credentials = new Credential(SettingName))
            {
                if (credentials.Exists())
                {
                    deletedAnything = true;
                    if (credentials.Delete())
                    {
                        Log.Information("Deleted TFS credentials succcessfully from Windows credential store (setting name: {settingName})", SettingName);
                    }
                    else
                    {
                        Log.Warning("Failed to deleted TFS credentials from Windows credential store (setting name: {settingName})", SettingName);
                    }
                }
            }

            if (!deletedAnything)
                Log.Information("Nothing to delete");
        }
    }
}
