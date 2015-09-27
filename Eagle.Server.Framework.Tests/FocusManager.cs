using System.Windows;
using System.Windows.Controls;

namespace Eagle.Server.Framework.Tests
{
    public static class FocusManager
    {
        public static readonly DependencyProperty FocusFirstProperty =
            DependencyProperty.RegisterAttached(
                "FocusFirst",
                typeof(bool),
                typeof(Control),
                new PropertyMetadata(false, OnFocusFirstPropertyChanged));

        public static bool GetFocusFirst(Control control)
        {
            return (bool)control.GetValue(FocusFirstProperty);
        }

        public static void SetFocusFirst(Control control, bool value)
        {
            control.SetValue(FocusFirstProperty, value);
        }

        static void OnFocusFirstPropertyChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (Control)obj;
            RoutedEventHandler handler = null;
            handler = (s, a) =>
            {
                control.Focus();
                control.Loaded -= handler;
            };
            control.Loaded += handler;
        }
    }
}