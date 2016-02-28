using System;
using System.Net;
using System.Threading;
using Windows.CredentialManagement;
using Microsoft.VisualStudio.Services.Common;
using Serilog;

namespace Emf.Web.Ui.Services.CredentialManagement
{
    public class CredentialsService
    {
        private const string SettingName = "BuildMonitor";

        public static VssCredentials Get()
        {
            using (var credentials = new Credential(SettingName))
            {
                Populate(credentials);

                return new VssCredentials(new WindowsCredential(new NetworkCredential(credentials.Username, credentials.SecurePassword)));
            }
        }

        public static void Delete()
        {
            using (var credentials = new Credential(SettingName))
            {
                if (credentials.Delete())
                    Log.Information("Deleted TFS credentials succcessfully from Windows credential store (setting name: {settingName})", SettingName);
                else
                    Log.Warning("Failed to deleted TFS credentials from Windows credential store (setting name: {settingName})", SettingName);
            }
        }

        private static void Populate(Credential credential)
        {
            if (credential.Load())
            {
                Log.Information("Loaded saved TFS credentials from Windows credential store (setting name: {settingName})", SettingName);
                return;
            }

            using (var prompt = new Prompt())
            {
                prompt.ShowSaveCheckBox = true;

                while (true)
                {
                    switch (prompt.ShowDialog())
                    {
                        case DialogResult.None:
                            Log.Warning("Failed to access credentials due to error code: {code}", prompt.ErrorCode);
                            Thread.Sleep(1000);
                            break;
                        case DialogResult.OK:
                            credential.Username = prompt.Username;
                            credential.SecurePassword = prompt.SecurePassword;

                            if (prompt.SaveChecked)
                            {
                                if (credential.Save())
                                    Log.Information("Saved TFS credentials succcessfully in Windows credential store (setting name: {settingName})", SettingName);
                                else
                                    Log.Warning("Failed to save TFS credentials in Windows credential store (setting name: {settingName})", SettingName);
                            }
                            return;
                        case DialogResult.Cancel:
                            Log.Error("You need to enter your windows credentials to access the TFS server. App is closing...");
                            Console.ReadLine();
                            Environment.Exit(1);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}
