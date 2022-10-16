using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;

using Nuke.Common;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

using Serilog;

using Nuke.Common.Utilities;

using System.Text.RegularExpressions;

using LibGit2Sharp;
using Semver;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.ProjectModel.ProjectModelTasks;

[SuppressMessage( "ReSharper", "MissingAnnotation" )]
public class BuildConfiguration : NukeBuild
{
    private const string SrcProjectName      = "ReflectionExtended";
    private const string SrcProjectFileName  = SrcProjectName + ".csproj";
    private const string TestProjectName     = "ReflectionExtended.Tests";
    private const string TestProjectFileName = TestProjectName + ".csproj";

    /*
     * Normal build version: $(Version)-$(IsMaster ? Empty : BranchName)+$(BuildNumber.ToHexString())
     * Tag build version: $(TagVersion)
     */

    [Solution(Name = "ReflectionExtended.sln")]
    readonly Solution Solution;

    

    /* Paths config */

    private static AbsolutePath SrcDirectory => RootDirectory / "src";
    private static AbsolutePath SrcProjectDirectory => SrcDirectory / SrcProjectName;
    private static AbsolutePath SrcProjectFilePath => SrcProjectDirectory / SrcProjectFileName;
    private static AbsolutePath TestDirectory => RootDirectory / "test";
    private static AbsolutePath TestProjectDirectory => TestDirectory / TestProjectName;
    private static AbsolutePath TestProjectFilePath => TestProjectDirectory / TestProjectFileName;

    private static AbsolutePath TestResultsDirectory => RootDirectory / "test_results";

    private static AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    private static AbsolutePath ArtifactsLibDirectory => ArtifactsDirectory / "lib";
    private static AbsolutePath ArtifactsPkgDirectory => ArtifactsDirectory / "packages";

    public Target Restore => _ => _.Executes(
        () => DotNetRestore( restore => restore.SetProjectFile( SrcProjectFilePath ) ),
        () => DotNetRestore( restore => restore.SetProjectFile( TestProjectFilePath ) )
    );

    public Target Clean => _ => _.Executes(
        () => EnsureCleanDirectory( ArtifactsLibDirectory ),
        () => EnsureCleanDirectory( ArtifactsPkgDirectory ),
        () => EnsureCleanDirectory( TestResultsDirectory ),
        () => DotNetClean( settings => settings.SetProject( SrcProjectFilePath ).SetConfiguration("Debug") ),
        () => DotNetClean( settings => settings.SetProject( SrcProjectFilePath ).SetConfiguration("Release") ),
        () => DotNetClean( settings => settings.SetProject( TestProjectFilePath ).SetConfiguration("Debug") ),
        () => DotNetClean( settings => settings.SetProject( TestProjectFilePath ).SetConfiguration("Release") )
    );

    public Target InitVersion => _ => _.OnlyWhenStatic( () => !IsLocalBuild )
                                       .Executes(
                                           () => {
                                               var project = ParseProject( SrcProjectFilePath );
                                               var version =
                                               project.GetPropertyValue( "Version" );
                                               var semver = SemVersion.Parse(
                                                   version,
                                                   SemVersionStyles.Strict
                                               );
                                               int buildNumber = AppVeyor.Instance.BuildNumber;

                                               semver = semver.WithMetadata(
                                                   "build", buildNumber.ToString("X")
                                               );

                                               if (AppVeyor.Instance.RepositoryTag)
                                               {
                                                   semver = SemVersion.Parse(AppVeyor.Instance.RepositoryTagName, SemVersionStyles.AllowV);
                                                   throw new NotImplementedException();
                                               }
                                               else
                                               {
                                                   if (AppVeyor.Instance.RepositoryBranch is not
                                                       "master")
                                                   {
                                                       semver = semver.WithPrerelease(
                                                           AppVeyor.Instance.RepositoryBranch
                                                       );
                                                   }
                                               }

                                               project.SetProperty( "Version", semver.ToString() );

                                               AppVeyor.Instance.UpdateBuildVersion( semver.ToString() );
                                           }
                                       );

    public Target BuildSrcProject => _ => _.DependsOn( Restore )
                                           .Executes(
                                               () => DotNetBuild(
                                                   settings =>
                                                   settings.SetProjectFile( SrcProjectFilePath )
                                                           .SetConfiguration( "Release" )
                                                           .EnableNoRestore()
                                               )
                                           );

    public Target BuildTestProject => _ => _.DependsOn( Restore )
                                            .Executes(
                                                () => DotNetBuild(
                                                    settings =>
                                                    settings.SetProjectFile( TestProjectFilePath )
                                                            .SetConfiguration( "Debug" )
                                                            .EnableNoRestore()
                                                )
                                            );

    public Target Build => _ => _.Triggers( BuildSrcProject, BuildTestProject );

    readonly Action[] TestActions = {
        () => DotNetTest(
            settings => settings.SetProjectFile( TestProjectFilePath )
                                .SetConfiguration( "Debug" )
                                .SetFramework("netcoreapp2.1")
                                .SetResultsDirectory( TestResultsDirectory )
                                .SetLoggers(
                                    "trx;LogFileName=results_netcoreapp2.1.xml",
                                    "console;verbosity=normal"
                                )
                                .SetVerbosity( DotNetVerbosity.Normal )
                                .EnableNoBuild()
                                .EnableNoLogo()
        ),
        () => DotNetTest(
            settings => settings.SetProjectFile( TestProjectFilePath )
                                .SetConfiguration( "Debug" )
                                .SetFramework( "netcoreapp3.1" )
                                .SetResultsDirectory( RootDirectory / "test_results" )
                                .SetLoggers(
                                    "trx;LogFileName=results_netcoreapp3.1.xml",
                                    "console;verbosity=normal"
                                )
                                .SetVerbosity( DotNetVerbosity.Normal )
                                .EnableNoBuild()
                                .EnableNoLogo()
        ),
        () => DotNetTest(
            settings => settings.SetProjectFile( TestProjectFilePath )
                                .SetConfiguration( "Debug" )
                                .SetFramework( "net6.0" )
                                .SetResultsDirectory( TestResultsDirectory )
                                .SetLoggers(
                                    "trx;LogFileName=results_net6.0.xml",
                                    "console;verbosity=normal"
                                )
                                .SetVerbosity( DotNetVerbosity.Normal )
                                .EnableNoBuild()
                                .EnableNoLogo()
        )
    };
    public Target Test => _ => _.DependsOn( BuildTestProject ).Executes( TestActions );
    public Target TestCi => _ => _.OnlyWhenStatic( () => !IsLocalBuild ).Executes( TestActions );

    public Target UploadTestResults => _ => _.OnlyWhenStatic( () => !IsLocalBuild )
                                             .Executes(
                                                 () => {
                                                     string jobId = AppVeyor.Instance.JobId;

                                                     string uploadUrl =
                                                     $"https://ci.appveyor.com/api/testresults/mstest/{jobId}";

                                                     using (HttpClient httpClient = new ())
                                                     {
                                                         foreach (AbsolutePath path in
                                                                  (RootDirectory / "test_results")
                                                                  .GlobFiles(
                                                                      "results_*.xml"
                                                                  ))
                                                         {
                                                             Info(
                                                                 "Uploading test result: {ResultPath}",
                                                                 path
                                                             );
                                                             MultipartFormDataContent content =
                                                             new ();

                                                             using (FileStream fs = File.OpenRead(
                                                                 path
                                                             ))
                                                             {
                                                                 content.Add(
                                                                     new StreamContent( fs )
                                                                 );

                                                                 httpClient.PostAsync(
                                                                     new Uri( uploadUrl ),
                                                                     content
                                                                 );
                                                             }
                                                         }
                                                     }
                                                 }
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
            AbsolutePath releaseNotesPath = RootDirectory / "RELEASE_NOTES.md";

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

    private static void Info(string template, params object[] args)
    {
        if (IsLocalBuild)
            Log.Information( template, args );
        else
            AppVeyor.Instance.WriteInformation( string.Format( template, args ) );
    }

    private static void Warn(string template, params object[] args)
    {
        if (IsLocalBuild)
            Log.Warning( template, args );
        else
            AppVeyor.Instance.WriteWarning( string.Format( template, args ) );
    }

    private static void Error(string template, params object[] args)
    {
        if (IsLocalBuild)
            Log.Error( template, args );
        else
            AppVeyor.Instance.WriteError( string.Format( template, args ) );
    }
}
