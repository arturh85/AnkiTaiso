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
  private List<Vocab> EntriesInUse;
  private Vocab? EntryActive;

  public string Buffer { get; set; }

  private DateTimeOffset? Start;
  private DateTimeOffset? End;

  public delegate void OnDelete();

  // private Dictionary<

  public GameTypingSystem(IEnumerable<string> vocabs) {
    EntriesLeft = new List<Vocab>(vocabs.Reverse().Select(vocab => new Vocab(vocab)));
    EntriesInUse = new();
  }


  public List<Vocab> NextEntries(int count) {
    var list = new List<Vocab>();
    for (int i = 0; i < count; i++) {
      var skipStart = list.Concat(EntriesInUse).Select(e => e.Entry[0]);
      var found = false;
      for (var idx = EntriesLeft.Count - 1; idx >= 0 ; idx--) {
        var vocab = EntriesLeft[idx];
        if (!skipStart.Contains(vocab.Entry[0])) {
          list.Add(vocab);
          EntriesLeft.Remove(vocab);
          found = true;
          break;
        }
      }
      if (!found && EntriesLeft.Count > 0) {
        list.Add(EntriesLeft.Last());
        EntriesLeft.RemoveAt(EntriesLeft.Count - 1);
      }
    }
    EntriesInUse.AddRange(list);
    return list;
  }

  public bool OnInput(Key key) {
    if (Start == null) {
      Start = DateTimeOffset.Now;
    }

    var success = false;
    var input = KeyboardUtils.KeyToString(key);
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
    } else {
      foreach (var vocab in EntriesInUse) {
        if (vocab.Entry.StartsWith(input)) {
          success = vocab.OnInput(input);
          EntryActive = vocab;
          break;
        }

        if (vocab.NextPlain != null && vocab.NextPlain.StartsWith(bufferInput)) {
          if (vocab.NextPlain == bufferInput) {
            if (vocab.NextIsHiragana) {
              success = vocab.OnInput(WanaKana.ToHiragana(bufferInput));
              Buffer = "";
              EntryActive = vocab;
              break;
            }
            if (vocab.NextIsKatakana) {
              success = vocab.OnInput(WanaKana.ToKatakana(bufferInput));
              Buffer = "";
              EntryActive = vocab;
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
      EntriesInUse.Remove(EntryActive);
      EntryActive = null;
    }
    if (EntriesInUse.Count == 0 && EntriesLeft.Count == 0) {
      End = DateTimeOffset.Now;
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
}

public class Vocab {
  public string Entry;
  public char Next;
  public string InputBuffer;
  public string? NextPlain;
  public bool NextIsKatakana;
  public bool NextIsHiragana;

  public Vocab(string entry) {
    Entry = entry;
    SetNext(entry[0]);
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
