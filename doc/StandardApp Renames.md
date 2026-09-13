# StandardApp Renames

Date: 2026-09-13 (branch `dev`)

Public names were renamed for clarity and consistency. Behavior is unchanged except for the bug fixes listed at the end.
Tables are `Old` → `New`. Unless stated otherwise, only the name changed (same signature and semantics).

## Arc.WinUI (library)

Projects that reference `Arc.WinUI` need these changes.

### Types

| Old | New |
|---|---|
| `UiHelper` | `UIHelper` |
| `WINDOWPLACEMENT` (struct) | `NativeWindowPlacement` |
| `POINT` (struct) | `NativePoint` |
| `RECT` (struct) | `NativeRect` |

### Members

| Old | New | Notes |
|---|---|---|
| `IApp.UiDispatcherQueue` / `AppBase.UiDispatcherQueue` | `UIDispatcherQueue` | |
| `IApp.DataFolder` / `AppBase.DataFolder` | `DataDirectory` | |
| `IApp.TryExit(CancellationToken)` / `AppBase.TryExit` | `TryExitAsync` | Update `override` in your `App`. |
| `IApp.NavigatingHandler(object, NavigatingCancelEventArgs)` | `OnFrameNavigating` | `frame.Navigating += app.OnFrameNavigating;` |
| `IMessageDialogService.Show(title, content, primaryCommand, cancelCommand, secondaryCommand, cancellationToken)` | `ShowAsync(title, content, primaryButtonText, cancelButtonText, secondaryButtonText, cancellationToken)` | Update explicit interface implementations. |
| `UiHelper.ShowMessageDialogAsync(service, title, content, primaryCommand, cancelCommand, secondaryCommand, ...)` | `UIHelper.ShowMessageDialogAsync(service, titleHash, contentHash, primaryButtonHash, cancelButtonHash, secondaryButtonHash, ...)` | Parameter names only. |
| `UiHelper.PreventMultipleInstances(Mutex)` | `UIHelper.TryActivateRunningInstance(Mutex)` | Returns true if another instance is running. |
| `WindowExtensions.ShowMessageDialogAsync(window, title, content, primaryCommand, cancelCommand, secondaryCommand, ...)` | `...(window, title, content, primaryButtonText, cancelButtonText, secondaryButtonText, ...)` | Parameter names only. |
| `WindowExtensions.ActivateWindow(this Window, bool force)` | `BringToForeground` | |
| `WindowExtensions.LoadWindowPlacement(this Window, DipWindowPlacement)` | `ApplyWindowPlacement` | |
| `WindowExtensions.SaveWindowPlacement(this Window)` | `GetWindowPlacement` | |
| `LanguageList.LanguageFile` | `LanguageFileFormat` | |
| `LanguageList.LoadHashedString(Assembly)` | `LoadHashedStrings` | |
| `BrushOption.ColorChanged` | `IsColorChanged` | Serialization key unchanged. |
| `BrushOption.ColorInt` | `ColorArgb` | Serialization key unchanged. |
| `BrushOption.Change(Color)` | `SetColor(Color)` | |
| `StringerExtension.Source` (ctor param `source`) | `Key` (`key`) | XAML: `{Arc:Stringer Source=X}` → `{Arc:Stringer Key=X}` |
| `StringerBindingSource.LanguageChanged()` | `NotifyLanguageChanged()` | |
| `DipWindowPlacement.ShowCmd` | `ShowCommand` | Serialization key unchanged. |
| `DipWindowPlacement(WINDOWPLACEMENT wp, ...)` | `DipWindowPlacement(NativeWindowPlacement windowPlacement, ...)` | |
| `DipWindowPlacement.FromWINDOWPLACEMENT` / `ToWINDOWPLACEMENT` | `FromWindowPlacement` / `ToWindowPlacement` | |
| `DipWindowPlacement.ToWINDOWPLACEMENT2` | `ToWindowPlacementWithPhysicalPosition` | Added counterpart: `FromWindowPlacementWithPhysicalPosition`. |
| `DipPoint.FromPOINT` / `ToPOINT` | `FromPoint` / `ToPoint` | |
| `DipPoint.FromPOINT2` / `ToPOINT2` | `FromPointUnscaled` / `ToPointUnscaled` | |
| `DipRect.FromRECT` / `ToRECT` | `FromRect` / `ToRect` | |
| `DipRect.FromRECT2` / `ToRECT2` | `FromRectWithPhysicalPosition` / `ToRectWithPhysicalPosition` | |

### Files

`Interfaces/BaseApp.cs` → `Interfaces/AppBase.cs`, `UiHelper.cs` → `UIHelper.cs`

## StandardConsole

| Old | New |
|---|---|
| `TestCommand2` (file `TestCommand2.cs`) | `Test2Command` (`Test2Command.cs`) |
| `ConsoleUnit.Product.Param(string Args)` | `ConsoleUnit.Product.RunParameters(string Arguments)` |
| `ConsoleUnit.Product.RunAsync(Param param)` | `RunAsync(RunParameters parameters)` |
| `TestCommand.Execute(TestOptions option, ...)` | `Execute(TestOptions options, ...)` |

## StandardWinUI

| Old | New | Notes |
|---|---|---|
| `Entrypoint` | `EntryPoint` | csproj `StartupObject` updated. File `EntryPoint.cs`. |
| `Entrypoint.UiDispatcherQueue` / `Entrypoint.DataFolder` | `EntryPoint.UIDispatcherQueue` / `EntryPoint.DataDirectory` | |
| `NaviWindow` | `MainWindow` | Files `0.Navi/MainWindow.xaml(.cs)`. |
| `SettingsState` | `SettingsPageState` | File renamed. |
| `InformationState` | `InformationPageState` | File renamed. |
| `AppUnit.Product.Param` / `RunAsync(Param param)` | `RunParameters(string Arguments)` / `RunAsync(RunParameters parameters)` | |
| `AppSettings.Filename` | `AppSettings.FileName` | |
| `AppSettings.Baibai` | `AppSettings.BaibainNumber` | `[Key("Baibai")]` keeps the saved key. |
| `AppSettings.BrushTest` | `AppSettings.TestBrush` | `[Key("BrushTest")]` keeps the saved key. |
| `AppSettings.OnAfterDeserialize` / `OnBeforeSerialize` | `OnDeserialized` / `OnSerializing` | |
| `AdvancedPageState(IApp, AppSettings, IMessageDialogService simpleWindowService)` | `...(..., IMessageDialogService messageDialogService)` | |
| `StatePageState.Baibain` / `AdvancedPageState.Baibain` (`BaibainCommand`) | `Multiply` (`MultiplyCommand`) | XAML `x:Bind` updated. |
| `MessagePageState.Test` (`TestCommand`) | `ShowSampleDialog` (`ShowSampleDialogCommand`) | XAML `x:Bind` updated. |
| `SettingsPageState.SelectScaling(double scaling)` (`SelectScalingCommand`) | `SelectViewScale(double scale)` (`SelectViewScaleCommand`) | |
| `SettingsPageState.ScalingText` | `ViewScaleText` | |
| `TestItem.Selection` | `TestItem.SelectionState` | |
| `TestItem(int id, DateTime dt)` | `TestItem(int id, DateTime dateTime)` | |

## StandardWPF

### Application (`namespace Application`)

| Old | New |
|---|---|
| `App.Initialized` / `App.SessionEnding` | `App.IsInitialized` / `App.IsSessionEnding` |
| `App.UI` | `App.UIDispatcher` |
| `App.LocalDataFolder` | `App.DataDirectory` |
| `App.InvokeAsyncOnUI(Action)` | `App.ExecuteOrEnqueueOnUI(Action)` |
| `AppClass` (App.xaml `x:Class`) | `WpfApplication` |
| `AppConst.AppDataFolder` / `AppConst.AppDataFile` | `AppConst.DataFolderName` / `AppConst.DataFileName` |
| `AppSettings.LoadError` | `AppSettings.HasLoadError` |
| `AppSettings.OnAfterDeserialize` | `AppSettings.OnDeserialized` |
| `AppOptions.BrushTest` | `AppOptions.TestBrush` |
| `TestItem.Selection` / ctor param `dt` | `TestItem.SelectionState` / `dateTime` |

### MainViewModel

| Old | New |
|---|---|
| `TestGoshujin` | `TestItems` |
| `Number1` / `Number2` | `Addend1` / `Addend2` |
| `Number3Value` (field `number3`) | `SumValue` (`sum`) |
| `Number4Value` (field `number4`) | `ToggleCountValue` (`toggleCount`) |
| `CommandFlagValue` (field `commandFlag`) | `IsToggleBrushColorEnabledValue` (`isToggleBrushColorEnabled`) |
| `CommandAddItem` / `CommandClearItem` | `AddItemCommand` / `ClearItemsCommand` |
| `CommandListViewIncrement` / `CommandListViewDecrement` | `IncrementSelectedIdCommand` / `DecrementSelectedIdCommand` |
| `CommandMessageId` | `SendMessageIdCommand` |
| `TestCommand` | `ToggleBrushColorCommand` |
| `TestCommand2` / `TestCommand3` | `ExitWithoutConfirmationCommand` / `ShowDescriptionDialogCommand` |
| `TestCommand4` / `TestCommand5` / `TestCommand6` | `ToggleEnabledStateCommand` / `ShowYesNoDialogCommand` / `ShowCustomDialogCommand` |
| `TestCrossChannel` / `TestCrossChannel2` | `TestCrossChannelCommand` / `TestCrossChannel2Command` |
| `Time1` | `CreatedTime` |

### IMainViewService / MainWindow / SettingsWindow

| Old | New |
|---|---|
| `Notification(NotificationMessage msg)` | `ShowNotification(NotificationMessage message)` |
| `MessageID(MessageId id)` | `HandleMessage(MessageId id)` |
| `Dialog(DialogParam p)` | `ShowDialogAsync(DialogParameters parameters)` |
| `CustomDialog(DialogParam p)` | `ShowCustomDialog(DialogParameters parameters)` |
| `MainWindow.CrossChannel_Dialog(DialogParam p)` | `MainWindow.ShowCrossChannelDialogAsync(DialogParameters parameters)` |
| `SettingsWindow.CultureList` | `SettingsWindow.Cultures` |
| `SettingsWindow.DisplayScaling` (List&lt;double&gt;) | `SettingsWindow.DisplayScalingOptions` |
| `SettingsWindow.LicenseTextCommand` | `SettingsWindow.ShowLicenseCommand` |

### Arc.WPF

| Old | New | Notes |
|---|---|---|
| `BooleanToVisibilityConverter` | `InverseBooleanToVisibilityConverter` | It always inverts. |
| `TextBoxAttachment` | `TextBoxBehavior` | |
| attached property `IsSelectAllOnGotFocus` (`IsSelectAllOnGotFocusProperty`, `Get/SetIsSelectAllOnGotFocus`) | `SelectAllOnGotFocus` (`SelectAllOnGotFocusProperty`, `Get/SetSelectAllOnGotFocus`) | |
| `Methods` | `VisualTreeUtility` | |
| `Methods.FindAncestor(DependencyObject targetParent, DependencyObject dependencyObject)` → bool | `VisualTreeUtility.IsDescendantOf(DependencyObject element, DependencyObject ancestor)` | ⚠ Argument order is swapped. |
| `Methods.Sort<T>(this ObservableCollection<T>, Comparison<T>)` | `ObservableCollectionExtensions.Sort<T>` | Moved; extension call syntax unchanged. |
| `ListViewDD` / `ListViewItemDD` | `DragDropListView` / `DragDropListViewItem` | XAML updated. |
| `ListViewDD.DropMoveAction` | `DragDropListView.ItemMovedAction` | |
| `ListViewDD.GetItem(Point pos)` | `DragDropListView.GetItemAt(Point position)` | |
| `DragAdorner(owner, adornElement, opacity, dragPos)` | `DragAdorner(owner, adornedElement, opacity, dragPosition)` | |
| `DialogParam` (field `Hashed`) | `DialogParameters` (field `MessageHash`) | |
| `Dialog` (ctor param `p`) | `MessageDialog` (`parameters`) | XAML `x:Class` updated. |
| `DialogTextBox` | `TextInputDialog` | XAML `x:Class` updated. |
| `DialogTextBoxParam` (fields `Hashed`, `CheckText`, `CheckTextAsync`) | `TextInputDialogParameters` (`MessageHash`, `ValidateText`, `ValidateTextAsync`) | |
| `DialogTextBoxResult` | `TextInputDialogResult` | |
| `DialogTextBox.CheckText` / `CheckTextAsync` | `TextInputDialog.ValidateText` / `ValidateTextAsync` | |
| `CheckTextDelegate` / `CheckTextAsyncDelegate` | `TextValidator` / `AsyncTextValidator` | |
| `BrushOption.ChangedFlag` / `BrushColor` / `Change(Color)` | `IsColorChanged` / `ColorArgb` / `SetColor(Color)` | Serialization keys unchanged. |
| `BrushOption.OnAfterDeserialize` / `OnBeforeSerialize` | `OnDeserialized` / `OnSerializing` | |
| `C5Extension` | `HashedStringExtension` | |
| `StringerBindingSource.CultureChanged()` | `NotifyCultureChanged()` | |
| `StringerUpdater` | `Stringer` | |
| `StringerUpdater.StringerAddExtensionObject(...)` / `StringerUpdate()` | `Stringer.Register(...)` / `Stringer.Refresh()` | |

### Arc.WinAPI

| Old | New |
|---|---|
| `Methods` | `NativeMethods` |
| `Extensions.ToLoWord(this IntPtr dword)` / `ToHiWord` | `IntPtrExtensions.GetLowWord(this IntPtr value)` / `GetHighWord` |
| `Const` | `ClipboardFormats` |
| `POINT32` | `NativeCursorPoint` |
| `DipWindowPlacement.ShowCmd` | `ShowCommand` |
| `DipWindowPlacement.FromWINDOWPLACEMENT` / `ToWINDOWPLACEMENT` | `FromWindowPlacement` / `ToWindowPlacement` |
| `DipWindowPlacement.FromWINDOWPLACEMENT2` / `ToWINDOWPLACEMENT2` | `FromWindowPlacementWithPhysicalPosition` / `ToWindowPlacementWithPhysicalPosition` |
| `DipPoint.FromPOINT` / `ToPOINT` / `FromPOINT2` / `ToPOINT2` | `FromPoint` / `ToPoint` / `FromPointUnscaled` / `ToPointUnscaled` |
| `DipRect.FromRECT` / `ToRECT` / `FromRECT2` / `ToRECT2` | `FromRect` / `ToRect` / `FromRectWithPhysicalPosition` / `ToRectWithPhysicalPosition` |

## Compatibility notes

- Saved data: every renamed serialized member keeps its key (integer keys, or an explicit `[Key("OldName")]` in StandardWinUI `AppSettings`). Existing settings and data files load unchanged.
- XAML: update `{Arc:Stringer Source=...}` → `{Arc:Stringer Key=...}`, and any `x:Bind`/`Binding` paths that use renamed commands or properties.
- Not renamed: namespaces (`Application`, `ConsoleApp1`), enums and their members (`ShowCommand`, `SW`, `ShowCommands`, ...), public fields of Win32 structs, string resource keys (e.g. `Baibain.Enter`), and folder names (`0.Navi`, `2.Baibain`).
- Docs: `Presentation-State Model/Presentation-State Model (WinUI).md` code samples were updated.

## Bug fixes

1. StandardWPF `MainWindow.xaml`: bound to `Number4` and `HideDialogButton`, which do not exist (ValueLink only generates `...Value` properties). The counter never showed, and the dialog buttons never hid. Now bound to `ToggleCountValue` / `HideDialogButtonValue`.
2. Arc.WPF `Transformer`: the layout transform used `ScaleX` for both axes (`new ScaleTransform(ScaleX, ScaleX)`). It now uses `ScaleY` for the vertical axis.
3. Arc.WinAPI `NativeMethods.GetPathFromIDList`: pointers were truncated to 32 bits (`(int)p`), which fails on 64-bit, and the HGlobal buffer was never freed. It now uses `IntPtr.Add` and `Marshal.FreeHGlobal`.
4. `GetWindowHandle` (Arc.WinUI internal, Arc.WinAPI): removed an unused `Process.GetProcessById(id)` call, which could throw inside the `EnumDesktopWindows` callback.
5. StandardWPF startup: if the saved culture was empty and the UI culture was `ja-JP`, `Settings.Culture` stayed empty. It is now set to `"ja"`.
6. StandardWinUI `AdvancedPageState.Exit`: the `CancellationTokenSource` is now disposed.
