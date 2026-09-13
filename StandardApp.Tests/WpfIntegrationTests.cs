using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Arc.WPF;
using Tinyhand;

namespace StandardApp.Tests;

/// <summary>
/// Tests WPF serialization, native placement, and dispatcher failures on an STA thread.
/// </summary>
public class WpfIntegrationTests
{
    [Fact]
    public void CustomizedBrushRoundTripsThroughSerialization()
    {
        RunSta(() =>
        {
            var original = new BrushOption(Colors.Red);
            var color = Color.FromArgb(123, 45, 67, 89);
            original.SetColor(color);
            var bytes = TinyhandSerializer.Serialize(original);
            var restored = TinyhandSerializer.Deserialize<BrushOption>(bytes)!;
            Assert.True(restored.IsColorChanged);
            Assert.Equal(color, restored.Brush!.Color);
            Assert.Equal(original.ColorArgb, restored.ColorArgb);
        });
    }

    [Fact]
    public void NativePlacementIsInitializedForBothImplementations()
    {
        RunSta(() =>
        {
            using var source = new HwndSource(new HwndSourceParameters("Placement test")
            {
                Width = 320,
                Height = 200,
                WindowStyle = 0,
            });
            var wpfMethod = typeof(Arc.WinAPI.NativeMethods).GetMethod("GetWindowPlacement", BindingFlags.NonPublic | BindingFlags.Static)!;
            object?[] wpfArgs = [source.Handle, null];
            Assert.True((bool)wpfMethod.Invoke(null, wpfArgs)!);
            var wpfPlacement = Assert.IsType<Arc.WinAPI.WINDOWPLACEMENT>(wpfArgs[1]);
            Assert.Equal(Marshal.SizeOf<Arc.WinAPI.WINDOWPLACEMENT>(), wpfPlacement.length);
            Assert.True(wpfPlacement.normalPosition.Right > wpfPlacement.normalPosition.Left);

            var winuiType = typeof(Arc.WinUI.DipWindowPlacement).Assembly.GetType("Arc.Internal.WinAPI")!;
            var winuiMethod = winuiType.GetMethod("GetWindowPlacement", BindingFlags.Public | BindingFlags.Static)!;
            object?[] winuiArgs = [source.Handle, null];
            Assert.True((bool)winuiMethod.Invoke(null, winuiArgs)!);
            var winuiPlacement = Assert.IsType<Arc.WinUI.NativeWindowPlacement>(winuiArgs[1]);
            Assert.Equal(Marshal.SizeOf<Arc.WinUI.NativeWindowPlacement>(), winuiPlacement.length);
            Assert.Equal(wpfPlacement.normalPosition.Left, winuiPlacement.normalPosition.Left);
            Assert.Equal(wpfPlacement.normalPosition.Right, winuiPlacement.normalPosition.Right);
        });
    }

    [Fact]
    public void ClosedDialogFaultsReturnedTaskInsteadOfLeavingItPending()
    {
        RunSta(() =>
        {
            var owner = new Window();
            _ = new WindowInteropHelper(owner).EnsureHandle();
            var dialog = new MessageDialog(owner);
            dialog.Close();
            var task = dialog.ShowDialogAsync();
            var frame = new DispatcherFrame();
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += (_, _) => frame.Continue = false;
            timer.Start();
            _ = task.ContinueWith(_ => dialog.Dispatcher.BeginInvoke(() => frame.Continue = false), TaskScheduler.Default);
            Dispatcher.PushFrame(frame);
            timer.Stop();
            Assert.True(task.IsFaulted);
            Assert.IsType<InvalidOperationException>(task.Exception!.InnerException);
            owner.Close();
        });
    }

    private static void RunSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                error = exception;
            }
            finally
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(15)), "The STA test did not complete.");
        if (error is not null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }
}
