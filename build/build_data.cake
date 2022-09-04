#addin nuget:?package=semver&version=2.0.4

using Semver;

using SemVersion = Semver.SemVersion;

public class BuildData {
  public Paths Paths;
  public VersionData Version;


  public BuildData(ISetupContext context) {
    Paths = new Paths(context.Environment.WorkingDirectory);
    context.Verbose("BuildPaths:\n" +
    "Root: {0}\n" +
    "Version.props: {1}\n" +
    "RELEASE_NOTES.md: {2}\n" +
    "Artifacts:\n" +
    "\tLibs: {3}\n" +
    "\tPackages: {4}\n" +
    "Projects:\n" +
    "\tMain:\n" +
    "\t\tRoot: {5}\n" +
    "\t\tFile: {6}\n" +
    "\tTests:\n" +
    "\t\tRoot: {7}\n" +
    "\t\tFile: {8}",
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

    context.XmlPoke(path, VersionXPath, strVersion);
  }
}
