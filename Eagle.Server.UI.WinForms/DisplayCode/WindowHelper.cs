using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Eagle.Server.Framework.Generic;

namespace Eagle.Server.UI.WinForms.DisplayCode
{
    public static class WindowHelper
    {
        [DataContract(Name = "WindowState", Namespace = "Data")]
        public class WindowState
        {
            [DataMember(Name = "State")]
            public FormWindowState State { get; set; }
            [DataMember(Name = "Position")]
            public Rectangle Position { get; set; }
            [DataMember(Name = "Size")]
            public Size Size { get; set; }
        }

        public static void SavePosition(Form form, string windowId)
        {
            var windowState = new WindowState();

            // Only save the WindowState if Normal or Maximized
            if (form.WindowState == FormWindowState.Normal || form.WindowState == FormWindowState.Maximized)
                windowState.State = form.WindowState;
            else
                windowState.State = FormWindowState.Normal;

            // Reset window state to normal to get the correct bounds
            // Also make the form invisible to prevent distracting the user
            form.Visible = false;
            form.WindowState = FormWindowState.Normal;

            windowState.Position = form.DesktopBounds;
            windowState.Size = form.Size;

            WriteToIsolatedStorage(windowId, windowState);
        }

        public static void LoadPosition(Form form, string windowId)
        {
            // This is the default
            form.WindowState = FormWindowState.Normal;
            form.StartPosition = FormStartPosition.WindowsDefaultBounds;

            var maybeWindowState = ReadFromIsolatedStorage(windowId);

            if (!maybeWindowState.HasValue)
                return;

            var windowState = maybeWindowState.Value;

            // Check if the saved bounds are nonzero and visible on any screen
            if (windowState.Position != Rectangle.Empty && IsVisibleOnAnyScreen(windowState.Position))
            {
                // First set the bounds
                form.StartPosition = FormStartPosition.Manual;
                form.DesktopBounds = windowState.Position;

                // Afterwards set the window state to the saved value (which could be Maximized)
                form.WindowState = windowState.State;
            }
            else
            {
                // This resets the upper left corner of the window to windows standards
                form.StartPosition = FormStartPosition.WindowsDefaultLocation;

                // We can still apply the saved size
                form.Size = windowState.Size;
            }
        }

        private static bool IsVisibleOnAnyScreen(Rectangle rect)
        {
            return Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(rect));
        }

        private static void WriteToIsolatedStorage(string windowId, WindowState windowState)
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            using (var stream = new IsolatedStorageFileStream(windowId, FileMode.Create, FileAccess.Write, isoStore))
            {
                var serializer = new DataContractSerializer(typeof(WindowState));
                serializer.WriteObject(stream, windowState);
            }
        }

        private static Maybe<WindowState> ReadFromIsolatedStorage(string windowId)
        {
            var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            if (!isoStore.FileExists(windowId))
                return Maybe.Empty;

            using (var stream = new IsolatedStorageFileStream(windowId, FileMode.Open, FileAccess.Read, isoStore))
            {
                var serializer = new DataContractSerializer(typeof(WindowState));
                return (WindowState)serializer.ReadObject(stream);
            }
        }
    }
}