#addin nuget:?package=LibGit2Sharp&version=0.27.0-preview-0182&prerelease&loaddependencies=true

#load build/data/build_data.cake;
#load build/local.cake
#load build/ci.cake

using LibGit2Sharp;

Setup(context => new BuildData(context));

Teardown<BuildData>((context, data) => {
  using var repo = data.Git.GetRepository();

  Information("Checkout on paths: {0} => {1}", data.Paths.VersionProps.GetFilename().ToString(), repo.Head.Tip.Sha);

  repo.CheckoutPaths(repo.Head.Tip.Sha, new[] {data.Paths.VersionProps.GetFilename().ToString()}, new CheckoutOptions {
    CheckoutModifiers = CheckoutModifiers.Force
  });
});

var target = Argument("target", "local");

RunTarget(target);
