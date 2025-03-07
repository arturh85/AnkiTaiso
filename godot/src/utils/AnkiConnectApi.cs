namespace ankitaiso.utils;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

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

  public async Task<string[]> ListDeckNames(Uri baseUrl) {
    var request = AnkiRequest.ListDeckNames();
    var result = await _httpClient.PostAsync(baseUrl, JsonContent.Create(request));
    var response = await request.DeserializeResponse(result);
    return response.Result ?? [];
  }
}

internal class AnkiResponse<T> {
  public T? Result;
  public string? Error;
}

internal class AnkiRequest {
  public static AnkiRequest<string[]> FindCards(string queryString) {
    return new AnkiRequest<string[]> {
      Action = "findCards",
      Params = new AnkiRequestParams { Query = queryString }
    };
  }
  public static AnkiRequest<string[]> ListDeckNames() {
    return new AnkiRequest<string[]> {
      Action = "deckNames"
    };
  }
}


internal class AnkiRequest<T> {
  public required string Action;
  public int Version = 6;
  public AnkiRequestParams Params;

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

internal class AnkiRequestParams {
  public string? Query;
  public string[]? Cards;
}
