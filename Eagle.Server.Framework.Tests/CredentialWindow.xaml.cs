using System;
using System.Net;
using System.Security;
using System.Windows;

namespace Eagle.Server.Framework.Tests
{
    public partial class CredentialWindow
    {
        public CredentialWindow()
        {
            InitializeComponent();
        }

        public SecureString Password => PasswordTextBox.SecurePassword;
        public string UserName => UserNameTextBox.Text;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static ICredentials GetCredentials()
        {
            var dialog = new CredentialWindow();
            dialog.ShowDialog();
            var userNameTokens = dialog.UserName.Split(new [] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
            if (userNameTokens.Length == 2)
                return new NetworkCredential(userName: userNameTokens[1], password: dialog.Password, domain: userNameTokens[0]);

            return new NetworkCredential(dialog.UserName, dialog.Password);
        }
    }
}
