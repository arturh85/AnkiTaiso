namespace ankitaiso.game_typing;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using utils;
using WanaKanaNet;

public class GameTypingSystem {
  private List<Vocab> EntriesLeft;
  private List<Vocab> EntriesInUse = new();
  private Vocab? EntryActive;

  public string Buffer { get; set; } = "";
  public int TotalCount { get; set; }
  public int ErrorCount { get; set; }
  public Dictionary<char, CharStatistic> StatisticByChar { get; set; } = new();

  private DateTimeOffset? Start;
  private DateTimeOffset? End;

  public delegate void OnDelete();

  public GameTypingSystem(IEnumerable<string> vocabs) {
    EntriesLeft = new List<Vocab>(vocabs.Reverse().Select(vocab => new Vocab(vocab)));
    TotalCount = EntriesLeft.Count;
  }
  public GameTypingSystem(IEnumerable<Vocab> vocabs) {
    EntriesLeft = new List<Vocab>(vocabs.Reverse());
    TotalCount = EntriesLeft.Count;
  }

  public Vocab? NextEntry(bool startVisible = true) {
    var validNexts = EntriesInUse.Select(e => e.Entry[0]).ToArray();
    var found = false;
    Vocab? vocab = null;
    for (var idx = EntriesLeft.Count - 1; idx >= 0; idx--) {
      vocab = EntriesLeft[idx];
      if (validNexts.Contains(vocab.Entry[0])) {
        continue;
      }
      vocab.State = startVisible ? VocabState.Visible : VocabState.Hidden;
      EntriesLeft.Remove(vocab);
      found = true;
      break;
    }
    if (!found && EntriesLeft.Count > 0) {
      var vocabIdx = EntriesLeft.Count - 1;
      vocab = EntriesLeft[vocabIdx];
      vocab.State = startVisible ? VocabState.Visible : VocabState.Hidden;
      EntriesLeft.RemoveAt(vocabIdx);
    }
    if (vocab != null) {
      EntriesInUse.Add(vocab);
    }
    return vocab;
  }

  public List<Vocab> NextEntries(int count, bool startVisible = true) {
    var list = new List<Vocab>();
    for (var i = 0; i < count; i++) {
      var entry = NextEntry(startVisible);
      if (entry != null) {
        list.Add(entry);
      }
    }
    return list;
  }

  private void SetActive(Vocab vocab) {
    EntryActive = vocab;
    vocab.State = VocabState.Active;
  }

  public bool OnInput(Key key) {
    var success = false;
    var input = KeyboardUtils.KeyToString(key);
    if (input.Length == 0) {
      return true;
    }

    if (Start == null) {
      Start = DateTimeOffset.Now;
    }

    var bufferInput = Buffer + input;

    if (EntryActive != null) {
      if (EntryActive.Entry.StartsWith(EntryActive.InputBuffer + input)) {
        success = EntryActive.OnInput(input);
      }
      else if (EntryActive.NextVariants != null) {
        foreach (var nextPlain in EntryActive.NextVariants) {
          if (!nextPlain.StartsWith(bufferInput)) {
            continue;
          }
          if (nextPlain == bufferInput) {
            success = EntryActive.OnInput(EntryActive.Next);
            Buffer = "";
          }
          else {
            Buffer = bufferInput;
            success = true;
          }
          break;
        }
      }
    }
    else {
      foreach (var vocab in EntriesInUse) {
        if (vocab.State == VocabState.Hidden) {
          continue;
        }
        if (vocab.Entry.StartsWith(input)) {
          success = vocab.OnInput(input);
          SetActive(vocab);
          break;
        }

        if (vocab.NextVariants == null) {
          continue;
        }

        foreach (var nextPlain in vocab.NextVariants) {
          if (!nextPlain.StartsWith(bufferInput)) {
            continue;
          }
          if (nextPlain == bufferInput) {
            success = vocab.OnInput(vocab.Next);
            Buffer = "";
            SetActive(vocab);
          } else {
            Buffer = bufferInput;
            success = true;
          }
          break;
        }
      }
    }
    if (EntryActive != null && EntryActive.Entry == EntryActive.InputBuffer) {
      EntryActive.State = VocabState.Completed;
      EntriesInUse.Remove(EntryActive);
      EntryActive = null;
    }
    if (EntriesInUse.Count == 0 && EntriesLeft.Count == 0) {
      End = DateTimeOffset.Now;
    }
    if (!success) {
      ErrorCount += 1;
    }
    var c = input[0];
    if (!StatisticByChar.TryGetValue(c, out var value)) {
      value = new CharStatistic(c);
      StatisticByChar.Add(c, value);
    }
    if (success) {
      value.SuccessCount += 1;
    }
    else {
      value.FailCount += 1;
    }

    return success;
  }

  public Vocab? GetActiveEntry() => EntryActive;

  public ImmutableList<Vocab> GetEntriesInUse() => EntriesInUse.ToImmutableList();
}

public class CharStatistic {
  public char character;
  public int FailCount;
  public int SuccessCount;

  public CharStatistic(char c) {
    character = c;
  }
}

public enum VocabState {
  Hidden,
  Visible,
  Active,
  Completed
}

public class Vocab {
  public readonly string Entry;
  public string Next;
  public string InputBuffer;
  public List<string>? NextVariants;

  public VocabState State;

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
      else if (Entry.Length > idx+1 && GameTypingUtils.IsSmallKana(Entry[idx+1])) {
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
    return false;
  }
}
