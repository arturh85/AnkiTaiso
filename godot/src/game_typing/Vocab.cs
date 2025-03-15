namespace ankitaiso.game_typing;

using System.Collections.Generic;
using System.Linq;
using utils;
using WanaKanaNet;

public class Vocab {
  public readonly VocabEntry Entry;
  public string Next;
  public string InputBuffer;
  public List<string>? NextVariants;
  private static readonly HangeulRomaniser _hangeulRomaniser = new HangeulRomaniser();

  public VocabState State;

  public Vocab(VocabEntry entry) {
    Entry = entry;
    Next = "";
    SetNext(0);
    State = VocabState.Hidden;
    InputBuffer = "";
  }

  private void SetNext(int idx) {
    var oldNext = Next;
    var entry = Entry.Prompt;
    var next = entry[idx];
    Next = entry.Substring(idx, 1);
    var nextIsHiragana = WanaKana.IsHiragana(next);
    var nextIsKatakana = !nextIsHiragana && WanaKana.IsKatakana(next);
    if (nextIsHiragana || nextIsKatakana) {
      if (GameTypingUtils.IsSmallTsu(next)) {
        Next = entry.Substring(idx, 2);
        NextVariants = [WanaKana.ToRomaji(Next).Trim()];
      }
      else if (entry.Length > idx + 1 && GameTypingUtils.IsSmallKana(entry[idx + 1])) {
        Next = entry.Substring(idx, 2);
        NextVariants = [WanaKana.ToRomaji(entry.Substring(idx, 2)).Trim()];
      }
      else {
        NextVariants = [WanaKana.ToRomaji(next.ToString()).Trim()];
      }

      if (next == 'ー') {
        NextVariants.Add(WanaKana.ToRomaji(oldNext).Last().ToString());
      }

      GameTypingUtils.PopulateAlternatives(Next, NextVariants);
    }
    else if (WanaKana.IsRomaji(next)) {
      NextVariants = null;
    }
    else {
      var roman = _hangeulRomaniser.Romanise(next);
      if (roman != null) {
        NextVariants = [roman];
      }
      else {
        NextVariants = null;
      }
    }
  }

  public bool OnInput(string input) {
    if (Entry.Prompt.StartsWith(InputBuffer + input)) {
      InputBuffer += input;
      if (InputBuffer.Length < Entry.Prompt.Length) {
        SetNext(InputBuffer.Length);
      }

      return true;
    }
    return false;
  }
}
