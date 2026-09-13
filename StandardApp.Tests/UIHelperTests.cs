using Arc.WinUI;

namespace StandardApp.Tests;

/// <summary>
/// Tests URL validation and single-instance mutex recovery.
/// </summary>
public class UIHelperTests
{
    [Theory]
    [InlineData("relative/path")]
    [InlineData("file:///C:/Windows/System32/cmd.exe")]
    [InlineData("javascript:alert(1)")]
    [InlineData("cmd /c echo test")]
    [InlineData("")]
    public void BrowserRejectsUnsupportedUrlsWithoutLaunchingProcesses(string url)
    {
        Assert.Equal("url", Assert.Throws<ArgumentException>(() => UIHelper.OpenBrowser(url)).ParamName);
    }

    [Fact]
    public void FirstInstanceAcquiresMutex()
    {
        using var mutex = new Mutex();
        Assert.False(UIHelper.TryActivateRunningInstance(mutex));
        mutex.ReleaseMutex();
    }

    [Fact]
    public void AbandonedMutexIsRecoveredByNextInstance()
    {
        using var mutex = new Mutex();
        var thread = new Thread(() => mutex.WaitOne());
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(5)));
        Assert.False(UIHelper.TryActivateRunningInstance(mutex));
        mutex.ReleaseMutex();
    }
}
