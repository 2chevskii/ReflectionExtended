#load ../../build_data.cake

Task("restore/main").Does<BuildData>(data => {
  DotNetRestore(data.Paths.MainProjectRoot.FullPath, new DotNetRestoreSettings {
    NoDependencies = true,
    WorkingDirectory = data.Paths.Root
  });
});

Task("restore/test").Does<BuildData>(data => {
  DotNetRestore(data.Paths.TestProjectRoot.FullPath, new DotNetRestoreSettings {
    NoDependencies = true,
    WorkingDirectory = data.Paths.Root
  });
});
