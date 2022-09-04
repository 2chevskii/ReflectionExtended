#load "build/build_data.cake"
#load "build/tasks/project/build.cake"
#load "build/tasks/project/restore.cake"
#load "build/tasks/project/test.cake"
#load "build/tasks/artifacts.cake"
#load "build/tasks/clean.cake"

Setup(context => {
  var data = new BuildData(context);



  return data;
});

Teardown<BuildData>((context, data) => {
  var repo = data.Git.GetRepository();
  repo.CheckoutPaths(data.Git.CommitHash,
    new[] {data.Paths.VersionProps.FullPath},
    new CheckoutOptions {
      CheckoutModifiers = CheckoutModifiers.Force
  });
});

Task("local/prepare").Does<BuildData>(data => {
  if(data.Git.Tag is not null) {
    var tagVersion = VersionData.GetTagVersion(data.Git);

    VersionData.WriteVersionProps(Context, data.Paths.VersionProps, tagVersion.Change(build: "local"));
  } else {
    var branchVersion = VersionData.SetBranchSuffix(data.Git, data.Version.VersionProps);

    VersionData.WriteVersionProps(Context, data.Paths.VersionProps, branchVersion.Change(build: "local"));
  }
});

Task("local").IsDependentOn("local/prepare")
             .IsDependentOn("clean/main")
             .IsDependentOn("clean/test")
             .IsDependentOn("clean/artifacts")
             .IsDependentOn("build/main")
             .IsDependentOn("build/test")
             .IsDependentOn("test/netstandard2")
             .IsDependentOn("test/net6")
             .IsDependentOn("artifacts/zip")
             .IsDependentOn("artifacts/pack");

var target = Argument("target", (string)null) ?? Argument("t", "build/main");

RunTarget(target);

/* Task("example").IsDependentOn("clean/main")
               .IsDependentOn("clean/test")
               .IsDependentOn("clean/artifacts");

RunTarget("example"); */
