namespace ankitaiso.data;

using Godot;

public record Scenario {
  public required string Id { get; set; }
  public required string Title { get; set; }
  public required string Source { get; set; }
  public required string WordList { get; set; }
  public required string Locale { get; set; }

  public string ReadWordList() {
    var contents = ResourceLoader.Load("src/data/" + WordList);
    if (contents == null) { return "error"; }
    var file = FileAccess.Open(contents.ResourcePath, FileAccess.ModeFlags.Read);
    var content = file.GetAsText();
    file.Close();
    return content;
  }
}
