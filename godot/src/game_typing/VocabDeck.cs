namespace ankitaiso.game_typing;

public class VocabDeck {
  public string Title  { get; set; } = "";
  public VocabEntry[] Entries { get; set; } = [];
  public string PromptKey { get; set; } = "";
  public string? TitleKey { get; set; }
  public string? TranslationKey { get; set; }
  public string? AudioKey { get; set; }
}
