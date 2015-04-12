using System;
using Eagle.Server.Framework.Services;
using Serilog;

namespace Eagle.Server.UI.Web.Services
{
    internal class MessageService : IMessageService
    {
        public void ShowInformation(string message, params object[] args)
        {
            Log.Information(message, args);
        }

        public void ShowInformation(Exception e, string message, params object[] args)
        {
            Log.Information(e, message, args);
        }

        public void ShowWarning(string message, params object[] args)
        {
            Log.Warning(message, args);
        }

        public void ShowWarning(Exception e, string message, params object[] args)
        {
            Log.Warning(e, message, args);
        }

        public void ShowError(string message, params object[] args)
        {
            Log.Error(message, args);
        }

        public void ShowError(Exception e, string message, params object[] args)
        {
            Log.Error(e, message, args);
        }
    }
}