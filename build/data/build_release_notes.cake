public sealed class BuildReleaseNotes {
  public bool IsCollected { get; set; }
  public string FirstCommit { get; set; }
  public string LastCommit { get; set; }
  public bool IsRelease { get; set; }
  public string ReleaseName { get; set; }
  public string PrevReleaseName { get; set; }
}
