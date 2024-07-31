namespace ShadowViewer;

public sealed partial class MainWindow : Window
{
    private MainViewModel ViewModel { get; } = DiFactory.Services.Resolve<MainViewModel>();

    public MainWindow()
    {
        InitializeComponent();
        AppTitleBar.Window = this;
        SystemBackdrop = new MicaBackdrop();
        Log.Information("Theme:{Theme}", ((FrameworkElement)this.Content).RequestedTheme);
    }

    private void DeleteHistory_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.HistoryDelete(sender);
    }
}