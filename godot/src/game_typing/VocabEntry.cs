namespace ankitaiso.game_typing;

public class VocabEntry {
  public string Prompt { get; set; } = "";
  public string? Translation { get; set; }
  public string? Title { get; set; }
  public string? AudioFilename { get; set; }

  public VocabEntry() {
  }
  public VocabEntry(string input) {
    var parts = input.Split("|");
    Prompt = parts[0].Trim();
    if (parts.Length > 1) {
      Title = parts[1].Trim();
    }
    if (parts.Length > 2) {
      Translation = parts[2].Trim();
    }
    if (parts.Length > 3) {
      AudioFilename = parts[3].Trim();
    }
  }
}
