#load ../data/build_data.cake

Task("test/net6").IsDependentOn("build/test:net6")
                 .Does<BuildData>(data => {
  Information("Running project tests targeting {0}", "net6.0");

  DotNetTest(data.Paths.TestProjectFile.FullPath, new DotNetTestSettings {
    NoRestore = true,
    NoBuild = true,
    Configuration = "Net6"
  });
});
Task("test/netstandard2").IsDependentOn("build/test:netstandard2")
                         .Does<BuildData>(data => {
  Information("Running project tests targeting {0}", "netstandard2.0");

  DotNetTest(data.Paths.TestProjectFile.FullPath, new DotNetTestSettings {
    NoRestore = true,
    NoBuild = true,
    Configuration = "NetStandard2"
  });
});
Task("test").IsDependentOn("test/netstandard2")
            .IsDependentOn("test/net6");
