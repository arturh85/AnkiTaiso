namespace ankitaiso.game_typing;

using System;
using System.Collections.Generic;
using System.Linq;
using Pinyin4net;
using Pinyin4net.Format;
using utils;
using WanaKanaNet;

public class Vocab {
  public VocabState State;
  public readonly VocabEntry Entry;
  public string Next;
  public string InputBuffer;
  public List<string>? NextVariants;

  private static readonly HangeulRomaniser _hangeulRomaniser = new ();
  private static readonly HanyuPinyinOutputFormat _format = new () {
    VCharType = HanyuPinyinVCharType.WITH_U_AND_COLON,
    CaseType = HanyuPinyinCaseType.LOWERCASE,
    ToneType = HanyuPinyinToneType.WITHOUT_TONE
  };

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
    // japanese kanas
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
      GameTypingUtils.PopulateKanaAlternatives(Next, NextVariants);
    }
    else if (WanaKana.IsRomaji(next)) {
      NextVariants = null;
    }
    else {
      // korean alphabet
      var roman = _hangeulRomaniser.Romanise(next);
      if (roman != null) {
        NextVariants = [roman];
      }
      else {
        // chinese alphabet
        try {
          NextVariants = PinyinHelper.ToHanyuPinyinStringArray(next, _format).Distinct().ToList();
          if (NextVariants.Count == 0) {
            NextVariants = null;
          }
        }
        catch (Exception) {
          NextVariants = null;
        }
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
