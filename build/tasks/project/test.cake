#load "../../build_data.cake"
#load "build.cake"

static readonly Func<DirectoryPath, DirectoryPath, string, DotNetTestSettings> GetTestSettings = (workingDirectory, resultsDirectory, targetFramework) => {
 return new DotNetTestSettings {
  Blame = true,
  NoBuild = true,
  NoRestore = true,
  NoLogo = true,
  WorkingDirectory = workingDirectory,
  ResultsDirectory = resultsDirectory,
  Framework = targetFramework,
  Configuration = "Debug"
 };
};

/*

Tests are available on configuration 'Debug'
For the following target frameworks (test project fw => tested project fw):
  - netcoreapp2.1 => netstandard2.0
  - netcoreapp3.1 => netstandard2.1
  - net6.0 => net6.0

Results are places in the /test_results/<tested project fw>/ directory
*/

Task("test/netstandard2.0").IsDependentOn("build/main:debug::netstandard2.0")
                           .IsDependentOn("build/test:debug::netcoreapp2.1")
                           .Does<BuildData>(data => DotNetTest(
                             data.Paths.TestProjectFile.FullPath,
                             GetTestSettings(data.Paths.Root, data.Paths.Root.Combine("test_results").Combine("netstandard2.0"), "netcoreapp2.1"))
                           );

Task("test/netstandard2.1").IsDependentOn("build/main:debug::netstandard2.1")
                           .IsDependentOn("build/test:debug::netcoreapp3.1")
                           .Does<BuildData>(data => DotNetTest(
                             data.Paths.TestProjectFile.FullPath,
                             GetTestSettings(data.Paths.Root, data.Paths.Root.Combine("test_results").Combine("netstandard2.1"), "netcoreapp3.1")
                           ));

Task("test/net6.0").IsDependentOn("build/main:debug::net6.0")
                   .IsDependentOn("build/test:debug::net6.0")
                   .Does<BuildData>(data => DotNetTest(
                     data.Paths.TestProjectFile.FullPath,
                     GetTestSettings(data.Paths.Root, data.Paths.Root.Combine("test_results").Combine("net6.0"), "net6.0")
                   ));
