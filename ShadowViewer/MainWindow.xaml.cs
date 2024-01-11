namespace ShadowViewer;

public sealed partial class MainWindow : Window
{
    

    private ICallableService Caller { get; } = DiFactory.Services.Resolve<ICallableService>(); 
    private MainViewModel ViewModel { get; } = DiFactory.Services.Resolve<MainViewModel>();

    public MainWindow()
    {
        InitializeComponent();
        AppTitleBar.Window = this;
        SystemBackdrop = new MicaBackdrop();
        Title = "ShadowViewer";
        AppTitleBar.Subtitle = Config.IsDebug ? ResourcesHelper.GetString(ResourceKey.Debug) : "";
        Caller.DebugEvent += (_, _) =>
            AppTitleBar.Subtitle = Config.IsDebug ? ResourcesHelper.GetString(ResourceKey.Debug) : "";
    }


    

    


    private void DeleteHistory_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.HistoryDelete(sender);
    }
}