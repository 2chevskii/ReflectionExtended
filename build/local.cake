#load data/build_data.cake
#load data/build_git_info.cake
#load data/build_paths.cake
#load data/build_test_info.cake
#load data/build_version_info.cake
#load tasks/artifacts.cake
#load tasks/build.cake
#load tasks/clean.cake
#load tasks/restore.cake
#load tasks/test.cake
#load utils.cake

using Semver;
using SemVersion = Semver.SemVersion;

/*
  setup:
    if tag:
      set version from tag
    else:
      set version from version.props + build branch

  if tag:
    create release notes since last tag

  restore projects
  build projects
  test projects
  create artifacts

 */

Task("local/prepare").Does<BuildData>((context, data) => {
  Information("Running LOCAL build flow...");

  Verbose("Setting target build version...");
  SemVersion version;
  if(data.Version.HasTagVersion) {
    version = data.Version.Tag;
  } else {
    version = context.CreateBranchVersion(data.Version.Source, data.Git.BranchName);
  }
  Information("Target build version: {0}", version);
  context.WriteVersionProps(data.Paths, version);
  Verbose("Target version written to Version.props");

  if(data.Git.HasTag) {
    Information("Collecting release notes...", data.Git.TagName);
    var notes = context.CollectReleaseNotes(data);
    var notesFormatted = FormatReleaseNotes(notes);
    Information("Release notes {0}:", notesFormatted);
    Information(notesFormatted);
    Verbose("Not writing release notes anywhere due to local build");
  }

});

Task("local").WithCriteria<BuildData>(data => data.IsLocal, "LOCAL build flow is only allowed in LOCAL environemnts")
             .IsDependentOn("local/prepare")
             .IsDependentOn("clean/main")
             .IsDependentOn("clean/artifacts")
             .IsDependentOn("test")
             .IsDependentOn("artifacts/zip:main")
             .IsDependentOn("artifacts/nuget-pack:main");
