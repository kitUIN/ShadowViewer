namespace ShadowViewer;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = DiFactory.Services.Resolve<MainViewModel>();
    public ICallableService Caller { get; } = DiFactory.Services.Resolve<ICallableService>();

    public MainWindow()
    {
        InitializeComponent();
        SystemBackdrop = new MicaBackdrop();
        Log.Information("Theme:{Theme}", ((FrameworkElement)this.Content).RequestedTheme);
        Caller.ThemeChangedEvent += AppTitleBar_ThemeChangedEvent;
    }

    private void AppTitleBar_ThemeChangedEvent(object? sender, EventArgs e)
    {
        AppTitleBar.InvokeThemeChanged(this);
    }
}