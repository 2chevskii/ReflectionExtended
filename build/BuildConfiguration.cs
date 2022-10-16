using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using LibGit2Sharp;

using Nuke.Common;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MinVer;
using Nuke.Common.Tools.NerdbankGitVersioning;
using Nuke.Common.Utilities.Collections;

using Serilog;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.CompressionTasks;

using Configuration = parameters.Configuration;

using GlobExpressions;

using Nuke.Common.Utilities;

using System.Text.RegularExpressions;

using Nuke.Common.CI.AppVeyor.Configuration;

#pragma warning disable CA1050
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

// ReSharper disable InconsistentNaming
// ReSharper disable ConditionalAnnotation
// ReSharper disable MissingSuppressionJustification
// ReSharper disable MemberCanBePrivate.Global

[SuppressMessage( "ReSharper", "MissingAnnotation" )]
public class BuildConfiguration : NukeBuild
{
    const string SrcProjectName      = "ReflectionExtended";
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

    public Target Restore => _ => _.Executes(
                                       () => DotNetRestore(
                                           restore => restore.SetProjectFile( SrcProjectFilePath )
                                       ),
                                       () => DotNetRestore(
                                           restore => restore.SetProjectFile( TestProjectFilePath )
                                       )
                                   )
                                   .After( Clean, CleanOnRebuild );

    public Target Clean => _ => _.Executes(
                                     () => EnsureCleanDirectory( ArtifactsLibDirectory ),
                                     () => EnsureCleanDirectory( ArtifactsPkgDirectory ),
                                     () => EnsureCleanDirectory( SrcProjectDirectory / "bin" ),
                                     () => EnsureCleanDirectory( SrcProjectDirectory / "obj" ),
                                     () => EnsureCleanDirectory( TestProjectDirectory / "bin" ),
                                     () => EnsureCleanDirectory( TestProjectDirectory / "obj" )
                                 )
                                 .Before( BuildSrcProject, BuildTestProject )
                                 .Triggers( Restore );

    public Target CleanOnRebuild => _ => _.DependsOn( Clean )
                                          .OnlyWhenStatic( () => Rebuild )
                                          .Unlisted();

    public Target BuildSrcProject => _ => _.DependsOn( Restore )
                                           .Executes(
                                               () => Configuration.ForEach(
                                                   c => DotNetBuild(
                                                       settings => {
                                                           Info(
                                                               "Building ReflectionExtended in '{Configuration}' configuration",
                                                               c.ToString()
                                                           );
                                                           return settings
                                                           .SetProjectFile( SrcProjectFilePath )
                                                           .SetConfiguration( c.ToString() )
                                                           .EnableNoRestore();
                                                       }
                                                   )
                                               )
                                           )
                                           .Unlisted();

    public Target BuildTestProject => _ => _.DependsOn( Restore )
                                            .Executes(
                                                () => DotNetBuild(
                                                    settings => {
                                                        Info(
                                                            "Building ReflectionExtended.Tests (Debug configuration)"
                                                        );
                                                        return settings
                                                               .SetProjectFile(
                                                                   TestProjectFilePath
                                                               )
                                                               .SetConfiguration( "Debug" )
                                                               .EnableNoRestore();
                                                    }
                                                )
                                            )
                                            .Unlisted();

    public Target Build => _ => _.DependsOn( BuildSrcProject, BuildTestProject );

    public Target Test => _ => _.DependsOn( BuildTestProject )
                                .Executes(
                                    () => DotNetTest(
                                        settings => settings.SetProjectFile( TestProjectFilePath )
                                                            .SetConfiguration( "Debug" )
                                                            .SetLoggers( "console" )
                                                            .SetVerbosity( DotNetVerbosity.Normal )
                                                            .EnableNoBuild()
                                    )
                                );

    public Target Pack => _ => _.DependsOn( Clean, BuildSrcProject )
                                .Executes( /* Create a nuget package => artifacts/packages/ReflectionExtended(Version).(s)nupkg */
                                    () => DotNetPack(
                                        settings => settings.SetProject( SrcProjectFilePath )
                                                            .SetConfiguration( "Release" )
                                                            .EnableNoBuild()
                                                            .SetOutputDirectory(
                                                                ArtifactsPkgDirectory
                                                            )
                                    ),
                                    /* Create zips for each target framework */
                                    () => CompressZip(
                                        SrcProjectDirectory / "bin" / "Release" / "netstandard2.0",
                                        ArtifactsLibDirectory / "netstandard2.0.zip"
                                    ),
                                    () => CompressZip(
                                        SrcProjectDirectory / "bin" / "Release" / "netstandard2.1",
                                        ArtifactsLibDirectory / "netstandard2.1.zip"
                                    ),
                                    () => CompressZip(
                                        SrcProjectDirectory / "bin" / "Release" / "net6.0",
                                        ArtifactsLibDirectory / "net6.0.zip"
                                    )
                                );

    public Target PushArtifacts => _ => _.OnlyWhenStatic( () => !IsLocalBuild )
                                         .Executes(
                                             () => ArtifactsPkgDirectory
                                                   .GlobFiles( "*.{snupkg,nupkg}" )
                                                   .ForEach(
                                                       path => {
                                                           Info(
                                                               "Pushing AppVeyor NuGet artifact: {Name}",
                                                               path.Name
                                                           );
                                                           AppVeyor.Instance.PushArtifact(
                                                               path,
                                                               path.Name
                                                           );
                                                       }
                                                   ),
                                             () => ArtifactsLibDirectory.GlobFiles( "*.zip" )
                                             .ForEach(
                                                 path => {
                                                     Info(
                                                         "Pushing AppVeyor Lib artifact: {Name}",
                                                         path.Name
                                                     );
                                                     AppVeyor.Instance.PushArtifact(
                                                         path,
                                                         path.Name
                                                     );
                                                 }
                                             )
                                         );

    public Target PrintReleaseNotes => _ => _.Executes(
        () => {
            const string HeaderRegex      = @"(##\s+ReflectionExtended\s*[vV](\d+\.\d+(?:\.\d+)?))";
            const string Version          = "1.0.0";
            AbsolutePath          releaseNotesPath = RootDirectory / "RELEASE_NOTES.md";

            string fileText = System.IO.File.ReadAllText( releaseNotesPath );

            MatchCollection headerMatches = Regex.Matches( fileText, HeaderRegex );

            Info( "Matches: {M}", headerMatches.Select( m => m.Value ).JoinCommaSpace() );

            Match versionMatch = headerMatches.SingleOrDefault(
                m => m.Groups[2].Value.EqualsOrdinalIgnoreCase( Version )
            );

            if (versionMatch is null)
            {
                Error( "Not found header for version {Version}", Version );
                return;
            }

            int spanStart = fileText.IndexOf( '\n', versionMatch.Index );

            Match nextHeader = headerMatches.OrderBy( m => m.Index )
                                            .FirstOrDefault( m => m.Index > versionMatch.Index );

            int spanEnd = fileText.Length;

            if (nextHeader is not null)
            {
                Info(
                    "Next header is not null: {Index} || {Header}",
                    nextHeader.Index,
                    nextHeader.Value
                );
                spanEnd = nextHeader.Index;
            }
            else { Info( "Next header is null" ); }

            Info( "SpanEnd is {Index}", spanEnd );

            string notes = fileText[(spanStart + 1)..spanEnd].Trim();

            Info( "ReleaseNotes: {Notes}", notes );
        }
    );

    public static int Main() => Execute<BuildConfiguration>();

    static void Info(string template, params object[] args)
    {
        if (IsLocalBuild) { Log.Information( template, args ); }
        else { AppVeyor.Instance.WriteInformation( string.Format( template, args ) ); }
    }

    static void Warn(string template, params object[] args)
    {
        if (IsLocalBuild) { Log.Warning( template, args ); }
        else { AppVeyor.Instance.WriteWarning( string.Format( template, args ) ); }
    }

    static void Error(string template, params object[] args)
    {
        if (IsLocalBuild) { Log.Error( template, args ); }
        else { AppVeyor.Instance.WriteError( string.Format( template, args ) ); }
    }
}
