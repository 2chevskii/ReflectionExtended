#load build_git_info.cake
#load build_paths.cake
#load build_test_info.cake
#load build_version_info.cake
#load build_environment.cake


public sealed class BuildData {
  public bool IsLocal { get; set; }
  public BuildPaths Paths { get; set; }
  public BuildGitInfo Git { get; set; }
  public BuildVersionInfo Version { get; set; }
  public BuildEnvironment Environment { get; set; }

  public BuildData(ICakeContext context) {
    IsLocal = context.Environment.GetEnvironmentVariable("CI")?.ToLowerInvariant() is not "true" or "1";
    Paths = new BuildPaths(context);
    Git = new BuildGitInfo(context, Paths);
    Version = new BuildVersionInfo(context, Paths, Git);
  }

}
