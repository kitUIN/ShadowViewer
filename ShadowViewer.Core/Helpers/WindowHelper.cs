namespace ShadowViewer.Helpers
{
    // Copy From WinUI 3 Gallery
    public static class WindowHelper
    {
        static public Window CreateWindow()
        {
            Window newWindow = new Window();
            TrackWindow(newWindow);
            return newWindow;
        }

        static public void TrackWindow(Window window)
        {
            window.Closed += (sender, args) => {
                _activeWindows.Remove(window);
            };
            _activeWindows.Add(window);
        }
        static public void ColseWindow(Window window)
        {
            window.Close();
        }
        static public void ColseWindowFromTitle(string title)
        {
            if (title != null)
            {
                foreach (Window window in _activeWindows)
                {
                    if (title == window.Title)
                    {
                        ColseWindow(window);
                        return;
                    }
                }
            }
        }
        static public Window GetWindowForTitle(string title)
        {
            if (title != null)
            {
                foreach (Window window in _activeWindows)
                {
                    if (title == window.Title)
                    {
                        return window;
                    }
                }
            }
            return null;
        }
        static public void SetWindowTitle(string oldTitle,string title)
        {
            foreach (Window window in _activeWindows)
            {
                if (oldTitle == window.Title)
                {
                    window.Title = title;
                }
            }
        }
        static public Window GetWindowForElement(UIElement element)
        {
            if (element.XamlRoot != null)
            {
                foreach (Window window in _activeWindows)
                {
                    if (element.XamlRoot == window.Content.XamlRoot)
                    {
                        return window;
                    }
                }
            }
            return null;
        }
        static public Window GetWindowForXamlRoot(XamlRoot xamlRoot)
        {
            if (xamlRoot != null)
            {
                foreach (Window window in _activeWindows)
                {
                    if (xamlRoot == window.Content.XamlRoot)
                    {
                        return window;
                    }
                }
            }
            return null;
        }

        public static List<Window> ActiveWindows { get { return _activeWindows; } }

        private static List<Window> _activeWindows = new List<Window>();
    }
}
