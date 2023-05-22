namespace ShadowViewer.ViewModels
{
    public partial class BookShelfSettingsViewModel : ObservableObject
    {
        public BookShelfSettingsViewModel() { }
        [ObservableProperty]
        private bool isRememberDeleteFilesWithComicDelete = Config.IsRememberDeleteFilesWithComicDelete; 
        [ObservableProperty]
        private bool isDeleteFilesWithComicDelete = Config.IsDeleteFilesWithComicDelete;

        partial void OnIsDeleteFilesWithComicDeleteChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                Config.IsDeleteFilesWithComicDelete = IsDeleteFilesWithComicDelete;
            }
        }
        partial void OnIsRememberDeleteFilesWithComicDeleteChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                Config.IsRememberDeleteFilesWithComicDelete = IsRememberDeleteFilesWithComicDelete;
            }
        }
    }
}
