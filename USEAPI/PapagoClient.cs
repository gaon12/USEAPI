using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace USEAPI;

public enum TranslationDirection
{
    KoreanToEnglish,
    EnglishToKorean
}

public sealed class PapagoClient
{
    private const string ApiUrl = "https://papago.apigw.ntruss.com/nmt/v1/translation";
    private const string ApiKeyIdEnvironmentName = "NCP_PAPAGO_API_KEY_ID";
    private const string ApiKeyEnvironmentName = "NCP_PAPAGO_API_KEY";

    private static readonly HttpClient HttpClient = new();

    public async Task<string> TranslateAsync(string text, TranslationDirection direction)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("번역할 문장을 입력하세요.");
        }

        var apiKeyId = Environment.GetEnvironmentVariable(ApiKeyIdEnvironmentName);
        var apiKey = Environment.GetEnvironmentVariable(ApiKeyEnvironmentName);
        if (string.IsNullOrWhiteSpace(apiKeyId) || string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                $"{ApiKeyIdEnvironmentName}, {ApiKeyEnvironmentName} 환경 변수를 설정해야 Papago 번역을 사용할 수 있습니다.");
        }

        var (source, target) = GetLanguages(direction);
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Add("X-NCP-APIGW-API-KEY-ID", apiKeyId);
        request.Headers.Add("X-NCP-APIGW-API-KEY", apiKey);
        request.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("source", source),
            new KeyValuePair<string, string>("target", target),
            new KeyValuePair<string, string>("text", text)
        ]);

        using var response = await HttpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Papago API 요청 실패: {(int)response.StatusCode} {response.ReasonPhrase}\r\n{body}");
        }

        var papagoResponse = await response.Content.ReadFromJsonAsync<PapagoResponse>();
        var translatedText = papagoResponse?.Message?.Result?.TranslatedText;
        if (string.IsNullOrEmpty(translatedText))
        {
            throw new InvalidOperationException("Papago API 응답에서 번역 결과를 찾을 수 없습니다.");
        }

        return translatedText;
    }

    private static (string Source, string Target) GetLanguages(TranslationDirection direction)
    {
        return direction == TranslationDirection.KoreanToEnglish
            ? ("ko", "en")
            : ("en", "ko");
    }

    private sealed class PapagoResponse
    {
        [JsonPropertyName("message")]
        public PapagoMessage? Message { get; set; }
    }

    private sealed class PapagoMessage
    {
        [JsonPropertyName("result")]
        public PapagoResult? Result { get; set; }
    }

    private sealed class PapagoResult
    {
        [JsonPropertyName("translatedText")]
        public string? TranslatedText { get; set; }
    }
}
