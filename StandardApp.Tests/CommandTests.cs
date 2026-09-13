using Arc.Mvvm;

namespace StandardApp.Tests;

/// <summary>
/// Tests command execution and nested property observation.
/// </summary>
public class CommandTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Enabled")]
    public void ObservedPropertyAcceptsAllPropertyNotifications(string? propertyName)
    {
        var model = new Model();
        var command = new DelegateCommand(() => { }).ObservesCanExecute(() => model.Enabled);
        var count = 0;
        command.CanExecuteChanged += (_, _) => count++;
        model.Notify(propertyName);
        Assert.Equal(1, count);
        Assert.True(command.CanExecute());
    }

    [Fact]
    public void NestedPropertyResubscribesAndDetachesOldChild()
    {
        var original = new Model();
        var model = new Model { Child = original };
        var command = new DelegateCommand(() => { }).ObservesProperty(() => model.Child!.Enabled);
        var count = 0;
        command.CanExecuteChanged += (_, _) => count++;
        model.Child = new Model();
        Assert.Equal(1, count);
        original.Notify(nameof(Model.Enabled));
        Assert.Equal(1, count);
        model.Child.Notify(nameof(Model.Enabled));
        Assert.Equal(2, count);
        model.Child = null;
        Assert.Equal(3, count);
        model.Child = new Model();
        model.Child.Notify(string.Empty);
        Assert.Equal(5, count);
    }

    [Fact]
    public void UnrelatedPropertyDoesNotNotifyCommand()
    {
        var model = new Model();
        var command = new DelegateCommand<string>(_ => { }).ObservesCanExecute(() => model.Enabled);
        var count = 0;
        command.CanExecuteChanged += (_, _) => count++;
        model.Notify("Other");
        Assert.Equal(0, count);
    }

    [Fact]
    public void ConstructorsIdentifyNullPredicate()
    {
        Assert.Equal("canExecuteMethod", Assert.Throws<ArgumentNullException>(() => new DelegateCommand(() => { }, null!)).ParamName);
        Assert.Equal("canExecuteMethod", Assert.Throws<ArgumentNullException>(() => new DelegateCommand<string>(_ => { }, null!)).ParamName);
        Assert.Throws<InvalidCastException>(() => new DelegateCommand<int>(_ => { }));
    }

    [Fact]
    public void NullableCommandExecutesNullAndValueParameters()
    {
        int? actual = 0;
        var command = new DelegateCommand<int?>(value => actual = value);
        command.Execute(null);
        Assert.Null(actual);
        command.Execute(42);
        Assert.Equal(42, actual);
    }

    private sealed class Model : BindableBase
    {
        private Model? child;

        public bool Enabled => true;

        public Model? Child
        {
            get => this.child;
            set => this.SetProperty(ref this.child, value);
        }

        public void Notify(string? name) => this.RaisePropertyChanged(name);
    }
}
