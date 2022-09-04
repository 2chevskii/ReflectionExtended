#load "../../build_data.cake"

Task("clean/main").Does<BuildData>(data => {
  var binDir = data.Paths.MainProjectRoot.Combine("bin");
  var objDir = data.Paths.MainProjectRoot.Combine("obj");

  Verbose("Deleting main project's <bin> dir: {0}", binDir);
  Verbose("Deleting main project's <obj> dir: {0}", objDir);

  EnsureDirectoryDoesNotExist(binDir);
  EnsureDirectoryDoesNotExist(objDir);
});

Task("clean/test").Does<BuildData>(data => {
  var binDir = data.Paths.TestProjectRoot.Combine("bin");
  var objDir = data.Paths.TestProjectRoot.Combine("obj");

  Verbose("Deleting test project's <bin> dir: {0}", binDir);
  Verbose("Deleting test project's <obj> dir: {0}", objDir);

  EnsureDirectoryDoesNotExist(binDir);
  EnsureDirectoryDoesNotExist(objDir);
});

Task("clean/projects").IsDependentOn("clean/main")
                      .IsDependentOn("clean/test");

Task("clean/artifacts:lib").Does<BuildData>(data => {
  Verbose("Cleaning artifacts lib directory: {0}", data.Paths.ArtifactsLib);

  foreach(var file in GetFiles(data.Paths.ArtifactsLib.Combine("*.zip").FullPath)) {
    Verbose("Deleting file: {0}", file.FullPath);

    DeleteFile(file);
  }
});

Task("clean/artifacts:packages").Does<BuildData>(data => {
  Verbose("Cleaning artifacts packages directory: {0}", data.Paths.ArtifactsPackages);

  foreach(var file in GetFiles(data.Paths.ArtifactsPackages.Combine("*.{nupkg,snupkg}").FullPath)) {
    Verbose("Deleting file: {0}", file.FullPath);

    DeleteFile(file);
  }
});

Task("clean/artifacts").IsDependentOn("clean/artifacts:lib")
                       .IsDependentOn("clean/artifacts:packages");
