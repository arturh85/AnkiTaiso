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

  public static string DeckDirPath(string deckName) => $"user://scenarios/{deckName}";
  private const string DECK_FILENAME = "deck.json";
  private const string MAPPING_FILENAME = "mapping.json";

  public static bool ScenarioExists(string deckName) => FileAccess.FileExists($"{DeckDirPath(deckName)}/{DECK_FILENAME}");

  public static async Task<VocabDeck> ImportScenario(Uri ankiUri, string deckName, VocabMapping mapping, Action<double>? onUpdateProgress) {

    var ankiService = AnkiConnectApi.GetInstance();
    var cardIds = await ankiService.FindCardsByDeck(ankiUri, deckName);
    if (cardIds.Length == 0) {
      throw new GameException("No cards found!");
    }

    const int batchSize = 10;

    var vocabDeck = new VocabDeck {
          Title = deckName,
          Entries = []
      };
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
                      throw new GameException($"failed to create {DECK_FILENAME} for '{deckName}'");
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
    var dirPath = DeckDirPath(deckName);
    DirAccess.MakeDirRecursiveAbsolute(dirPath);
    var json = JsonSerializer.Serialize(vocabDeck, JsonSerializerOptions.Web);
    using var file = FileAccess.Open($"{dirPath}/{DECK_FILENAME}", FileAccess.ModeFlags.Write);
    if (file == null) {
      throw new GameException($"failed to create {DECK_FILENAME} for '{deckName}'");
    }
    file.StoreString(json);
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
    var dirPath = DeckDirPath(deckName);
    var filePath = $"{dirPath}/{DECK_FILENAME}";
    if (!FileAccess.FileExists(filePath)) {
      return null;
    }

    using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
    if (file == null) {
      throw new GameException($"failed to create {DECK_FILENAME} for '{deckName}'");
    }

    var obj = JsonSerializer.Deserialize<VocabDeck>(file.GetAsText(), JsonSerializerOptions.Web);
    return obj;
  }

  public static VocabMapping? LoadMapping(string deckName) {
    var dirPath = DeckDirPath(deckName);
    var filePath = $"{dirPath}/{MAPPING_FILENAME}";
    if (!FileAccess.FileExists(filePath)) {
      return null;
    }
    using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
    if (file == null) {
      throw new GameException($"failed to create {DECK_FILENAME} for '{deckName}'");
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
    } else if (audioFilename == "") {
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
