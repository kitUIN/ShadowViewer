using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
namespace ShadowViewer;

/// <summary>
/// 单实例应用
/// https://learn.microsoft.com/zh-cn/windows/apps/windows-app-sdk/applifecycle/applifecycle-single-instance
/// </summary>
public class Program
{
    /// <summary>
    /// 启动函数
    /// </summary>
    [STAThread]
    static int Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        var isRedirect = DecideRedirection();

        if (!isRedirect)
        {
            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                _ = new App();
            });
        }

        return 0;
    }
    /// <summary>
    /// 重定向
    /// </summary>
    /// <returns>是否重定向</returns>
    private static bool DecideRedirection()
    {
        var isRedirect = false;
        var args = AppInstance.GetCurrent().GetActivatedEventArgs();
        var kind = args.Kind;
        var keyInstance = AppInstance.FindOrRegisterForKey("MySingleInstanceApp");

        if (keyInstance.IsCurrent)
        {
            keyInstance.Activated += OnActivated;
        }
        else
        {
            isRedirect = true;
            RedirectActivationTo(args, keyInstance);
        }

        return isRedirect;
    }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateEvent(
        IntPtr lpEventAttributes, bool bManualReset,
        bool bInitialState, string lpName);

    [DllImport("kernel32.dll")]
    private static extern bool SetEvent(IntPtr hEvent);

    [DllImport("ole32.dll")]
    private static extern uint CoWaitForMultipleObjects(
        uint dwFlags, uint dwMilliseconds, ulong nHandles,
        IntPtr[] pHandles, out uint dwIndex);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private static IntPtr _redirectEventHandle = IntPtr.Zero;

    /// <summary>
    /// Do the redirection on another thread, and use a non-blocking
    /// wait method to wait for the redirection to complete.
    /// </summary>
    public static void RedirectActivationTo(AppActivationArguments args,
        AppInstance keyInstance)
    {
        _redirectEventHandle = CreateEvent(IntPtr.Zero, true, false, null!);
        Task.Run(() =>
        {
            keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
            SetEvent(_redirectEventHandle);
        });

        const uint camoDefault = 0;
        const uint infinite = 0xFFFFFFFF;
        _ = CoWaitForMultipleObjects(
            camoDefault, infinite, 1,
            [_redirectEventHandle], out var handleIndex);

        // Bring the window to the foreground
        var process = Process.GetProcessById((int)keyInstance.ProcessId);
        SetForegroundWindow(process.MainWindowHandle);
    }
    /// <summary>
    /// OnActivated
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private static void OnActivated(object? sender, AppActivationArguments args)
    {
        var kind = args.Kind;
    }
}