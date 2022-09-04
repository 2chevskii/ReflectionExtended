#load ../build_data.cake
#load project/build.cake

Task("artifacts/zip:netstandard2.0").IsDependentOn("build/main:release::netstandard2.0")
                                    .Does<BuildData>(data => {
                                      var src = data.Paths.MainProjectRoot.Combine("bin").Combine("Release").Combine("netstandard2.0");
                                      var dest = data.Paths.ArtifactsLib.CombineWithFilePath("netstandard2.0.zip");

                                      Verbose("Compressing lib artifact (netstandard2.0): {0} => {1}", src, dest);

                                      Zip(src, dest);
                                    });
Task("artifacts/zip:net6.0").IsDependentOn("build/main:release::net6.0")
                            .Does<BuildData>(data => {
                              var src = data.Paths.MainProjectRoot.Combine("bin").Combine("Release").Combine("net6.0");
                              var dest = data.Paths.ArtifactsLib.CombineWithFilePath("net6.0.zip");

                              Verbose("Compressing lib artifact (net6.0): {0} => {1}", src, dest);

                              Zip(src, dest);
                            });
Task("artifacts/zip").IsDependentOn("artifacts/zip:netstandard2.0")
                     .IsDependentOn("artifacts/zip:net6.0");

Task("artifacts/pack").IsDependentOn("build/main:release")
                      .Does<BuildData>(data => {
                        Verbose("Creating NuGet package for project {0}", data.Paths.MainProjectFile.GetFilenameWithoutExtension());

                        DotNetPack(data.Paths.MainProjectFile.FullPath, new DotNetPackSettings {
                          WorkingDirectory = data.Paths.Root,
                          Configuration = "Release",
                          NoBuild = true,
                          NoDependencies = true,
                          NoRestore = true,
                          NoLogo = true,
                          IncludeSymbols = true,
                          SymbolPackageFormat = "snupkg",
                          Verbosity = DotNetVerbosity.Normal,
                          OutputDirectory = data.Paths.ArtifactsPackages
                        });
                      });

Task("artifacts/push:appveyor").IsDependentOn("artifacts/zip")
                               .IsDependentOn("artifacts/pack")
                               .Does<BuildData>(data => throw new NotImplementedException());
Task("artifacts/push:nuget").IsDependentOn("artifacts/pack")
                            .Does<BuildData>(data => throw new NotImplementedException());

Task("artifacts/push:github").IsDependentOn("artifacts/zip")
                             .IsDependentOn("artifacts/pack")
                             .Does<BuildData>(data => throw new NotImplementedException());
