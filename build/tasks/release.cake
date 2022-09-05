#load "../build_data.cake"

Task("release/notes").Does<BuildData>(data => {
  var commits = data.Git.GetCommitsForReleaseNotes();


});
