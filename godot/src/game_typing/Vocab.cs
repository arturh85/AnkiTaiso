namespace ankitaiso.game_typing;

using System.Collections.Generic;
using System.Linq;
using WanaKanaNet;

public class Vocab {
  public readonly string Entry;
  public string Next;
  public string InputBuffer;
  public List<string>? NextVariants;

  public VocabState State;

  public event OnMistakeEvent? OnMistake;
  public delegate void OnMistakeEvent(string key);

  public Vocab(string entry) {
    Entry = entry.Trim();
    Next = "";
    SetNext(0);
    State = VocabState.Hidden;
    InputBuffer = "";
  }

  private void SetNext(int idx) {
    var oldNext = Next;
    var next = Entry[idx];
    Next = Entry.Substring(idx, 1);
    var nextIsHiragana = WanaKana.IsHiragana(next);
    var nextIsKatakana = !nextIsHiragana && WanaKana.IsKatakana(next);
    if (nextIsHiragana || nextIsKatakana) {
      if (GameTypingUtils.IsSmallTsu(next)) {
        Next = Entry.Substring(idx, 2);
        NextVariants = [WanaKana.ToRomaji(Next).Trim()];
      }
      else if (Entry.Length > idx + 1 && GameTypingUtils.IsSmallKana(Entry[idx + 1])) {
        Next = Entry.Substring(idx, 2);
        NextVariants = [WanaKana.ToRomaji(Entry.Substring(idx, 2)).Trim()];
      }
      else {
        NextVariants = [WanaKana.ToRomaji(next.ToString()).Trim()];
      }

      if (next == 'ー') {
        NextVariants.Add(WanaKana.ToRomaji(oldNext).Last().ToString());
      }

      GameTypingUtils.PopulateAlternatives(Next, NextVariants);
    }
    else {
      NextVariants = null;
    }
  }

  public bool OnInput(string input) {
    if (Entry.StartsWith(InputBuffer + input)) {
      InputBuffer += input;
      if (InputBuffer.Length < Entry.Length) {
        SetNext(InputBuffer.Length);
      }

      return true;
    }

    OnMistake?.Invoke(input);
    return false;
  }
}
