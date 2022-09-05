#addin nuget:?package=Cake.FileHelpers&version=5.0.0

#load "../build_data.cake"

Task("release/notes").Does<BuildData>(data => {
  string formattedReleaseNotes;
  if(data.Git.Tag is not null) {
    // try parsing RELEASE_NOTES.md

    var releaseNotesPath = data.Paths.ReleaseNotes;

    var text = FileReadText(releaseNotesPath);
    var tagName = data.Git.TagName;
    // find start range to parse
    var tagStartIndex = text.IndexOf(TagName);
    // find this line
    int rangeStartIndex = tagStartIndex;
    while(rangeStartIndex >= 0) {
      char c = text[rangeStartIndex];
      if(c == '\n')
        break;

      rangeStartIndex--;
    }
    // find range end

  } else {
    // derive commits directly from repo
  }

  // if RELEASE_NOTES.md does not contain RN definition for current release
  var commits = data.Git.GetCommitsForReleaseNotes();


});
