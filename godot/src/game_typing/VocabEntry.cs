namespace ankitaiso.game_typing;

public class VocabEntry {
  public string Prompt { get; set; } = "";
  public string? Translation { get; set; }
  public string? Title { get; set; }
  public string? AudioFilename { get; set; }

  public VocabEntry() {
  }
  public VocabEntry(string prompt) {
    Prompt = prompt.Trim();
  }
}
