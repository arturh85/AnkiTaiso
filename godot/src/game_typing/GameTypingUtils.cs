namespace ankitaiso.game_typing;

using System.Collections.Generic;

public static class GameTypingUtils {
  private static readonly HashSet<int> SmallKanaCodePoints = new HashSet<int> {
    // Small Hiragana
    0x3041, // ぁ
    0x3043, // ぃ
    0x3045, // ぅ
    0x3047, // ぇ
    0x3049, // ぉ
    0x3083, // ゃ
    0x3085, // ゅ
    0x3087, // ょ

    // Small Katakana
    0x30A1, // ァ
    0x30A3, // ィ
    0x30A5, // ゥ
    0x30A7, // ェ
    0x30A9, // ォ
    0x30E3, // ャ
    0x30E5, // ュ
    0x30E7 // ョ
  };

  private static readonly HashSet<int> SmallTsuCodePoints = new HashSet<int> {
    // Small Hiragana Tsu
    0x3063, // っ

    // Small Katakana Tsu
    0x30C3, // ッ
  };

  private static readonly Dictionary<string, string[]> KanaAlternatives = new() {
    { "し", ["si"] }, // default shi
    { "シ", ["si"] }, // default shi

    { "ち", ["ti"] }, // default chi
    { "チ", ["ti"] }, // default chi

    { "つ", ["tu"] }, // default tsu
    { "ツ", ["tu"] }, // default tsu

    { "ふ", ["hu"] }, // default fu
    { "フ", ["hu"] }, // default fu

    { "じ", ["zi"] }, // default ji
    { "ジ", ["zi"] }, // default ji

    { "ぢ", ["di"] }, // default ji
    { "ヂ", ["di"] }, // default ji

    { "づ", ["du"] }, // default zu
    { "ヅ", ["du"] }, // default zu

    { "を", ["o"] }, // default wo
    { "ヲ", ["o"] }, // default wo
  };

  public static bool IsSmallKana(char c) => SmallKanaCodePoints.Contains(c);
  public static bool IsSmallTsu(char c) => SmallTsuCodePoints.Contains(c);

  public static void PopulateKanaAlternatives(string input, List<string> entries) {
    if (!KanaAlternatives.TryGetValue(input, out var alternatives)) {
      return;
    }
    entries.AddRange(alternatives);
  }
}
