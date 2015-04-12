using System;

namespace Eagle.Server.Framework.Services
{
    public interface IMessageService
    {
        void ShowInformation(string message, params object[] args);
        void ShowInformation(Exception e, string message, params object[] args);

        void ShowWarning(string message, params object[] args);
        void ShowWarning(Exception e, string message, params object[] args);

        void ShowError(string message, params object[] args);
        void ShowError(Exception e, string message, params object[] args);
    }
}
