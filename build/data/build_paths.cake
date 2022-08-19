using Cake.Core.IO;

public sealed class BuildPaths {
  public DirectoryPath Root { get; }
  public FilePath VersionProps { get; }
  public FilePath DirectoryBuildProps { get; }
  public FilePath DirectoryBuildTargets { get; }
  public DirectoryPath ArtifactsRoot { get; }
  public DirectoryPath ArtifactsLib { get; }
  public DirectoryPath ArtifactsPackages { get; }
  public DirectoryPath SrcRoot { get; }
  public DirectoryPath MainProjectRoot { get; }
  public FilePath MainProjectFile { get; }
  public DirectoryPath TestRoot { get; }
  public DirectoryPath TestProjectRoot { get; }
  public FilePath TestProjectFile { get; }
  public FilePath ReleaseNotes { get; }

  public BuildPaths(ICakeContext context) {
    context.Verbose("Initializing build paths");
    var cwd = context.Environment.WorkingDirectory;
    Root = cwd;
    VersionProps = Root.CombineWithFilePath("Version.props");
    DirectoryBuildProps = Root.CombineWithFilePath("Directory.Build.props");
    DirectoryBuildTargets = Root.CombineWithFilePath("Directory.Build.targets");
    ArtifactsRoot = Root.Combine("artifacts");
    ArtifactsLib = ArtifactsRoot.Combine("lib");
    ArtifactsPackages = ArtifactsRoot.Combine("packages");
    SrcRoot = Root.Combine("src");
    MainProjectRoot = SrcRoot.Combine("ReflectionExtended");
    MainProjectFile = MainProjectRoot.CombineWithFilePath("ReflectionExtended.csproj");
    TestRoot = Root.Combine("test");
    TestProjectRoot = TestRoot.Combine("ReflectionExtended.Tests");
    TestProjectFile = TestProjectRoot.CombineWithFilePath("ReflectionExtended.Tests.csproj");
    ReleaseNotes = Root.CombineWithFilePath("RELEASE_NOTES.md");
  }

  public FilePath GetPathForLibArtifact(string name) {
    return ArtifactsLib.CombineWithFilePath(FilePath.FromString(name + ".zip"));
  }

  public FilePath GetPathForPackageArtifact(string name, string extension = "nupkg") {
    return ArtifactsPackages.CombineWithFilePath(FilePath.FromString(name + '.' + extension));
  }

  public override string ToString()
  {
    return $@"
Root: {Root}
Version.props: {VersionProps}
Directory.Build.props: {DirectoryBuildProps}
Directory.Build.targets: {DirectoryBuildTargets}
Artifacts:
  Root: {ArtifactsRoot}
  Lib: {ArtifactsLib}
  Packages: {ArtifactsPackages}
Sources root: {SrcRoot}
Tests root: {TestRoot}
Projects:
  Main:
    Root: {MainProjectRoot}
    Project file: {MainProjectFile}
  Tests:
    Root: {TestProjectRoot}
    Project file: {TestProjectFile}
";
  }
}
