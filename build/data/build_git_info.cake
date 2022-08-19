#addin nuget:?package=LibGit2Sharp&version=0.27.0-preview-0182&prerelease&loaddependencies=true

#load build_data.cake

using LibGit2Sharp;

public static IEnumerable<string> CollectReleaseNotes(this ICakeContext context, BuildData data) {
  using var repo = data.Git.GetRepository();
  if (!data.Git.HasTag)
    throw new InvalidOperationException("Release notes can only be collected when current commit is tag");

  var currentTagVersion = data.Version.Tag;
  var prevTag = repo.Tags.Select(t => new { Tag = t, Version = context.ParseTagName(t.FriendlyName) })
                        .Where(vt => vt.Version < currentTagVersion)
                        .OrderBy(vt => vt.Version)
                        .LastOrDefault()?.Tag;

  var currentTagCommit = (Commit)data.Git.Tag.Target;
  Commit prevTagCommit = prevTag?.Target as Commit;

  return from c in repo.Commits
         where c.Author.When <= currentTagCommit.Author.When &&
               (prevTag is null || c.Author.When > prevTagCommit.Author.When)
         where !c.MessageShort.Contains("[skip notes]")
         select c.Message;
}

public static string GetGitAuthorName(this ICakeContext context) {
  return context.Environment.GetEnvironmentVariable("GIT_AUTHOR_NAME");
}

public static string GetGitAuthorEmail(this ICakeContext context) {
  return context.Environment.GetEnvironmentVariable("GIT_AUTHOR_EMAIL");
}

public static string GetGitHubAccessToken(this ICakeContext context) {
  return context.Environment.GetEnvironmentVariable("GITHUB_ACCESS_TOKEN");
}

public sealed class BuildGitInfo {
  private readonly BuildPaths _paths;

  public Commit Commit { get; set; }
  public Branch Branch { get; set; }
  public string BranchName { get; set; }
  public Tag Tag { get; set; }
  public string TagName { get; set; }
  public bool HasTag => Tag is not null;

  public BuildGitInfo(ICakeContext context, BuildPaths paths) {
    _paths = paths;

    using var repo = GetRepository();

    Commit = repo.Head.Tip;

    if(context.AppVeyor().IsRunningOnAppVeyor) {
      BranchName = context.AppVeyor().Environment.Repository.Branch;
      Branch = repo.Branches[BranchName];
    } else {
      Branch = repo.Head;
      BranchName = Branch.FriendlyName;
    }

    Tag = repo.Tags.FirstOrDefault(t => t.Target.Sha == Commit.Sha);
    if (Tag is not null) {
      TagName = Tag.FriendlyName;
    }
  }

  public Repository GetRepository() => new(_paths.Root.FullPath);
}
