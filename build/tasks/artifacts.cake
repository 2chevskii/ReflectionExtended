#load "../build_data.cake"
#load "project/build.cake"

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
                               .WithCriteria(context => context.AppVeyor().IsRunningOnAppVeyor, "How am I supposed to upload artifacts to AppVeyor while not running build there?!")
                               .Does<BuildData>(data => {
                                var stdZip = data.Paths.ArtifactsLib.CombineWithFilePath("netstandard2.0.zip");
                                AppVeyor.UploadArtifact(stdZip,
                                  new AppVeyorUploadArtifactsSettings {
                                  DeploymentName = stdZip.GetFilename().FullPath
                                });
                                var net6Zip = data.Paths.ArtifactsLib.CombineWithFilePath("net6.0.zip");
                                AppVeyor.UploadArtifact(net6Zip,
                                  new AppVeyorUploadArtifactsSettings {
                                  DeploymentName = net6Zip.GetFilename().FullPath
                                });

                                foreach(var pkg in GetFiles(data.Paths.ArtifactsPackages.CombineWithFilePath("*.{nupkg,snupkg}").FullPath)) {
                                  AppVeyor.UploadArtifact(pkg, new AppVeyorUploadArtifactsSettings {
                                    DeploymentName = pkg.GetFilename().FullPath
                                  });
                                }
                               });
Task("artifacts/push:nuget").IsDependentOn("artifacts/pack")
                            .WithCriteria<BuildData>(data => throw new NotImplementedException(), "Pushing NuGet packages is only allowed while running tag build")
                            .WithCriteria(context => context.AppVeyor().IsRunningOnAppVeyor, "Pushing artifacts to NuGet is only allowed from AppVeyor") // + when release
                            .Does<BuildData>(data => {
                              foreach(var pkg in GetFiles(data.Paths.ArtifactsPackages.CombineWithFilePath("*.nupkg").FullPath)) {

                              }
                            });

Task("artifacts/push:github").IsDependentOn("artifacts/zip")
                             .IsDependentOn("artifacts/pack")
                             .WithCriteria<BuildData>(data => throw new NotImplementedException(), "Pushing artifacts to GitHub release is only allowed while running tag build")
                             .WithCriteria(context => context.AppVeyor().IsRunningOnAppVeyor, "Pushing artifacts to GitHub release is only allowed from AppVeyor")
                             .Does<BuildData>(data => throw new NotImplementedException());
