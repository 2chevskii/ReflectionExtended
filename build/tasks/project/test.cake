#load "../../build_data.cake"

Task("test/netstandard2").IsDependentOn("build/main:debug::netstandard2.0")
                         .IsDependentOn("build/test:netstandard2")
                         .Does<BuildData>(data => {
                          DotNetTest(data.Paths.TestProjectFile.FullPath, new DotNetTestSettings {
                            Blame = true,
                            Configuration = "NetStandard2",
                            NoBuild = true,
                            NoRestore = true,
                            NoLogo = true,
                            WorkingDirectory = data.Paths.Root
                          });
                         });
Task("test/net6").IsDependentOn("build/main:debug::net6.0")
                 .IsDependentOn("build/test:net6")
                 .Does<BuildData>(data => {
                  DotNetTest(data.Paths.TestProjectFile.FullPath, new DotNetTestSettings {
                    Blame = true,
                    Configuration = "Net6",
                    NoBuild = true,
                    NoRestore = true,
                    NoLogo = true,
                    WorkingDirectory = data.Paths.Root
                  });
                 });
