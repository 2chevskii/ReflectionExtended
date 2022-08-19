#load data/build_data.cake

/* Task("ci").WithCriteria<BuildData>(data => !data.IsLocal)
          .IsDependentOn("ci/prepare")
          .IsDependentOn("clean/main")
          .IsDependentOn("clean/artifacts")
          .IsDependentOn("test")
          .IsDependentOn("artifacts/zip:main")
          .IsDependentOn("artifacts/nuget-pack:main")
          .Does<BuildData>(data => {

          }); */

/*

  set target version

  if tag write version and commit
  set version with build number

 */

Task("ci/prepare").Does<BuildData>((context, data) => {
  Information("Running CI build flow...");

  Verbose("Setting target build version...");
  SemVersion version;
  if(data.Version.HasTagVersion) {
    version = data.Version.Tag;

    Verbose("Creating commit with updated version {0} => {1}", data.Version.Source, version);
    context.WriteVersionProps(data.Paths, version);
    using var repo = data.Git.GetRepository();
    repo.Index.Add(data.Paths.VersionProps.FullPath);
    repo.Index.Write();
    Signature author = new Signature {

    };
    repo.Commit($"Bump version {data.Version.Source} => {version}");
  } else {
    version = context.CreateBranchVersion(data.Version.Source, data.Git.BranchName);
  }

  var currentBuildNumber = AppVeyor.Environment.Build.Number;
  Verbose("Appending AppVeyor build number ({0}) to target version ({1})", currentBuildNumber, version);

  version = context.CreateBuildNumberVersion(version, currentBuildNumber);


});

Task("ci").WithCriteria<BuildData>(data => !data.IsLocal);
