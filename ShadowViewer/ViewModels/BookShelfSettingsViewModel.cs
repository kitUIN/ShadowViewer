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
        private bool isBookShelfInfoBar = Config.IsBookShelfInfoBar;
        [ObservableProperty]
        private bool isImportAgain = Config.IsImportAgain;
        [ObservableProperty]
        private bool isTopBarDetail = Config.IsTopBarDetail;
        partial void OnIsTopBarDetailChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                Config.IsTopBarDetail = IsTopBarDetail;
            }
        }
        partial void OnIsImportAgainChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                Config.IsImportAgain = IsImportAgain;
            }
        }
        partial void OnIsBookShelfInfoBarChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                Config.IsBookShelfInfoBar = IsBookShelfInfoBar;
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
