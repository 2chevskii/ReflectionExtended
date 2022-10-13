using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;

using LibGit2Sharp;

using NuGet.Versioning;

using Nuke.Common;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

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
    [Parameter] readonly bool Rebuild = false;

    [Solution(Name = "Solution")] Solution ReflectionExtendedSln;

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
                             _.Executes(
                                  () => DotNetRestore( restore =>
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
                                                      .SetConfiguration(
                                                          c.ToString()
                                                      )
                                                      .SetFramework( f )
                                                      .EnableNoRestore()
                                                      .EnableNoDependencies()
                                                      .SetNoIncremental(
                                                          Rebuild
                                                      )
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
                                                       .SetProjectFile(
                                                           TestProjectFilePath
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

    public Target CI => _ => _.Executes( () =>
                                         {

                                             // Read version from project file
                                             var project =
                                             ReflectionExtendedSln.GetProject( "ReflectionExtended"
                                             );

                                             var strProjectVersion =
                                             project.GetProperty( "Version" );

                                             Console.WriteLine(
                                                 "Current project version is {0}", strProjectVersion
                                             );

                                             SemanticVersion projectVersion = SemanticVersion.Parse( strProjectVersion );

                                             Console.WriteLine("Current project parsed version is {0}", projectVersion);

                                             var appveyorBranch =
                                             AppVeyor.Instance.RepositoryBranch;

                                             if ( appveyorBranch is not "master" )
                                             {
                                                 projectVersion.Release.Append(
                                                     appveyorBranch.ToLowerInvariant()
                                                 );
                                             }

                                             var appveyorBuildNumber =
                                             AppVeyor.Instance.BuildNumber;

                                         }
                        );

    public static int Main() => Execute<BuildConfiguration>();
}
