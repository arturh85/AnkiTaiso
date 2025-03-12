namespace ankitaiso.game_typing;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using utils;

public class GameTypingSystem {
  private List<Vocab> EntriesLeft = null!;
  private List<Vocab> EntriesInUse = new();
  private Vocab? EntryActive;

  public string Buffer { get; set; } = null!;
  public int TotalCount { get; set; }

  public int StatisticTotalSuccess { get; set; }
  public int StatisticTotalError { get; set; }
  public Dictionary<char, CharStatistic> StatisticByChar { get; set; } = new();

  private DateTimeOffset? Start;
  private DateTimeOffset? End;

  public event OnMistakeEvent? OnMistake;
  public delegate void OnMistakeEvent(string key, Vocab? vocab);

  public event OnLeftCountChangedEvent? OnLeftCountChanged;
  public delegate void OnLeftCountChangedEvent(int leftCount, int totalCount);

  public GameTypingSystem() {
    RestartGame((IEnumerable<Vocab>)[]);
  }

  public GameTypingSystem(IEnumerable<string> vocabs) {
    RestartGame(vocabs);
  }

  public GameTypingSystem(IEnumerable<Vocab> vocabs) {
    RestartGame(vocabs);
  }

  public void RestartGame(IEnumerable<Vocab> vocabs) {
    EntriesLeft = new List<Vocab>(vocabs.Reverse());
    TotalCount = EntriesLeft.Count;
    OnLeftCountChanged?.Invoke(EntriesLeft.Count, TotalCount);
    StatisticTotalError = 0;
    Buffer = "";
    Start = null;
    End = null;
    StatisticByChar = new();
    EntriesInUse = new();
    EntryActive = null;
  }

  public void RestartGame(IEnumerable<string> vocabs) => RestartGame(vocabs.Select(entry => new Vocab(entry)));


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
          }
          else {
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
      OnLeftCountChanged?.Invoke(EntriesLeft.Count + EntriesInUse.Count, TotalCount);
    }
    if (EntriesInUse.Count == 0 && EntriesLeft.Count == 0) {
      End = DateTimeOffset.Now;
    }
    if (success) {
      StatisticTotalSuccess += 1;
    }
    else {
      OnMistake?.Invoke(input, EntryActive);
      StatisticTotalError += 1;
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
