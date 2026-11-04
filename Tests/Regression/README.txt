Regression checks for the actual project sources, linked by Regression.csproj.

Run from the project root:
    dotnet run --project Tests/Regression/Regression.csproj

Requires .NET 6 SDK and this Unity project's resolved Newtonsoft package in Library/PackageCache.
No NuGet test packages or player save files are used. Test data is written to a unique temporary directory.

UnityStubs.cs substitutes only the engine/UI surfaces needed to exercise the application logic.
These checks do not replace Unity Play Mode tests or testing lifecycle, clipboard and file replacement on Android.
