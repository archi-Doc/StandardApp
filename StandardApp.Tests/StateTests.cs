using StandardWinUI.PresentationState;

namespace StandardApp.Tests;

/// <summary>
/// Tests arithmetic commands at integer boundaries and after invalid input.
/// </summary>
public class StateTests
{
    [Theory]
    [InlineData("2147483647", "6442450941")]
    [InlineData("-2147483648", "-6442450944")]
    [InlineData("0", "0")]
    [InlineData("abc", "")]
    [InlineData("2147483648", "")]
    public void MultiplyDoesNotOverflowOrRetainStaleOutput(string source, string expected)
    {
        var state = new StatePageState { SourceText = source, DestinationText = "old" };
        state.MultiplyCommand.Execute(null);
        Assert.Equal(expected, state.DestinationText);
        var advanced = new AdvancedPageState(null!, null!, null!) { SourceText = source, DestinationText = "old" };
        advanced.MultiplyCommand.Execute(null);
        Assert.Equal(expected, advanced.DestinationText);
        Assert.False(advanced.CanExit);
    }
}
