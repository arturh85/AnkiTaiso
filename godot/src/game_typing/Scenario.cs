namespace ankitaiso.data;

using game_typing;
using Godot;
using utils;

public record Scenario {
  public required string Id { get; set; }
  public required string Title { get; set; }
  public required string Source { get; set; }
  public required string WordList { get; set; }
  public required string Locale { get; set; }

  public VocabConfig? Config { get; set; }

  public string ReadWordList() {
    using var file = FileAccess.Open(WordList, FileAccess.ModeFlags.Read);
    if (file == null) {
      throw new GameException($"failed to find bundled {WordList}");
    }
    var content = file.GetAsText();
    file.Close();
    return content;
  }
}
