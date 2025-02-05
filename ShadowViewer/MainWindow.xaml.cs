using ShadowViewer.Plugin.Local;
using ShadowViewer.Plugin.PluginManager;
using System.Diagnostics;

namespace ShadowViewer;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = DiFactory.Services.Resolve<MainViewModel>();
    public ICallableService Caller { get; } = DiFactory.Services.Resolve<ICallableService>();

    public MainWindow()
    {
        InitializeComponent();
        SystemBackdrop = new MicaBackdrop();
        Caller.ThemeChangedEvent += AppTitleBar_ThemeChangedEvent;
    }


    private void AppTitleBar_ThemeChangedEvent(object? sender, EventArgs e)
    {
        AppTitleBar.InvokeThemeChanged(this);
    }

    private async void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
    {

    }
}