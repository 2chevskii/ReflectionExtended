#addin nuget:?package=semver&version=2.0.4

#load build_data.cake
#load build_paths.cake
#load build_git_info.cake

using Semver;

using SemVersion = Semver.SemVersion;







public sealed class BuildVersionInfo {
  public const string VersionXPath = "//Version";

  private readonly BuildData _data;

  public SemVersion Source { get; set; }
  public SemVersion Tag { get; set; }
  public bool HasTagVersion => Tag is not null;
  public SemVersion Target => HasTagVersion ? Tag : Source;

  public BuildVersionInfo(ICakeContext context, BuildData data) {
    _data = data;

    Source = ReadVersionProps(data.Paths);

    if(git.HasTag) {
      Tag = context.ParseTagName(data.Git.TagName);
    }
  }

  public static SemVersion ReadVersionProps(ICakeContext context, BuildPaths paths) {
    var path = paths.VersionProps;

    context.Verbose("Reading Version.props file ({0}) {1}", path, VersionXPath);

    if(!context.FileExists(path)) {
      throw new FileNotFoundException("Version.props file does not exist", path.FullPath);
    }

    var strVersion = context.XmlPeek(path, VersionXPath);
    if(string.IsNullOrEmpty(strVersion)) {
      throw new InvalidOperationException("Version.props file does not contain a valid version property");
    }

    var version = SemVersion.Parse(strVersion);
    context.Verbose("Version.props' version is {0}", version);

    return version;
  }

  public static void WriteVersionProps(ICakeContext context, BuildPaths paths, SemVersion version) {
    var path = paths.VersionProps;

    context.Verbose("Writing version {0} to Version.props file ({1}:{2})", version, path, VersionXPath);

    if(!context.FileExists(path)) {
      throw new FileNotFoundException("Version.props file does not exist", path.FullPath);
    }

    context.XmlPoke(path, VersionXPath, version.ToString());

    context.Verbose("Version written successfully to Version.props");
  }

  public static SemVersion CreateBranchVersion(ICakeContext context, SemVersion version, string branchName) {
    SemVersion targetVersion = version.Change();

    if(version.Prerelease.Length is 0) {
      targetVersion = version.Change(prerelease: branchName);
    }

    if(!version.Prerelease.Equals(branchName) && !version.Prerelease.EndsWith('-' + branchName)) {
      targetVersion =  version.Change(prerelease: version.Prerelease + '-' + branchName);
    }

    context.Verbose("CreateBranchVersion: {0} + {1} => {2}", version, branchName, targetVersion);

    return targetVersion;
  }

  public static SemVersion CreateBuildNumberVersion(SemVersion version, int buildNumber)
    => version.Change(build: buildNumber.ToString());

  public static SemVersion ParseTagName(string tagName) => SemVersion.Parse(tagName.Trim(' ', '\t').TrimStart('v', 'V'), true);


}
