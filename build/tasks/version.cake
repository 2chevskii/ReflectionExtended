#load ../data/build_data.cake

/*
  Common:
    if tag: write version.props from tag name
    else: write version.props from version.props + branch name (if not master)
  CI:
    append build number to version.props
*/

Task("version/common").Does<BuildData>(data => {
  var isMaster = data.Git.BranchName is "master";

  if(data.Git.HasTag) {

  } else {

  }
});

Task("version/ci").IsDependentOn("version/common")
                  .Does<BuildData>(data => {


                  });

// Task("version/save")
