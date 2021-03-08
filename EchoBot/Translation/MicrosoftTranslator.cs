// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EchoBot.Translation.Model;
using Microsoft.BotBuilderSamples.Translation.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Translation
{
    public class MicrosoftTranslator
    {
        private const string Host = "https://api.cognitive.microsofttranslator.com";
        private const string TranslatePath = "/translate?api-version=3.0";
        private const string DetectPath = "/detect?api-version=3.0";
        private const string UriParams = "&to=";

        private static HttpClient _client = new HttpClient();

        private readonly string _key;


        public MicrosoftTranslator(IConfiguration configuration)

        {
            var key = configuration["TranslatorKey"];
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public async Task<string> DetectLanguageAsync(string text, CancellationToken cancellationToken = default(CancellationToken))
        {
            // From Cognitive Services translation documentation:
            // https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-csharp-translate
            var body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var request = new HttpRequestMessage())
            {
                var uri = Host + DetectPath;
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _key);

                var response = await _client.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"The call to the translation service returned HTTP status code {response.StatusCode}.");

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<DetectLanguageResult[]>(responseBody);

                return result?.FirstOrDefault()?.Language;
            }
        }
        
        public async Task<string> TranslateAsync(string text, string targetLocale, string sourceLocale = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // From Cognitive Services translation documentation:
            // https://docs.microsoft.com/en-us/azure/cognitive-services/translator/quickstart-csharp-translate
            var body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var request = new HttpRequestMessage())
            {
                if (!string.IsNullOrEmpty(sourceLocale))
                    sourceLocale = $"&from={sourceLocale}";

                var uri = Host + TranslatePath + UriParams + targetLocale + sourceLocale;
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _key);

                var response = await _client.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"The call to the translation service returned HTTP status code {response.StatusCode}.");
                
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TranslatorResponse[]>(responseBody);

                return result?.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text;
            }
        }
    }
}
