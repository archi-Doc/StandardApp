using Arc.WinUI;

namespace StandardApp.Tests;

/// <summary>
/// Tests DPI conversion and preservation of physical window positions.
/// </summary>
public class PlacementTests
{
    [Fact]
    public void FullyScaledPlacementRoundTripsAndRetainsMetadata()
    {
        var native = new NativeWindowPlacement
        {
            flags = 2,
            showCmd = ShowCommand.SHOWNORMAL,
            minPosition = new(96, 192),
            maxPosition = new(-96, -192),
            normalPosition = new(-960, -720, 960, 720),
        };
        var placement = new DipWindowPlacement();
        Assert.False(placement.IsValid);
        placement.FromWindowPlacement(native, 192, 144);
        Assert.Equal(48, placement.MinPosition.X);
        Assert.Equal(-480, placement.NormalPosition.Left);
        var result = placement.ToWindowPlacement(192, 144);
        Assert.Equal(native.normalPosition, result.normalPosition);
        Assert.Equal(native.minPosition, result.minPosition);
        Assert.Equal(native.maxPosition, result.maxPosition);
        Assert.Equal(native.flags, result.flags);
        Assert.Equal(native.showCmd, result.showCmd);

        var wpfNative = new Arc.WinAPI.WINDOWPLACEMENT
        {
            length = 44,
            flags = native.flags,
            showCmd = Arc.WinAPI.SW.SHOWNORMAL,
            minPosition = new(96, 192),
            maxPosition = new(-96, -192),
            normalPosition = new(-960, -720, 960, 720),
        };
        var wpf = new Arc.WinAPI.DipWindowPlacement();
        wpf.FromWindowPlacement(wpfNative, 192, 144);
        Assert.Equal(wpfNative.normalPosition, wpf.ToWindowPlacement(192, 144).normalPosition);
        wpf.FromWindowPlacementWithPhysicalPosition(wpfNative, 192, 144);
        var wpfResult = wpf.ToWindowPlacementWithPhysicalPosition(192, 144);
        Assert.Equal(wpfNative.normalPosition, wpfResult.normalPosition);
        Assert.Equal(wpfNative.minPosition, wpfResult.minPosition);
        Assert.Equal(wpfNative.maxPosition, wpfResult.maxPosition);
        Assert.Equal(44, wpfResult.length);
    }

    [Theory]
    [InlineData(96, 96)]
    [InlineData(144, 192)]
    [InlineData(192, 144)]
    public void PlacementRoundTripsAtDifferentDpi(double dpiX, double dpiY)
    {
        var native = new NativeWindowPlacement
        {
            flags = 2,
            showCmd = ShowCommand.SHOWMAXIMIZED,
            minPosition = new(-1, -1),
            maxPosition = new(10, 20),
            normalPosition = new(-1920, -100, -960, 620),
        };
        var placement = new DipWindowPlacement(native, dpiX, dpiY);
        Assert.True(placement.IsValid);
        Assert.Equal(-1920, placement.NormalPosition.Left);
        var result = placement.ToWindowPlacementWithPhysicalPosition(dpiX, dpiY);
        Assert.Equal(native.normalPosition, result.normalPosition);
        Assert.Equal(native.minPosition, result.minPosition);
        Assert.Equal(native.maxPosition, result.maxPosition);
        Assert.Equal(native.flags, result.flags);
        Assert.Equal(native.showCmd, result.showCmd);
        Assert.Equal(44, result.length);
    }

    [Fact]
    public void LargeCoordinatesDoNotOverflowIntermediateIntegerArithmetic()
    {
        var point = new DipPoint(new NativePoint(int.MaxValue, int.MinValue), 96, 96);
        Assert.Equal((double)int.MaxValue, point.X);
        Assert.Equal((double)int.MinValue, point.Y);
        var rect = new DipRect();
        rect.FromRectWithPhysicalPosition(new(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue), 96, 96);
        Assert.Equal((double)int.MaxValue, rect.Right);
        Assert.Equal((double)int.MaxValue, rect.Bottom);

        var wpfPoint = new Arc.WinAPI.DipPoint(new(int.MaxValue, int.MinValue), 96, 96);
        Assert.Equal(point.X, wpfPoint.X);
        Assert.Equal(point.Y, wpfPoint.Y);
        var wpfRect = new Arc.WinAPI.DipRect();
        wpfRect.FromRectWithPhysicalPosition(new(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue), 96, 96);
        Assert.Equal(rect.Right, wpfRect.Right);
        Assert.Equal(rect.Bottom, wpfRect.Bottom);
    }
}
