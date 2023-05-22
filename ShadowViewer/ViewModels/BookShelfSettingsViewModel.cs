namespace ShadowViewer.ViewModels
{
    public partial class BookShelfSettingsViewModel : ObservableObject
    {
        public BookShelfSettingsViewModel() { }
        [ObservableProperty]
        private bool isRememberDeleteFilesWithComicDelete = Config.IsRememberDeleteFilesWithComicDelete; 
        [ObservableProperty]
        private bool isDeleteFilesWithComicDelete = Config.IsDeleteFilesWithComicDelete;
        [ObservableProperty]
        private bool isBookShelfMenuShow = Config.IsBookShelfMenuShow;
        [ObservableProperty]
        private bool isBookShelfDetailShow = Config.IsBookShelfDetailShow;

        partial void OnIsBookShelfDetailShowChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                Config.IsBookShelfDetailShow = IsBookShelfDetailShow;
            }
        }
        partial void OnIsBookShelfMenuShowChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                Config.IsBookShelfMenuShow = IsBookShelfMenuShow;
            }
        }
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
