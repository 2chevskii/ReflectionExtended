#load ../data/build_data.cake

Task("clean/main").Does<BuildData>((context,data) => {
  var binDir = data.Paths.MainProjectRoot.Combine("bin");
  var objDir = data.Paths.MainProjectRoot.Combine("obj");

  Information("Cleaning project output directories, will delete:\n{0}\n{1}", binDir, objDir);

  EnsureDirectoryDoesNotExist(binDir);
  EnsureDirectoryDoesNotExist(objDir);
});

Task("clean/artifacts").Does<BuildData>((context,data) => {
  Information("Cleaning artifact directories, will delete:\n{0}", data.Paths.ArtifactsRoot);

  EnsureDirectoryDoesNotExist(data.Paths.ArtifactsRoot);
});
