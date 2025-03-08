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

  public string Buffer { get; set; }
  public int ErrorCount { get; set; }
  public Dictionary<char, CharStatistic> StatisticByChar { get; set; } = new();

  private DateTimeOffset? Start;
  private DateTimeOffset? End;

  public delegate void OnDelete();

  // private Dictionary<

  public GameTypingSystem(IEnumerable<string> vocabs) {
    EntriesLeft = new List<Vocab>(vocabs.Reverse().Select(vocab => new Vocab(vocab)));
  }


  public Vocab NextEntry(bool startVisible = true) {
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

    EntriesInUse.Add(vocab);
    return vocab;
  }

  public List<Vocab> NextEntries(int count, bool startVisible = true) {
    var list = new List<Vocab>();
    for (int i = 0; i < count; i++) {
      list.Add(NextEntry(startVisible));
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
      else if (EntryActive.NextPlain != null && EntryActive.NextPlain.StartsWith(bufferInput)) {
        if (EntryActive.NextPlain == bufferInput) {
          if (EntryActive.NextIsHiragana) {
            success = EntryActive.OnInput(WanaKana.ToHiragana(bufferInput));
            Buffer = "";
          }

          if (EntryActive.NextIsKatakana) {
            success = EntryActive.OnInput(WanaKana.ToKatakana(bufferInput));
            Buffer = "";
          }
        }
        else {
          Buffer = bufferInput;
          success = true;
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

        if (vocab.NextPlain != null && vocab.NextPlain.StartsWith(bufferInput)) {
          if (vocab.NextPlain == bufferInput) {
            if (vocab.NextIsHiragana) {
              success = vocab.OnInput(WanaKana.ToHiragana(bufferInput));
              Buffer = "";
              SetActive(vocab);
              break;
            }

            if (vocab.NextIsKatakana) {
              success = vocab.OnInput(WanaKana.ToKatakana(bufferInput));
              Buffer = "";
              SetActive(vocab);
              break;
            }
          }
          else {
            Buffer = bufferInput;
            success = true;
            break;
          }
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
  public string Entry;
  public char Next;
  public string InputBuffer;
  public string? NextPlain;
  public bool NextIsKatakana;
  public bool NextIsHiragana;

  public VocabState State;


  public Vocab(string entry) {
    Entry = entry.Trim();
    SetNext(Entry[0]);
    State = VocabState.Hidden;
    InputBuffer = "";
  }

  private void SetNext(char next) {
    Next = next;
    NextIsHiragana = WanaKana.IsHiragana(next);
    NextIsKatakana = !NextIsHiragana && WanaKana.IsKatakana(next);
    if (NextIsHiragana || NextIsKatakana) {
      NextPlain = WanaKana.ToRomaji(next.ToString()).Trim();
    }
    else {
      NextPlain = null;
    }
  }

  public bool OnInput(string input) {
    if (Entry.StartsWith(InputBuffer + input)) {
      InputBuffer += input;
      if (InputBuffer.Length < Entry.Length) {
        SetNext(Entry[InputBuffer.Length]);
      }

      return true;
    }

    return false;
  }
}
