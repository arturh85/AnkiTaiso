namespace ankitaiso.data;

using Godot;

public record Scenario {
  public required string Id { get; set; }
  public required string Title { get; set; }
  public required string Source { get; set; }
  public required string WordList { get; set; }
  public required string Locale { get; set; }

  public string ReadWordList() {
    var file = FileAccess.Open("res://src/data/" + WordList, FileAccess.ModeFlags.Read);
    var content = file.GetAsText();
    file.Close();
    return content;
  }
}
