#addin nuget:?package=Cake.FileHelpers&version=5.0.0

#load "../build_data.cake"

Task("release/notes").Does<BuildData>(data => {
  string formattedReleaseNotes;
  if(data.Git.Tag is not null) {
    // try parsing RELEASE_NOTES.md

    var allReleaseNotes = ParseAllReleaseNotes(data.Paths.ReleaseNotes);
    // try find mathing release notes
    // var currentReleaseNotes = allReleaseNotes.FirstOrDefault(rn => rn.SemVersion.ToString() == data.Version.

    /* var releaseNotesPath = data.Paths.ReleaseNotes;

    var text = FileReadText(releaseNotesPath);
    var tagName = data.Git.TagName;
    // find start range to parse
    var tagStartIndex = text.IndexOf(tagName);
    // find this line
    int rangeStartIndex = tagStartIndex;
    while(rangeStartIndex >= 0) {
      char c = text[rangeStartIndex];
      if(c == '\n')
        break;

      rangeStartIndex--;
    }
    rangeStartIndex++;
    // find range end
    int rangeEndIndex = tagStartIndex;
    while(rangeEndIndex <= text.Length) {
      if(rangeEndIndex == text.Length)
        break;
      char c = text[rangeEndIndex];
      if(c == '#')
        break;
    }

    // get range lines
    var textLines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1); */


  } else {
    // derive commits directly from repo
  }

  // if RELEASE_NOTES.md does not contain RN definition for current release
  var commits = data.Git.GetCommitsForReleaseNotes();


});
