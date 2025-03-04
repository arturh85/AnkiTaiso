namespace kyoukaitansa.data;

using Godot;

public record Scenario {
    public string Id { get; set; }
    public string Title { get; set; }
    public string Source { get; set; }
    public string WordList { get; set; }
    public string Locale { get; set; }

    public string ReadWordList() {
      var file = FileAccess.Open("res://src/data/" + WordList, FileAccess.ModeFlags.Read);
      var content = file.GetAsText();
      return content;
    }
}
