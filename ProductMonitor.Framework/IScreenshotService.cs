using System;

namespace ProductMonitor.Framework
{
    public interface IScreenshotService
    {
        void TakeScreenshot(string tab, string saveLocation);
    }
}
