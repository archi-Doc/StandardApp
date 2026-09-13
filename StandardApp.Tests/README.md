# Tests

Run from the repository root on Windows with the .NET 10 SDK and Windows SDK installed:

```powershell
dotnet test --project StandardApp.Tests/StandardApp.Tests.csproj -p:Platform=x64 --no-progress
```

Collect coverage:

```powershell
dotnet test --project StandardApp.Tests/StandardApp.Tests.csproj -p:Platform=x64 --no-progress --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml
```

The report is written to `TestResults/coverage.cobertura.xml`. The test project uses xUnit and Microsoft.Testing.Platform, configured by the repository's `global.json`.

Tests reference the production projects directly. They cover language registration, command observation, collection sorting, converters, arithmetic boundaries, DPI conversion, mutex recovery, WPF brush serialization, native window placement, and asynchronous dialog failures. WPF integration tests create hidden native windows on dedicated STA threads. URL validation tests reject input before launching any external process.

Assembly-level parallelization is disabled because localization uses shared static state. A dedicated registration test exercises concurrency explicitly. WinUI XAML initialization is disabled in the test host; these tests do not require a packaged WinUI application. Tests that need a live WinUI visual tree or MAUI application lifecycle require a separate UI test host.
