using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.Build.Tasks;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

using parameters;

using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

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
    _.Executes( () => DotNetRestore( restore => restore.SetProjectFile( SrcProjectFilePath ) ),
                () =>
                DotNetRestore( restore =>
                               restore.SetProjectFile( TestProjectFilePath )
                )
    ).After( Clean, CleanOnRebuild );

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
    ).Before(BuildSrcProject, BuildTestProject);

    public Target CleanOnRebuild => _ => _.DependsOn( Clean )
                                          .OnlyWhenStatic( () => Rebuild )
                                          .Unlisted();

    public Target BuildSrcProject => _ => _.DependsOn( CleanOnRebuild , Restore )
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
                                                      .SetNoIncremental( Rebuild ).AddNoWarns(1591)
                                                      )
                                                      )
                                                      )
                                           )
                                           .Unlisted();

    public Target BuildTestProject => _ => _.DependsOn( CleanOnRebuild, Restore, BuildSrcProject )
                                            .OnlyWhenDynamic( () => Configuration.Contains( parameters.Configuration.Debug ) )
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
                                               compressionLevel: CompressionLevel.SmallestSize,
                                               fileMode: FileMode.Create
                                           )
                                           )
                                );

    /*public Target Build => _ => _.DependsOn( CleanOnRebuild )
                                 .Executes( () =>
                                     DotNetBuild( build => build.SetConfiguration( Configuration.ToString() ). )
                                 );*/

    /*public Target CleanSrcProject => _ => _.Before( BuildSrcProject )
                                           .Executes( () => Configuration.ForEach( c =>
                                                      DotNetClean( settings =>
                                                      settings.SetProject(
                                                                  SrcProjectFilePath
                                                              )
                                                              .SetConfiguration( c.ToString() )
                                                      )
                                                      )
                                           );
    public Target CleanTestProject => _ => _.Before( BuildTestProject )
                                            .Executes( () => Configuration.ForEach( c =>
                                                       DotNetClean( settings =>
                                                       settings.SetProject(
                                                                   TestProjectFilePath
                                                               )
                                                               .SetConfiguration( c.ToString() )
                                                       )
                                                       )
                                            );
    public Target CleanProjects => _ => _.DependsOn( CleanSrcProject, CleanTestProject );
    public Target CleanLibArtifacts => _ =>
    _.OnlyWhenDynamic( () => Configuration.Contains( parameters.Configuration.Release ) )
     .Executes( () => EnsureCleanDirectory( ArtifactsLibDirectory ) );
    public Target CleanPkgArtifacts => _ =>
    _.OnlyWhenDynamic( () => Configuration.Contains( parameters.Configuration.Release ) )
     .Executes( () => EnsureCleanDirectory( ArtifactsPkgDirectory ) );
    public Target CleanArtifacts => _ => _.DependsOn( CleanLibArtifacts, CleanPkgArtifacts );
    public Target Clean => _ => _.DependsOn( CleanProjects, CleanArtifacts );
    public Target RestoreSrcProject => _ =>
    _.Executes( () => DotNetRestore( restore => restore.SetProjectFile( SrcProjectFilePath ) ) );
    public Target RestoreTestProject => _ =>
    _.Executes( () => DotNetRestore( restore => restore.SetProjectFile( TestProjectFilePath ) ) );
    public Target Restore => _ => _.DependsOn( RestoreSrcProject, RestoreTestProject );
    public Target BuildSrcProject => _ => _.DependsOn( RestoreSrcProject )
                                           .Executes( () => Configuration.ForEach(
                                                          c => Framework.ForEach(
                                                              f => DotNetBuild( settings =>
                                                              settings
                                                              .SetProjectFile(
                                                                  SrcProjectFilePath
                                                              )
                                                              .SetConfiguration( c.ToString() )
                                                              .SetFramework(
                                                                  f.AsSrcProjectTarget()
                                                              )
                                                              .EnableNoRestore()
                                                              .EnableNoDependencies()
                                                              )
                                                          )
                                                      )
                                           );
    public Target BuildTestProject => _ => _.DependsOn( RestoreTestProject )
                                            .DependsOn( BuildSrcProject )
                                            .Executes( () => Configuration.ForEach( c =>
                                                       Framework.ForEach(
                                                           f => DotNetBuild( settings =>
                                                           settings
                                                           .SetProjectFile(
                                                               TestProjectFilePath
                                                           )
                                                           .SetConfiguration( c.ToString() )
                                                           .SetFramework(
                                                               f.AsTestProjectTarget()
                                                           )
                                                           .EnableNoRestore()
                                                           .EnableNoDependencies()
                                                           )
                                                       )
                                                       )
                                            );
    public Target BuildProjects => _ => _.DependsOn( BuildSrcProject, BuildTestProject );*/

    public static int Main() => Execute<BuildConfiguration>();
}
