namespace ShadowViewer.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isDebug = Config.IsDebug;
        [ObservableProperty]
        private string comicsPath = Config.ComicsPath;
        [ObservableProperty]
        private string tempPath = Config.TempPath;
        [ObservableProperty]
        private bool isTopBarDetail = Config.IsTopBarDetail;
        partial void OnIsTopBarDetailChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                Config.IsTopBarDetail = IsTopBarDetail;
            }
        }
        partial void OnComicsPathChanged(string oldValue, string newValue)
        {
            if(oldValue != newValue)
            {
                Config.ComicsPath = ComicsPath;
            }
        }
        partial void OnIsDebugChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                Config.IsDebug = IsDebug;
            }
        }
        partial void OnTempPathChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                Config.TempPath = TempPath;
            }
        }
        public SettingsViewModel() { }
    }
}
