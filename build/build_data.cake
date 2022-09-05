#addin nuget:?package=semver&version=2.0.4
#addin nuget:?package=LibGit2Sharp&version=0.27.0-preview-0182&prerelease&loaddependencies=true

using System.Linq;
using Semver;
using LibGit2Sharp;
using SemVersion = Semver.SemVersion;

public class BuildData {
  public Paths Paths;
  public VersionData Version;
  public GitInfo Git;

  public BuildData(ISetupContext context) {
    Paths = new Paths(context.Environment.WorkingDirectory);
    context.Verbose("BuildPaths:\n" +
    "Root: {0}\n" +
    "Version.props: {1}\n" +
    "RELEASE_NOTES.md: {2}\n" +
    "Artifacts:\n" +
    "  Libs: {3}\n" +
    "  Packages: {4}\n" +
    "Projects:\n" +
    "  Main:\n" +
    "    Root: {5}\n" +
    "    File: {6}\n" +
    "  Tests:\n" +
    "    Root: {7}\n" +
    "    File: {8}",
    Paths.Root,
    Paths.VersionProps,
    Paths.ReleaseNotes,
    Paths.ArtifactsLib,
    Paths.ArtifactsPackages,
    Paths.MainProjectRoot,
    Paths.MainProjectFile,
    Paths.TestProjectRoot,
    Paths.TestProjectFile);

    Version = new VersionData(VersionData.ReadVersionProps(context, Paths.VersionProps));
    context.Verbose("Version.props read version: {0}", Version.VersionProps);

    Git = new GitInfo(context, this);
    context.Verbose("Build commit: {0}", Git.CommitHash);
    context.Information("Commit info: {0} - {1} ({2} at {3})",
      Git.CommitHash[..8],
      Git.CommitMessage,
      Git.Commit.Author.Name,
      Git.Commit.Author.When
    );
  }
}

public class Paths {
  public DirectoryPath Root;
  public FilePath VersionProps;
  public FilePath ReleaseNotes;
  public DirectoryPath ArtifactsLib;
  public DirectoryPath ArtifactsPackages;
  public DirectoryPath MainProjectRoot;
  public FilePath MainProjectFile;
  public DirectoryPath TestProjectRoot;
  public FilePath TestProjectFile;

  public Paths(DirectoryPath root) {
    Root = root;
    VersionProps = root.CombineWithFilePath("Version.props");
    ReleaseNotes = root.CombineWithFilePath("RELEASE_NOTES.md");
    ArtifactsLib = root.Combine("artifacts").Combine("lib");
    ArtifactsPackages = root.Combine("artifacts").Combine("packages");
    MainProjectRoot = root.Combine("src").Combine("ReflectionExtended");
    MainProjectFile = MainProjectRoot.CombineWithFilePath("ReflectionExtended.csproj");
    TestProjectRoot = root.Combine("test").Combine("ReflectionExtended.Tests");
    TestProjectFile = TestProjectRoot.CombineWithFilePath("ReflectionExtended.Tests.csproj");
  }
}

public class VersionData {
  public const string VersionPropsXPath = "//Version";

  public SemVersion VersionProps;

  public VersionData(SemVersion versionProps) {
    VersionProps = versionProps;
  }

  public static SemVersion ReadVersionProps(ICakeContext context, FilePath path) {
    var strVersion = context.XmlPeek(path, VersionPropsXPath);

    return SemVersion.Parse(strVersion);
  }

  public static void WriteVersionProps(ICakeContext context, FilePath path, SemVersion version) {
    var strVersion = version.ToString();

    context.XmlPoke(path, VersionPropsXPath, strVersion);
  }

  public static SemVersion GetTagVersion(GitInfo git) {
    if(git.Tag is null)
      throw new InvalidOperationException("Cannot get tag version: no tag");

    return ParseTagVersion(git.TagName);
  }

  public static SemVersion ParseTagVersion(Tag tag) {
    return ParseTagVersion(tag.FriendlyName);
  }

  public static SemVersion ParseTagVersion(string tagName) {
    return SemVersion.Parse(tagName.Trim(' ', '\t').TrimStart('v'));
  }

  public static SemVersion SetBranchSuffix(GitInfo git, SemVersion version) {
    if(git.Tag is not null || git.BranchName is "master" || version.Prerelease.EndsWith(git.BranchName)) {
      return version;
    } else if(version.Prerelease.Length is 0) {
      return version.Change(prerelease: git.BranchName);
    } else {
      return version.Change(prerelease: version.Prerelease + "-" + git.BranchName);
    }
  }
}

public class GitInfo {
  private readonly Repository _repo;
  public string CommitHash;
  public string CommitMessage;
  public Commit Commit;
  public string BranchName;
  public Branch Branch;
  public Tag Tag;
  public string TagName;

  public GitInfo(ISetupContext context, BuildData data) {
    _repo = new Repository(data.Paths.Root.FullPath);

    var head = _repo.Head;
    Commit = head.Tip;
    CommitHash = Commit.Sha;
    CommitMessage = Commit.MessageShort;

    Tag = _repo.Tags.FirstOrDefault(t => t.Target.Sha == CommitHash);
    if(Tag is not null) {
      TagName = Tag.FriendlyName;
    }
    else {
      if(context.AppVeyor().IsRunningOnAppVeyor) {
        BranchName = context.AppVeyor().Environment.Repository.Branch;
        Branch = _repo.Branches[BranchName]; // Because AppVeyor does checkout on commit, HEAD will not be at any branch
      } else {
        Branch = head;
        BranchName = head.FriendlyName;
      }
    }
  }

  public Repository GetRepository() => _repo;

  public IEnumerable<Commit> GetCommitsForReleaseNotes() {
    IEnumerable<Commit> result;

    if(!_repo.Tags.Any()) {
      result = from commit in Branch.Commits
               where commit.Author.When <= Commit.Author.When
               select commit;
    } else {
      var tagVersions = from tag in _repo.Tags
                        select new {Tag = tag, Version = VersionData.ParseTagVersion(tag)} into tv
                        orderby tv.Version descending
                        select tv;

      if(Tag is not null) {
        var currentTagVersion =  VersionData.GetTagVersion(this);
        var since = tagVersions.Where(tv => tv.Version < currentTagVersion).FirstOrDefault();
        if(since is not null) {
          result = from commit in _repo.Commits
                   where commit.Author.When <= Tag.Target.Peel<Commit>().Author.When &&
                   commit.Author.When > since.Tag.Target.Peel<Commit>().Author.When
                   select commit;
        } else {
          result = from commit in _repo.Commits
                   where commit.Author.When <= Tag.Target.Peel<Commit>().Author.When
                   select commit;
        }
      } else {
        var since = tagVersions.First();
        result = from commit in Branch.Commits
                 where commit.Author.When <= Commit.Author.When && commit.Author.When > since.Tag.Target.Peel<Commit>().Author.When
                 select commit;
      }
    }

    return from commit in result
           where !commit.MessageShort.Contains("[skip notes]")
           orderby commit.Author.When descending
           select commit;

    /* var repo = GetRepository();

    IEnumerable<Commit> result;

    if(repo.Tags.Count() < 2) {
      result = from commit in Branch.Commits
               where commit.Author.When <= Commit.Author.When
               select commit;
    } else {
      var tagVersions = from t in repo.Tags
                        select new {Tag = t, Version = SemVersion.Parse(t.FriendlyName.Trim(' ', '\t').TrimStart('v'))} into tv
                        orderby tv.Version descending
                        select tv;

      if(Tag is not null) {
        var tagVersion = SemVersion.Parse(TagName.Trim(' ', '\t').TrimStart('v'));
        var prev = tagVersions.Where(tv => tv.Version < tagVersion).First();
        var prevCommit = (Commit)prev.Tag.Target;

        result = from commit in repo.Commits
                 where commit.Author.When <= Commit.Author.When && commit.Author.When > prevCommit.Author.When
                 select commit;
      } else {
        result = from commit in Branch.Commits
                 where commit.Author.When > (tagVersions.First().Tag.Target as Commit).Author.When
                 select commit;
      }
    }

    return from commit in result
             where !commit.MessageShort.Contains("[skip notes]")
             orderby commit.Author.When descending
             select commit;
    */
  }
}
