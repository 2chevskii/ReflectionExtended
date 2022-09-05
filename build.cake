#addin nuget:?package=Cake.Incubator&version=7.0.0

#load "build/build_data.cake"
#load "build/tasks/project/build.cake"
#load "build/tasks/project/restore.cake"
#load "build/tasks/project/test.cake"
#load "build/tasks/artifacts.cake"
#load "build/tasks/clean.cake"

using System.Text.Json;

Setup(context => {
  var data = new BuildData(context);



  return data;
});

Teardown<BuildData>((context, data) => {
  var repo = data.Git.GetRepository();

  /* Revert version.props to it's commited state to wipe out build meta and branch suffixes */
  repo.CheckoutPaths(data.Git.CommitHash,
    new[] {data.Paths.VersionProps.FullPath},
    new CheckoutOptions {
      CheckoutModifiers = CheckoutModifiers.Force
  });

  repo.Dispose();
});

Task("local/prepare").Does<BuildData>(data => {
  if(data.Git.Tag is not null) {
    data.Version.TagVersion = VersionData.GetTagVersion(data.Git);
    data.Version.TargetVersion = data.Version.TagVersion;
  } else {
    data.Version.BranchVersion = VersionData.SetBranchSuffix(data.Git, data.Version.VersionProps);
    data.Version.TargetVersion = data.Version.BranchVersion;
  }

  data.Version.TargetVersionWithBuildNumber = data.Version.TargetVersion.Change(build: "local");
  VersionData.WriteVersionProps(Context, data.Paths.VersionProps, data.Version.TargetVersionWithBuildNumber);

  // Collect release notes
  var rnCommits = data.Git.GetCommitsForReleaseNotes();
  StringBuilder sb = new();
  // Format release notes
  foreach(var commit in rnCommits) {
    sb.AppendFormat("- {0} {1} // {2} {3}\n", commit.Sha[..8], commit.MessageShort, commit.Author.Name, commit.Author.When);
  }

  Information("Release notes:\n" + sb.ToString());
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
