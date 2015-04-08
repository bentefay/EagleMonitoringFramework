using System;
using System.Windows.Forms;
using ProductMonitor.Framework;
using ProductMonitor.Framework.Services;

namespace ProductMonitor
{
    public class MessageService : IMessageService
    {
        public void ShowInformation(string message, params object[] args)
        {
            MessageBox.Show(String.Format(message, args));
        }

        public void ShowInformation(Exception e, string message, params object[] args)
        {
            MessageBox.Show(String.Format(message, args));
        }

        public void ShowWarning(string message, params object[] args)
        {
            MessageBox.Show(String.Format(message, args));
        }

        public void ShowWarning(Exception e, string message, params object[] args)
        {
            MessageBox.Show(String.Format(message, args));
        }

        public void ShowError(string message, params object[] args)
        {
            MessageBox.Show(String.Format(message, args));
        }

        public void ShowError(Exception e, string message, params object[] args)
        {
            MessageBox.Show(String.Format(message, args));
        }
    }
}