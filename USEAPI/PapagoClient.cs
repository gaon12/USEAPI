using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace USEAPI
{
    public enum TranslationDirection
    {
        KoreanToEnglish,
        EnglishToKorean
    }

    public sealed class PapagoClient
    {
        private const string ApiUrl = "https://openapi.naver.com/v1/papago/n2mt";
        private const string ClientIdEnvironmentName = "NAVER_CLIENT_ID";
        private const string ClientSecretEnvironmentName = "NAVER_CLIENT_SECRET";

        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<string> TranslateAsync(string text, TranslationDirection direction)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("번역할 문장을 입력하세요.");
            }

            var clientId = Environment.GetEnvironmentVariable(ClientIdEnvironmentName);
            var clientSecret = Environment.GetEnvironmentVariable(ClientSecretEnvironmentName);
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "{0}, {1} 환경 변수를 설정해야 Papago 번역을 사용할 수 있습니다.",
                        ClientIdEnvironmentName,
                        ClientSecretEnvironmentName));
            }

            var languages = GetLanguages(direction);
            using (var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl))
            {
                request.Headers.Add("X-Naver-Client-Id", clientId);
                request.Headers.Add("X-Naver-Client-Secret", clientSecret);
                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("source", languages.Source),
                    new KeyValuePair<string, string>("target", languages.Target),
                    new KeyValuePair<string, string>("text", text)
                });

                using (var response = await HttpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            string.Format("Papago API 요청 실패: {0} {1}", (int)response.StatusCode, response.ReasonPhrase));
                    }

                    return ParseTranslatedText(body);
                }
            }
        }

        private string ParseTranslatedText(string json)
        {
            var response = serializer.Deserialize<PapagoResponse>(json);
            var translatedText = response == null ||
                response.Message == null ||
                response.Message.Result == null
                    ? null
                    : response.Message.Result.TranslatedText;
            if (string.IsNullOrEmpty(translatedText))
            {
                throw new InvalidOperationException("Papago API 응답에서 번역 결과를 찾을 수 없습니다.");
            }

            return translatedText;
        }

        private static LanguagePair GetLanguages(TranslationDirection direction)
        {
            return direction == TranslationDirection.KoreanToEnglish
                ? new LanguagePair("ko", "en")
                : new LanguagePair("en", "ko");
        }

        private sealed class LanguagePair
        {
            public LanguagePair(string source, string target)
            {
                Source = source;
                Target = target;
            }

            public string Source { get; private set; }

            public string Target { get; private set; }
        }

        private sealed class PapagoResponse
        {
            public PapagoMessage Message { get; set; }
        }

        private sealed class PapagoMessage
        {
            public PapagoResult Result { get; set; }
        }

        private sealed class PapagoResult
        {
            public string TranslatedText { get; set; }
        }
    }
}
