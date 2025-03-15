namespace ankitaiso.utils;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Godot;
using HttpClient = System.Net.Http.HttpClient;

public class AnkiConnectApi : IDisposable {
  private static AnkiConnectApi? _instance;
  private HttpClient _httpClient;

  public static AnkiConnectApi GetInstance() {
    if (_instance == null) {
      _instance = new AnkiConnectApi();
    }
    return _instance;
  }

  private AnkiConnectApi() {
    _httpClient = new HttpClient();
  }

  private async Task<AnkiResponse<T>> CallAnkiConnect<T>(AnkiRequest<T> request, Uri baseUrl) {
    var json = JsonSerializer.Serialize(request, JsonSerializerOptions.Web);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    var result = await _httpClient.PostAsync(baseUrl, content);
    var response = await request.DeserializeResponse(result);
    return response;
  }

  public async Task<string[]> ListDeckNames(Uri baseUrl) {
    var request = AnkiRequest.ListDeckNames();
    var response = await CallAnkiConnect(request, baseUrl);
    return response.Result ?? [];
  }
  public async Task<long[]> FindCardsByDeck(Uri baseUrl, string deckname) {
    var request = AnkiRequest.FindCards($"deck:\"{deckname}\"");
    var response = await CallAnkiConnect(request, baseUrl);
    return response.Result ?? [];
  }
  public async Task<CardInfo[]> CardsInfo(Uri baseUrl, long[] cards) {
    var request = AnkiRequest.CardsInfo(cards);
    var response = await CallAnkiConnect(request, baseUrl);
    return response.Result ?? [];
  }

  public async Task<NoteInfo[]> NotesInfo(Uri baseUrl, long[] notes) {
    var request = AnkiRequest.NotesInfo(notes);
    var response = await CallAnkiConnect(request, baseUrl);
    return response.Result ?? [];
  }
  public async Task<byte[]> RetrieveMediaFile(Uri baseUrl, string filename) {
    var request = AnkiRequest.RetrieveMediaFile(filename);
    var response = await CallAnkiConnect(request, baseUrl);
    return Convert.FromBase64String(response.Result!);
  }

  public void Dispose() {
    GC.SuppressFinalize(this);
    _httpClient.Dispose();
  }
}

internal sealed class AnkiResponse<T> {
  public T? Result { get; set; } = default!;
  public string? Error { get; set; } = default!;
}

internal sealed class AnkiRequest {
  public static AnkiRequest<long[]> FindCards(string queryString) {
    return new AnkiRequest<long[]> {
      Action = "findCards",
      Params = new AnkiRequestParams { Query = queryString }
    };
  }
  public static AnkiRequest<string[]> ListDeckNames() {
    return new AnkiRequest<string[]> {
      Action = "deckNames",
      Version = 6
    };
  }
  public static AnkiRequest<string> RetrieveMediaFile(string filename) {
    return new AnkiRequest<string> {
      Action = "retrieveMediaFile",
      Version = 6,
      Params = new AnkiRequestParams() {
        Filename = filename
      }
    };
  }
  public static AnkiRequest<CardInfo[]> CardsInfo(long[] cards) {
    return new AnkiRequest<CardInfo[]> {
      Action = "cardsInfo",
      Version = 6,
      Params = new AnkiRequestParams { Cards = cards }
    };
  }
  public static AnkiRequest<NoteInfo[]> NotesInfo(long[] notes) {
    return new AnkiRequest<NoteInfo[]> {
      Action = "notesInfo",
      Version = 6,
      Params = new AnkiRequestParams { Notes = notes }
    };
  }
}

public class NoteInfo {
  public long NoteId { get; set; }
  public string? Profile { get; set; }
  public string? ModelName { get; set; }
  public string[]? Tags { get; set; }
  public required Dictionary<string, CardInfoField> Fields { get; set; }
  public long Mod { get; set; }
  public long[]? Cards { get; set; }
}

public class CardInfoField {
  public string? Value { get; set; }
  public int Order { get; set; }
}

public class CardInfo {
  public string? Answer { get; set; }
  public string? Question { get; set; }
  public string? DeckName { get; set; }
  public string? ModelName { get; set; }
  public int FieldOrder { get; set; }
  public required Dictionary<string, CardInfoField> Fields { get; set; }
  public string? Css { get; set; }
  public long CardId { get; set; }
  public int Interval { get; set; }
  public long Note { get; set; }
  public int Ord { get; set; }
  public int Type { get; set; }
  public int Queue { get; set; }
  public int Due { get; set; }
  public int Reps { get; set; }
  public int Lapses { get; set; }
  public int Left { get; set; }
  public long Mod { get; set; }
}

internal sealed class AnkiRequest<T> {
  public required string Action { get; set; } = default!;
  public int Version { get; set; } = 6;
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public AnkiRequestParams? Params { get; set; } = default!;

  public async Task<AnkiResponse<T>> DeserializeResponse(HttpResponseMessage response) {
    try {
      var obj = await JsonSerializer.DeserializeAsync<AnkiResponse<T>>(response.Content.ReadAsStream(),
        JsonSerializerOptions.Web);

      if (obj == null) {
        throw new GameException("failed to deserialize response");
      }
      return obj;
    }
    catch (Exception) {
      var s = await response.Content.ReadAsStringAsync();
      GD.Print(s);
      throw;
    }
  }

  // todo:
  // getMediaFilesNames
  // retrieveMediaFile

}

internal sealed class AnkiRequestParams {
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? Query { get; set; }
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public long[]? Cards { get; set; }
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public long[]? Notes { get; set; }

  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public string? Filename { get; set; }
}
