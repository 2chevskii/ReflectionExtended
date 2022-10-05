#load "../../build_data.cake"
#load "restore.cake"

/*
Main project configurations:
  - debug
  - release
Main project frameworks:
  - netstandard2.0
  - netstandard2.1
  - net6.0
Test project configurations:
  - debug
Test project frameworks:
  - netcoreapp2.1
  - netcoreapp3.1
  - net6.0

Build task syntax: build/<project>:<configuration>::<framework>

Hierarchy:
  build
    build/main
      build/main:debug
        build/main:debug::netstandard2.0
        build/main:debug::netstandard2.1
        build/main:debug::net6.0
      build/main:release
        build/main:release::netstandard2.0
        build/main:release::netstandard2.1
        build/main:release::net6.0
    build/test
      build/test:debug
        build/test:debug::netcoreapp2.0
        build/test:debug::netcoreapp2.1
        build/test:debug::net6.0

*/

Task("build/main:debug::netstandard2.0").IsDependeeOf("build/main:debug")
                                        .Does<BuildData>(data => DotNetBuild(
                                          data.Paths.MainProjectFile.FullPath,
                                          new DotNetBuildSettings {
                                            NoDependencies = true,
                                            NoLogo = true,
                                            NoRestore = true,
                                            Verbosity = DotNetVerbosity.Minimal,
                                            Configuration = "Debug",
                                            Framework = "netstandard2.0"
                                          }
                                        ));
Task("build/main:debug::netstandard2.1").IsDependeeOf("build/main:debug")
                                        .Does<BuildData>(data => DotNetBuild(
                                          data.Paths.MainProjectFile.FullPath,
                                          new DotNetBuildSettings {
                                            NoDependencies = true,
                                            NoLogo = true,
                                            NoRestore = true,
                                            Verbosity = DotNetVerbosity.Minimal,
                                            Configuration = "Debug",
                                            Framework = "netstandard2.1"
                                          }
                                        ));
Task("build/main:debug::net6.0").IsDependeeOf("build/main:debug")
                                .Does<BuildData>(data => DotNetBuild(
                                  data.Paths.MainProjectFile.FullPath,
                                  new DotNetBuildSettings {
                                    NoDependencies = true,
                                    NoLogo = true,
                                    NoRestore = true,
                                    Verbosity = DotNetVerbosity.Minimal,
                                    Configuration = "Debug",
                                    Framework = "net6.0"
                                  }
                                ));

Task("build/main:debug").IsDependeeOf("build/main");

Task("build/main:release::netstandard2.0").IsDependeeOf("build/main:release")
                                          .Does<BuildData>(data => DotNetBuild(
                                            data.Paths.MainProjectFile.FullPath,
                                            new DotNetBuildSettings {
                                              NoDependencies = true,
                                              NoLogo = true,
                                              NoRestore = true,
                                              Verbosity = DotNetVerbosity.Minimal,
                                              Configuration = "Release",
                                              Framework = "netstandard2.0"
                                            }
                                          ));
Task("build/main:release::netstandard2.1").IsDependeeOf("build/main:release")
                                          .Does<BuildData>(data => DotNetBuild(
                                            data.Paths.MainProjectFile.FullPath,
                                            new DotNetBuildSettings {
                                              NoDependencies = true,
                                              NoLogo = true,
                                              NoRestore = true,
                                              Verbosity = DotNetVerbosity.Minimal,
                                              Configuration = "Release",
                                              Framework = "netstandard2.1"
                                            }
                                          ));
Task("build/main:release::net6.0").IsDependeeOf("build/main:release")
                                  .Does<BuildData>(data => DotNetBuild(
                                    data.Paths.MainProjectFile.FullPath,
                                    new DotNetBuildSettings {
                                      NoDependencies = true,
                                      NoLogo = true,
                                      NoRestore = true,
                                      Verbosity = DotNetVerbosity.Minimal,
                                      Configuration = "Release",
                                      Framework = "net6.0"
                                    }
                                  ));

Task("build/main:release").IsDependeeOf("build/main");

Task("build/main").IsDependeeOf("build");

Task("build/test:debug::netcoreapp2.1").IsDependentOn("build/main:debug::netstandard2.0")
                                       .IsDependeeOf("build/test:debug")
                                       .Does<BuildData>(data => DotNetBuild(
                                          data.Paths.TestProjectFile.FullPath,
                                          new DotNetBuildSettings {
                                            NoDependencies = true,
                                            NoLogo = true,
                                            NoRestore = true,
                                            Verbosity = DotNetVerbosity.Minimal,
                                            Configuration = "Debug",
                                            Framework = "netcoreapp2.1"
                                          }
                                        ));
Task("build/test:debug::netcoreapp3.1").IsDependentOn("build/main:debug::netstandard2.1")
                                       .IsDependeeOf("build/test:debug")
                                       .Does<BuildData>(data => DotNetBuild(
                                          data.Paths.TestProjectFile.FullPath,
                                          new DotNetBuildSettings {
                                            NoDependencies = true,
                                            NoLogo = true,
                                            NoRestore = true,
                                            Verbosity = DotNetVerbosity.Minimal,
                                            Configuration = "Debug",
                                            Framework = "netcoreapp3.1"
                                          }
                                        ));
Task("build/test:debug::net6.0").IsDependentOn("build/main:debug::net6.0")
                                .IsDependeeOf("build/test:debug")
                                .Does<BuildData>(data => DotNetBuild(
                                  data.Paths.TestProjectFile.FullPath,
                                  new DotNetBuildSettings {
                                    NoDependencies = true,
                                    NoLogo = true,
                                    NoRestore = true,
                                    Verbosity = DotNetVerbosity.Minimal,
                                    Configuration = "Debug",
                                    Framework = "net6.0"
                                  }
                                ));

Task("build/test:debug").IsDependeeOf("build/test");
Task("build/test").IsDependeeOf("build");

Task("build");
