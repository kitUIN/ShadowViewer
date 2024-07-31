namespace ShadowViewer;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = DiFactory.Services.Resolve<MainViewModel>();

    public MainWindow()
    {
        InitializeComponent();
        AppTitleBar.Window = this;
        SystemBackdrop = new MicaBackdrop();
        Log.Information("Theme:{Theme}", ((FrameworkElement)this.Content).RequestedTheme);
    }

}