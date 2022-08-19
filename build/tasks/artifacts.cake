#load ../data/build_data.cake
#load clean.cake

Task("artifacts/zip:main::netstandard2.0").IsDependentOn("build/main:release::netstandard2.0")
                                          .Does<BuildData>(
data => {
  EnsureDirectoryExists(data.Paths.ArtifactsLib);
  var sourcePath = data.Paths.MainProjectRoot.Combine("bin").Combine("Release").Combine("netstandard2.0");
  var destinationPath = data.Paths.ArtifactsLib.CombineWithFilePath("netstandard2.0.zip");

  Verbose("Compressing zip archive from release/netstandard2.0 configuration...");
  Zip(sourcePath, destinationPath);
  Verbose("Zipped successfully: {0} => {1}", sourcePath, destinationPath);
});
Task("artifacts/zip:main::net6.0").IsDependentOn("build/main:release::net6.0")
                                  .Does<BuildData>(
data => {
  EnsureDirectoryExists(data.Paths.ArtifactsLib);
  var sourcePath = data.Paths.MainProjectRoot.Combine("bin").Combine("Release").Combine("net6.0");
  var destinationPath = data.Paths.GetPathForLibArtifact("net6.0");

  Verbose("Generating zip archive from release/net6.0 configuration...", sourcePath, destinationPath);
  Zip(sourcePath, destinationPath);
  Verbose("Zipped successfully: {0} => {1}", sourcePath, destinationPath);
});
Task("artifacts/zip:main").IsDependentOn("artifacts/zip:main::netstandard2.0")
                          .IsDependentOn("artifacts/zip:main::net6.0");
Task("artifacts/nuget-pack:main").IsDependentOn("build/main:release")
                                 .Does<BuildData>(
data => {
    EnsureDirectoryExists(data.Paths.ArtifactsPackages);
    DotNetPack(data.Paths.MainProjectFile.FullPath, new DotNetPackSettings {
        Configuration = "Release",
        IncludeSymbols = true,
        NoBuild = true,
        NoDependencies = true,
        WorkingDirectory = data.Paths.MainProjectRoot,
        SymbolPackageFormat = "snupkg",
        NoRestore = true,
        OutputDirectory = data.Paths.ArtifactsPackages,
        Verbosity = DotNetVerbosity.Normal
    });
});

Task("artifacts/upload-appveyor").IsDependentOn("clean/artifacts")
                                 .IsDependentOn("artifacts/zip:main")
                                 .IsDependentOn("artifacts/nuget-pack:main")
                                 .Does<BuildData>(data => {
                                  Verbose("Uploading artifacts to AppVeyor...");

                                  var libsGlob = data.Paths.ArtifactsLib.Combine("*.zip").FullPath;
                                  Debug("Libs glob: {0}", libsGlob);
                                  var libsFiles = GetFiles(libsGlob);
                                  foreach(var file in libsFiles) {
                                    Verbose("Discovered lib artifact: {0}", file.GetFilename());

                                    AppVeyor.UploadArtifact(file.FullPath, new AppVeyorUploadArtifactsSettings {
                                      DeploymentName = file.GetFilename().ToString()
                                    });
                                  }

                                  var packagesGlob = data.Paths.ArtifactsPackages.Combine("*.?(s)nupkg").FullPath;
                                  Debug("Packages glob: {0}", packagesGlob);
                                  var packagesFiles = GetFiles(packagesGlob);
                                  foreach(var file in packagesFiles) {
                                    Verbose("Discovered package artifact: {0}", file.GetFilename());

                                    AppVeyor.UploadArtifact(file.FullPath, new AppVeyorUploadArtifactsSettings {
                                      DeploymentName = file.GetFilename().ToString(),
                                      ArtifactType = AppVeyorUploadArtifactType.NuGetPackage
                                    });
                                  }

                                  Verbose("Successfully uploaded AppVeyor artifacts");
                                 });

Task("artifacts/nuget-push").IsDependentOn("clean/artifacts")
                            .IsDependentOn("artifacts/nuget-pack:main")
                            .Does<BuildData>(
data => {


  /* var packagePath = data.Paths.GetPathForPackageArtifact("ReflectionExtended");
  var symbolPackagePath = data.Paths.GetPathForPackageArtifact()
  DotNetNuGetPush(packagePath, new DotNetNuGetPushSettings {

  }); */

  // DotNetNuGetPush()
});
