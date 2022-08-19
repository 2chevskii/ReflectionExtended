using Path = System.IO.Path;
/*
  Target list:
    - build
    - test
    - pack
    - publish
    - ci
    - local

*/

readonly string RootDir = Path.GetFullPath(".");
readonly string MainProjectRoot = Path.GetFullPath("src/ReflectionExtended");
readonly string TestProjectRoot = Path.GetFullPath("test/ReflectionExtended.Tests");
readonly string MainProjectPath = Path.Combine(MainProjectRoot, "ReflectionExtended.csproj");
readonly string TestProjectPath = Path.Combine(TestProjectRoot, "ReflectionExtended.Tests.csproj");
readonly string ArtifactsRoot = Path.GetFullPath("artifacts");
readonly string LibArtifactsRoot = Path.Combine(ArtifactsRoot, "lib");
readonly string PackagesArtifactsRoot = Path.Combine(ArtifactsRoot, "packages");


DotNetBuildSettings GetBaseBuildSettings() {
  return new DotNetBuildSettings {
    NoRestore=true,
    NoDependencies=true,
    WorkingDirectory=RootDir,
    NoLogo = true
  };
}

/*
Tasks:
  restore/main
  restore/test

  build/main:debug
  build/main:release
  build/test:netstandard2.0
  build/test:net6.0


*/

#region Restore

Task("restore/main").Does(() => {
  Information("Restoring main project...");
  RestoreProject(MainProjectPath);
});
Task("restore/test").Does(() => {
  Information("Restoring test project...");
  RestoreProject(TestProjectPath);
});

#endregion

#region Build

/*
Build Tasks:
    build/all:
        build/main:
            build/main:debug:
                build/main:debug::netstandard2.0
                build/main:debug::net6.0
            build/main:release:
                build/main:release::netstandard2.0
                build/main:release::net6.0
        build/test:
            build/test:netstandard2
            build/test:net6
*/

Task("build/main:debug::netstandard2.0").IsDependentOn("restore/main")
.Does(() => BuildProject(MainProjectPath, "Debug", "netstandard2.0"));
Task("build/main:debug::net6.0").IsDependentOn("restore/main")
.Does(() => BuildProject(MainProjectPath, "Debug", "net6.0"));
Task("build/test:netstandard2").IsDependentOn("restore/test")
.IsDependentOn("build/main:debug::netstandard2.0")
.Does(() => BuildProject(TestProjectPath, "NetStandard2", "net6.0"));
Task("build/test:net6").IsDependentOn("restore/test")
.IsDependentOn("build/main:debug::net6.0").Does(() => BuildProject(TestProjectPath, "Net6", "net6.0"));
Task("build/main:debug").IsDependentOn("build/main:debug::netstandard2.0").IsDependentOn("build/main:debug::net6.0");

Task("build/main:release::netstandard2").IsDependentOn("restore/main").Does(() => BuildProject(MainProjectPath, "Release", "netstandard2.0"));
Task("build/main:release::net6.0").IsDependentOn("restore/main").Does(() => BuildProject(MainProjectPath, "Release", "net6.0"));
Task("build/main:release").IsDependentOn("build/main:release::netstandard2").IsDependentOn("build/main:release::net6.0");

#endregion

#region Test

Task("test/net6").Does(() => {
  Information("Running project tests targeting {0}", "net6.0");

  DotNetTest(TestProjectPath, new DotNetTestSettings {
    NoRestore = true,
    NoBuild = true,
    Configuration = "Net6"
  });
}).IsDependentOn("build/test:net6");
Task("test/netstandard2").Does(() => {
  Information("Running project tests targeting {0}", "netstandard2.0");

  DotNetTest(TestProjectPath, new DotNetTestSettings {
    NoRestore = true,
    NoBuild = true,
    Configuration = "NetStandard2"
  });
}).IsDependentOn("build/test:netstandard2");
Task("test").IsDependentOn("test/netstandard2").IsDependentOn("test/net6");

#endregion

void BuildProject(string path, string configuration, string framework) {
  string projectFileName = Path.GetFileName(path);
  Information("Building project {0}:{1}::{2}", projectFileName, configuration, framework);

  var settings = GetBaseBuildSettings();

  settings.Configuration = configuration;
  settings.Framework = framework;

  DotNetBuild(path, settings);
}

void RestoreProject(string path) {
    string projectFilename = Path.GetFileName(path);
    Information("Restoring project {0} in {1}", projectFilename, path);

    DotNetRestore(path, new DotNetRestoreSettings {WorkingDirectory = RootDir});
}

var target = Argument<string>("target", "build");

RunTarget(target);
