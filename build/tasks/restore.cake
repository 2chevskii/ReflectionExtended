#load ../data/build_data.cake
#load ../utils.cake

Task("restore/main").Does<BuildData>(data => {
  Information("Restoring main project...");
  RestoreProject(data.Paths.Root, data.Paths.MainProjectFile);
});
Task("restore/test").Does<BuildData>(data => {
  Information("Restoring test project...");
  RestoreProject(data.Paths.Root, data.Paths.TestProjectFile);
});
