using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.InteropServices;
using Windows.System;
using WinRT;
internal class Library
{
    private object _queue;
    private ISystemBackdropControllerWithTargets _controller;
    [StructLayout(LayoutKind.Sequential)]
    struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }
    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController(
    [In] DispatcherQueueOptions options,
    [In, Out, MarshalAs(UnmanagedType.IUnknown)]
    ref object dispatcherQueueController);
    private void EnsureDispatcherQueueController()
    {
        if (DispatcherQueue.GetForCurrentThread() != null)
            return;
        if (_queue == null)
        {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
            options.threadType = 2; // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA
            _ = CreateDispatcherQueueController(options, ref _queue);
        }
    }

    // Set Backdrop
    public void SetBackdrop(Window window, ComboBox options)
    {
        if (_controller != null)
            _controller.Dispose();
        EnsureDispatcherQueueController();
        string value = (options.SelectedItem as ComboBoxItem).Content as string;
        switch (value)
        {
            case "Acrylic":
                if (DesktopAcrylicController.IsSupported())
                {
                    _controller = new DesktopAcrylicController();
                    _controller.AddSystemBackdropTarget(
                    window.As<ICompositionSupportsSystemBackdrop>());
                    _controller.SetSystemBackdropConfiguration(
                    new SystemBackdropConfiguration());
                }
                break;
            case "Mica":
                if (MicaController.IsSupported())
                {
                    _controller = new MicaController();
                    _controller.AddSystemBackdropTarget(
                    window.As<ICompositionSupportsSystemBackdrop>());
                    _controller.SetSystemBackdropConfiguration(
                    new SystemBackdropConfiguration());
                }
                break;
            default:
                _controller = null;
                break;
        }
    }

}
