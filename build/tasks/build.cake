#load ../data/build_data.cake
#load ../utils.cake

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
                                        .Does<BuildData>(data =>
                                          BuildProject(data.Paths.Root, data.Paths.MainProjectFile, "Debug", "netstandard2.0")
                                        );
Task("build/main:debug::net6.0").IsDependentOn("restore/main")
                                .Does<BuildData>(data =>
                                  BuildProject(data.Paths.Root, data.Paths.MainProjectFile, "Debug", "net6.0")
                                );
Task("build/test:netstandard2").IsDependentOn("restore/test")
                               .IsDependentOn("build/main:debug::netstandard2.0")
                               .Does<BuildData>(data =>
                                 BuildProject(data.Paths.Root, data.Paths.TestProjectFile, "NetStandard2", "net6.0")
                               );
Task("build/test:net6").IsDependentOn("restore/test")
                       .IsDependentOn("build/main:debug::net6.0")
                       .Does<BuildData>(data =>
                         BuildProject(data.Paths.Root, data.Paths.TestProjectFile, "Net6", "net6.0")
                       );
Task("build/test").IsDependentOn("build/test:netstandard2")
                  .IsDependentOn("build/test:net6");
Task("build/main:debug").IsDependentOn("build/main:debug::netstandard2.0")
                        .IsDependentOn("build/main:debug::net6.0");

Task("build/main:release::netstandard2.0").IsDependentOn("restore/main")
                                          .Does<BuildData>(data =>
                                            BuildProject(data.Paths.Root, data.Paths.MainProjectFile, "Release", "netstandard2.0")
                                          );
Task("build/main:release::net6.0").IsDependentOn("restore/main")
                                  .Does<BuildData>(data =>
                                    BuildProject(data.Paths.Root, data.Paths.MainProjectFile, "Release", "net6.0")
                                  );
Task("build/main:release").IsDependentOn("build/main:release::netstandard2.0")
                          .IsDependentOn("build/main:release::net6.0");
Task("build/main").IsDependentOn("build/main:debug")
                  .IsDependentOn("build/main:release");
