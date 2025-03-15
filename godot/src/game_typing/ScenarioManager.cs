namespace ankitaiso.game_typing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;
using utils;

public class ScenarioManager {
  public static string UserScenarioPath = "user://scenarios";
  public static string DeckDirPath(string deckName) => $"{UserScenarioPath}/{deckName}";
  public static string WordListPath(string deckName) => $"{DeckDirPath(deckName)}/{WORDLIST_FILENAME}";
  private const string WORDLIST_FILENAME = "wordlist.txt";
  private const string MAPPING_FILENAME = "mapping.json";

  public static bool ScenarioExists(string deckName) =>
    FileAccess.FileExists($"{DeckDirPath(deckName)}/{MAPPING_FILENAME}");

  public static async Task<VocabDeck> ImportScenario(Uri ankiUri, string deckName, VocabMapping mapping,
    Action<double>? onUpdateProgress) {
    var ankiService = AnkiConnectApi.GetInstance();
    var cardIds = await ankiService.FindCardsByDeck(ankiUri, deckName);
    if (cardIds.Length == 0) {
      throw new GameException("No cards found!");
    }

    const int batchSize = 10;

    var vocabDeck = new VocabDeck { Title = deckName, Entries = [] };
    List<VocabEntry> entries = [];
    var dirPath = DeckDirPath(deckName);
    DirAccess.MakeDirRecursiveAbsolute(dirPath);


    for (var i = 0; i < cardIds.Length; i += batchSize) {
      var batch = cardIds.Skip(i).Take(batchSize).ToArray();
      var cardInfos = await ankiService.CardsInfo(ankiUri, batch);
      if (cardInfos.Length == 0) {
        throw new GameException("No cardInfo found!");
      }

      foreach (var cardInfo in cardInfos) {
        var entry = BuildVocabEntry(cardInfo, mapping);
        entries.Add(entry);
        if (entry.AudioFilename != null && !FileAccess.FileExists($"{DeckDirPath(deckName)}/{entry.AudioFilename}")) {
          var contents = await ankiService.RetrieveMediaFile(ankiUri, entry.AudioFilename);
          using var audioFile = FileAccess.Open($"{dirPath}/{entry.AudioFilename}", FileAccess.ModeFlags.Write);
          if (audioFile == null) {
            throw new GameException($"failed to create {entry.AudioFilename} for '{deckName}'");
          }

          audioFile.StoreBuffer(contents);
        }
      }

      var progress = (double)(i + batch.Length) / cardIds.Length * 100;
      onUpdateProgress?.Invoke(progress);
    }
    vocabDeck.Entries = entries.ToArray();
    StoreDeck(vocabDeck);
    StoreMapping(mapping, deckName);
    return vocabDeck;
  }

  private static void StoreDeck(VocabDeck vocabDeck) {
    var deckName = vocabDeck.Title;
    var contents = "";
    foreach (var entry in vocabDeck.Entries) {
      List<string> parts = [entry.Prompt, entry.Title ?? "", entry.Translation ?? "", entry.AudioFilename ?? ""];
      foreach (var part in parts) {
        if (part.Contains('|')) {
          throw new GameException("found | in anki deck");
        }
      }
      if (parts[3] == "") {
        parts.RemoveAt(3);
        if (parts[2] == "") {
          parts.RemoveAt(2);
        }
        if (parts[1] == "") {
          parts.RemoveAt(1);
        }
      }
      var line = string.Join("|", parts);
      contents += line + "\n";
    }
    using var file = FileAccess.Open($"{WordListPath(deckName)}", FileAccess.ModeFlags.Write);
    if (file == null) {
      throw new GameException($"failed to create {WORDLIST_FILENAME} for '{deckName}'");
    }
    file.StoreString(contents);
  }

  private static void StoreMapping(VocabMapping mapping, string deckName) {
    var dirPath = DeckDirPath(deckName);
    DirAccess.MakeDirRecursiveAbsolute(dirPath);
    var json = JsonSerializer.Serialize(mapping, JsonSerializerOptions.Web);
    using var file = FileAccess.Open($"{dirPath}/{MAPPING_FILENAME}", FileAccess.ModeFlags.Write);
    if (file == null) {
      throw new GameException($"failed to create {MAPPING_FILENAME} for '{deckName}'");
    }

    file.StoreString(json);
  }

  public static VocabDeck? LoadDeck(string deckName) {
    var filePath = WordListPath(deckName);
    if (!FileAccess.FileExists(filePath)) {
      return null;
    }
    using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
    if (file == null) {
      throw new GameException($"failed to read {WORDLIST_FILENAME} for '{deckName}'");
    }
    var contents = file.GetAsText();
    var deck = new VocabDeck { Title = deckName };
    var entries = new List<VocabEntry>();
    var lines = contents.Split("\n");
    foreach (var line in lines) {
      entries.Add(new VocabEntry(line));
    }
    deck.Entries = entries.ToArray();
    return deck;
  }

  public static VocabMapping? LoadMapping(string deckName) {
    var dirPath = DeckDirPath(deckName);
    var filePath = $"{dirPath}/{MAPPING_FILENAME}";
    if (!FileAccess.FileExists(filePath)) {
      return null;
    }

    using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
    if (file == null) {
      throw new GameException($"failed to read {MAPPING_FILENAME} for '{deckName}'");
    }

    var obj = JsonSerializer.Deserialize<VocabMapping>(file.GetAsText(), JsonSerializerOptions.Web);
    return obj;
  }

  public static VocabEntry BuildVocabEntry(CardInfo card, VocabMapping mapping) {
    var audioFilename = mapping.AudioKey == null ? null : card.Fields[mapping.AudioKey].Value;
    var pattern = @"\[sound:(.*?)\]";
    var match = Regex.Match(audioFilename ?? "", pattern);
    if (match.Success) {
      audioFilename = match.Groups[1].ToString();
    }
    else if (audioFilename == "") {
      audioFilename = null;
    }

    return new VocabEntry {
      Prompt = card.Fields[mapping.PromptKey].Value ?? "missing",
      Translation = mapping.TranslationKey == null ? null : card.Fields[mapping.TranslationKey].Value,
      AudioFilename = audioFilename,
      Title = mapping.TitleKey == null ? null : card.Fields[mapping.TitleKey].Value
    };
  }
}
