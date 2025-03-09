namespace ankitaiso.utils;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Fractural.Tasks;

public class AnkiConnectApi {
  private static AnkiConnectApi? _instance = null;
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
}

class AnkiResponse<T> {
  public T? Result { get; set; } = default!;
  public string? Error { get; set; } = default!;
}

class AnkiRequest {
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
}

class AnkiRequest<T> {
  public required string Action { get; set; } = default!;
  public int Version { get; set; } = 6;
  [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public AnkiRequestParams? Params { get; set; } = default!;

  public async Task<AnkiResponse<T>> DeserializeResponse(HttpResponseMessage response) {
    var obj = await JsonSerializer.DeserializeAsync<AnkiResponse<T>>(response.Content.ReadAsStream(), JsonSerializerOptions.Web);
    if (obj == null) {
      throw new GameException("failed to deserialize response");
    }
    return obj;
  }

  // todo:
  // getMediaFilesNames
  // retrieveMediaFile

}

class AnkiRequestParams {
  public string? Query;
  public string[]? Cards;
}
