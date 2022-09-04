#load ../../build_data.cake
#load restore.cake

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
                                        .Does<BuildData>(data => {
                                          DotNetBuild(data.Paths.MainProjectFile.FullPath, new DotNetBuildSettings {
                                            Configuration = "Debug",
                                            Framework = "netstandard2.0",
                                            NoDependencies = true,
                                            NoLogo = true,
                                            WorkingDirectory = data.Paths.Root,
                                            Verbosity = DotNetVerbosity.Minimal,
                                            NoRestore = true
                                          });
                                        });

Task("build/main:debug::net6.0").IsDependentOn("restore/main")
                                .Does<BuildData>(data => {
                                  DotNetBuild(data.Paths.MainProjectFile.FullPath, new DotNetBuildSettings {
                                    Configuration = "Debug",
                                    Framework = "net6.0",
                                    NoDependencies = true,
                                    NoLogo = true,
                                    WorkingDirectory = data.Paths.Root,
                                    Verbosity = DotNetVerbosity.Minimal,
                                    NoRestore = true
                                  });
                                });


Task("build/main:debug").IsDependentOn("build/main:debug::netstandard2.0")
                        .IsDependentOn("build/main:debug::net6.0");

Task("build/main:release::netstandard2.0").IsDependentOn("restore/main")
                                          .Does<BuildData>(data => {
                                            DotNetBuild(data.Paths.MainProjectFile.FullPath, new DotNetBuildSettings {
                                              Configuration = "Release",
                                              Framework = "netstandard2.0",
                                              NoDependencies = true,
                                              NoLogo = true,
                                              WorkingDirectory = data.Paths.Root,
                                              Verbosity = DotNetVerbosity.Minimal,
                                              NoRestore = true
                                            });
                                          });
Task("build/main:release::net6.0").IsDependentOn("restore/main")
                                  .Does<BuildData>(data => {
                                    DotNetBuild(data.Paths.MainProjectFile.FullPath, new DotNetBuildSettings {
                                      Configuration = "Release",
                                      Framework = "net6.0",
                                      NoDependencies = true,
                                      NoLogo = true,
                                      WorkingDirectory = data.Paths.Root,
                                      Verbosity = DotNetVerbosity.Minimal,
                                      NoRestore = true
                                    });
                                  });
Task("build/main:release").IsDependentOn("build/main:release::netstandard2.0")
                          .IsDependentOn("build/main:release::net6.0");

Task("build/main").IsDependentOn("build/main:debug")
                  .IsDependentOn("build/main:release");

Task("build/test:netstandard2").IsDependentOn("restore/test")
                               .IsDependentOn("build/main:debug::netstandard2.0")
                               .Does<BuildData>(data => {
                                  DotNetBuild(data.Paths.TestProjectFile.FullPath, new DotNetBuildSettings {
                                    Configuration = "NetStandard2",
                                    Framework = "net6.0",
                                    NoDependencies = true,
                                    NoLogo = true,
                                    WorkingDirectory = data.Paths.Root,
                                    Verbosity = DotNetVerbosity.Minimal,
                                    NoRestore = true
                                  });
                               });
Task("build/test:net6").IsDependentOn("restore/test")
                       .IsDependentOn("build/main:debug::net6.0")
                       .Does<BuildData>(data => {
                         DotNetBuild(data.Paths.TestProjectFile.FullPath, new DotNetBuildSettings {
                           Configuration = "Net6",
                           Framework = "net6.0",
                           NoDependencies = true,
                           NoLogo = true,
                           WorkingDirectory = data.Paths.Root,
                           Verbosity = DotNetVerbosity.Minimal,
                           NoRestore = true
                         });
                       });
Task("build/test").IsDependentOn("build/test:netstandard2")
                  .IsDependentOn("build/test:net6");
