#load build_data.cake

public void BuildProject(DirectoryPath workingDirectory, FilePath projectPath, string configuration, string framework) {
  Information("Building project {0}\nConfiguration: {1}\nTarget framework: {2}",
    projectPath.GetFilenameWithoutExtension(),
    configuration,
    framework
  );

  DotNetBuild(projectPath.FullPath, new DotNetBuildSettings {
    WorkingDirectory = workingDirectory,
    NoRestore = true,
    NoDependencies = true,
    NoLogo = true,
    Configuration = configuration,
    Framework = framework
  });
}

public void RestoreProject(DirectoryPath workingDirectory, FilePath projectPath) {
    Information("Restoring project {0}...", projectPath.GetFilenameWithoutExtension());
    DotNetRestore(projectPath.FullPath, new DotNetRestoreSettings {
        WorkingDirectory = workingDirectory,
        NoDependencies = true
    });
}

public string FormatReleaseNotes(IEnumerable<string> releaseNotes) {
  var builder = new StringBuilder();
  foreach(var note in releaseNotes)
    builder.Append('-').Append(' ').AppendLine(note);

  return builder.ToString();
}
