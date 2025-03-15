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
  public Dictionary<string, CharStatistic> StatisticByChar { get; set; } = new();

  private DateTimeOffset? _start;
  private DateTimeOffset? _end;

  public DateTimeOffset? GetStart() => _start;
  public DateTimeOffset? GetEnd() => _end;

  public event OnWonEvent? OnWon;

  public delegate void OnWonEvent();

  public event OnHitEvent? OnHit;

  public delegate void OnHitEvent(string key, Vocab? vocab);

  public event OnMistakeEvent? OnMistake;

  public delegate void OnMistakeEvent(string key, Vocab? vocab);

  public event OnLeftCountChangedEvent? OnLeftCountChanged;

  public delegate void OnLeftCountChangedEvent(int leftCount, int totalCount);

  public GameTypingSystem() {
    RestartGame((IEnumerable<Vocab>) []);
  }

  public GameTypingSystem(IEnumerable<VocabEntry> vocabs) {
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
    _start = null;
    _end = null;
    StatisticByChar = new();
    EntriesInUse = new();
    EntryActive = null;
  }

  public void RestartGame(IEnumerable<VocabEntry> vocabs) => RestartGame(vocabs.Select(entry => new Vocab(entry)));


  public Vocab? NextEntry(bool startVisible = true) {
    var validNexts = EntriesInUse.Select(e => e.Entry.Prompt[0]).ToArray();
    var found = false;
    Vocab? vocab = null;
    for (var idx = EntriesLeft.Count - 1; idx >= 0; idx--) {
      vocab = EntriesLeft[idx];
      if (validNexts.Contains(vocab.Entry.Prompt[0])) {
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

    if (_start == null) {
      _start = DateTimeOffset.Now;
    }

    var bufferInput = Buffer + input;

    if (EntryActive != null) {
      if (EntryActive.Entry.Prompt.StartsWith(EntryActive.InputBuffer + input)) {
        var c = EntryActive.Next;
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
        success = EntryActive.OnInput(input);
      }
      else if (EntryActive.NextVariants != null) {
        var c = EntryActive.Next;
        if (!StatisticByChar.TryGetValue(c, out var value)) {
          value = new CharStatistic(c);
          StatisticByChar.Add(c, value);
        }
        foreach (var nextPlain in EntryActive.NextVariants) {
          if (!nextPlain.StartsWith(bufferInput)) {
            continue;
          }

          if (nextPlain == bufferInput) {
            success = EntryActive.OnInput(EntryActive.Next);
            Buffer = "";
            value.SuccessCount += 1;
          }
          else {
            Buffer = bufferInput;
            success = true;
          }

          break;
        }
        value.FailCount += 1;
      }
      else {
        var c = EntryActive.Next;
        if (!StatisticByChar.TryGetValue(c, out var value)) {
          value = new CharStatistic(c);
          StatisticByChar.Add(c, value);
        }
        value.FailCount += 1;
      }
    }
    else {
      foreach (var vocab in EntriesInUse) {
        if (vocab.State == VocabState.Hidden) {
          continue;
        }

        if (vocab.Entry.Prompt.StartsWith(input)) {
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

    var completed = false;
    if (EntryActive != null && EntryActive.Entry.Prompt == EntryActive.InputBuffer) {
      EntryActive.State = VocabState.Completed;
      EntriesInUse.Remove(EntryActive);
      completed = true;
      OnLeftCountChanged?.Invoke(EntriesLeft.Count + EntriesInUse.Count, TotalCount);
      if (EntriesInUse.Count == 0 && EntriesLeft.Count == 0) {
        _end = DateTimeOffset.Now;
        OnWon?.Invoke();
      }
    }

    if (success) {
      OnHit?.Invoke(input, EntryActive);
      StatisticTotalSuccess += 1;
    }
    else {
      OnMistake?.Invoke(input, EntryActive);
      StatisticTotalError += 1;
    }

    if (completed) {
      EntryActive = null;
    }

    return success;
  }

  public Vocab? GetActiveEntry() => EntryActive;

  public ImmutableList<Vocab> GetEntriesInUse() => EntriesInUse.ToImmutableList();

  public TimeSpan? GetDuration() {
    if (_start == null) {
      return null;
    }

    var end = _end ?? DateTimeOffset.Now;
    var diff = end - _start;
    return diff;
  }
}
