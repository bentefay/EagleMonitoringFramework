namespace ProductMonitor.Framework
{
    public interface ICheckDisplay
    {
        bool IsPaused();
        bool HasError();
        bool IsTriggered();
        string GetResult();
        string GetStatus();
        string GetError();
        string GetLocation();
        string GetCheckType();
        string GetTab();
    }
}
