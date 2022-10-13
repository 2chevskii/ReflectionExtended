using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

using LibGit2Sharp;

using Nuke.Common;
using Nuke.Common.ChangeLog;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MinVer;
using Nuke.Common.Tools.NerdbankGitVersioning;
using Nuke.Common.Utilities.Collections;

using Serilog;

using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

using Configuration = parameters.Configuration;

// ReSharper disable InconsistentNaming
// ReSharper disable ConditionalAnnotation
// ReSharper disable MissingSuppressionJustification
// ReSharper disable MemberCanBePrivate.Global

[SuppressMessage( "ReSharper", "MissingAnnotation" )]
public class BuildConfiguration : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    const string SrcProjectName = "ReflectionExtended";
    const string SrcProjectFileName  = SrcProjectName + ".csproj";
    const string TestProjectName     = "ReflectionExtended.Tests";
    const string TestProjectFileName = TestProjectName + ".csproj";

    readonly string[] SrcProjectFrameworks  = {"netstandard2.0", "netstandard2.1", "net6.0"};
    readonly string[] TestProjectFrameworks = {"netcoreapp2.1", "netcoreapp3.1", "net6.0"};

    [Parameter( List = true )]
    readonly Configuration[] Configuration = {parameters.Configuration.Debug};
    [Parameter] readonly bool Rebuild;

    [Solution( Name = "Solution" )] Solution ReflectionExtendedSln;

    /* Paths config */

    AbsolutePath SrcDirectory => RootDirectory / "src";
    AbsolutePath SrcProjectDirectory => SrcDirectory / SrcProjectName;
    AbsolutePath SrcProjectFilePath => SrcProjectDirectory / SrcProjectFileName;
    AbsolutePath TestDirectory => RootDirectory / "test";
    AbsolutePath TestProjectDirectory => TestDirectory / TestProjectName;
    AbsolutePath TestProjectFilePath => TestProjectDirectory / TestProjectFileName;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath ArtifactsLibDirectory => ArtifactsDirectory / "lib";
    AbsolutePath ArtifactsPkgDirectory => ArtifactsDirectory / "packages";

    public Target Restore => _ =>
    _.Executes( () => DotNetRestore( restore =>
                                     restore.SetProjectFile( SrcProjectFilePath )
                ),
                () =>
                DotNetRestore( restore =>
                               restore.SetProjectFile( TestProjectFilePath )
                )
     )
     .After( Clean, CleanOnRebuild );

    public Target Clean => _ => _.Executes( () => EnsureCleanDirectory( ArtifactsLibDirectory ),
                                            () => EnsureCleanDirectory( ArtifactsPkgDirectory ),
                                            () => EnsureCleanDirectory( SrcProjectDirectory / "bin"
                                            ),
                                            () => EnsureCleanDirectory( SrcProjectDirectory / "obj"
                                            ),
                                            () => EnsureCleanDirectory( TestProjectDirectory / "bin"
                                            ),
                                            () => EnsureCleanDirectory( TestProjectDirectory / "obj"
                                            )
                                 )
                                 .Before( BuildSrcProject, BuildTestProject );

    public Target CleanOnRebuild => _ => _.DependsOn( Clean )
                                          .OnlyWhenStatic( () => Rebuild )
                                          .Unlisted();

    public Target BuildSrcProject => _ => _.DependsOn( CleanOnRebuild, Restore )
                                           .Executes( () => Configuration.ForEach( c =>
                                                      SrcProjectFrameworks.ForEach( f =>
                                                      DotNetBuild( build => build
                                                      .SetProjectFile(
                                                          SrcProjectFilePath
                                                      )
                                                      .SetConfiguration( c.ToString() )
                                                      .SetFramework( f )
                                                      .EnableNoRestore()
                                                      .EnableNoDependencies()
                                                      .SetNoIncremental( Rebuild )
                                                      )
                                                      )
                                                      )
                                           )
                                           .Unlisted();

    public Target BuildTestProject => _ => _.DependsOn( CleanOnRebuild, Restore, BuildSrcProject )
                                            .OnlyWhenDynamic(
                                                () => Configuration.Contains(
                                                    parameters.Configuration.Debug
                                                )
                                            )
                                            .Executes( () => TestProjectFrameworks.ForEach( f =>
                                                       DotNetBuild( build => build
                                                       .SetProjectFile( TestProjectFilePath
                                                       )
                                                       .SetConfiguration( "Debug" )
                                                       .SetFramework( f )
                                                       .EnableNoRestore()
                                                       .EnableNoDependencies()
                                                       .SetNoIncremental( Rebuild )
                                                       )
                                                       )
                                            )
                                            .Unlisted();

    public Target Build => _ => _.DependsOn( BuildSrcProject, BuildTestProject );

    public Target Test => _ => _
                               .DependsOn( Build )
                               .Executes( () => TestProjectFrameworks.ForEach(
                                              f => DotNetTest( test => test
                                                               .SetProjectFile( TestProjectFilePath
                                                               )
                                                               .SetConfiguration( "Debug" )
                                                               .SetLoggers( "console" )
                                                               .SetVerbosity( DotNetVerbosity.Normal
                                                               )
                                                               .SetFramework( f )
                                                               .EnableNoRestore()
                                                               .EnableNoBuild()
                                              )
                                          )
                               );

    public Target Pack => _ => _.DependsOn( Clean, Build )
                                .After( Test )
                                .OnlyWhenDynamic(
                                    () => Configuration.Contains( parameters.Configuration.Release )
                                )
                                .Executes( () => DotNetPack( pack => pack
                                                             .SetConfiguration(
                                                                 parameters.Configuration.Release
                                                                 .ToString()
                                                             )
                                                             .EnableNoRestore()
                                                             .EnableNoDependencies()
                                                             .EnableNoBuild()
                                                             .SetVerbosity( DotNetVerbosity.Normal )
                                                             .SetProject( SrcProjectFilePath )
                                                             .EnableIncludeSymbols()
                                                             .SetOutputDirectory(
                                                                 ArtifactsPkgDirectory
                                                             )
                                           ),
                                           () => SrcProjectFrameworks.ForEach( f => CompressZip(
                                               SrcProjectDirectory /
                                               "bin" /
                                               "Release" /
                                               f, ArtifactsLibDirectory / f + ".zip",
                                               compressionLevel: CompressionLevel
                                               .SmallestSize,
                                               fileMode: FileMode.Create
                                           )
                                           )
                                );

    public Target CI => _ => _
                             .DependsOn( Build,Test,Pack )
                             .OnlyWhenStatic(() => !IsLocalBuild)
                             .Executes( () =>
                                 {
                                     IReadOnlyCollection<AbsolutePath> libArtifacts       = ArtifactsLibDirectory.GlobFiles("*.zip");
                                     IReadOnlyCollection<AbsolutePath> pkgArtifacts = ArtifactsPkgDirectory.GlobFiles("*.{nupkg,snupkg}");

                                     /*Log.Information( "Lib artifacts: {@Libs}" ,libArtifacts.Select( p => p.ToString() ));
                                     Log.Information( "Package artifacts: {@Pkgs}", pkgArtifacts
                                                      .Select( p => p.ToString() )
                                     );
                                     */

                                     libArtifacts.Concat(pkgArtifacts).ForEach(
                                         path => AppVeyor.Instance.PushArtifact( path, path.Name )
                                     );
                                 }
                             );

    /*public Target PrintVersion => _ => _.Executes( () =>
    {
        string releaseNotes = @"
## v0.1.0

- Some fix
- Some feature

## v0.2.0

- Some another cool feature

";

        Regex noteHeaderRegex = new ( @"## (?:[v]\s*([^\s]+))(?:\r\n|\n|\r)" );

        var headers = noteHeaderRegex.Matches( releaseNotes )
                                     .Select( m => m.Index )
                                     .OrderBy( i => i );

        
    } );*/

    public static int Main() => Execute<BuildConfiguration>();

    string[] GetReleaseNotes(Commit[] commits)
    {
        return commits.Where( commit => !commit.MessageShort.Contains( "[skip rn]",
                                                StringComparison.OrdinalIgnoreCase
                                        ) &&
                                        !commit.MessageShort.Contains( "[skip notes]",
                                                StringComparison.OrdinalIgnoreCase
                                        )
                      )
                      .Select( commit =>
                               $"{commit.MessageShort} // {commit.Author.Name} at {commit.Author.When.ToString( "g" )}"
                      )
                      .ToArray();
    }
    

}
