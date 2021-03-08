using Newtonsoft.Json;
using System.Collections.Generic;

namespace EchoBot.Translation.Model
{
    internal class DetectLanguageResult
    {
        [JsonProperty("language")]
        public string Language { get; set; }
        [JsonProperty("score")]
        public float Score { get; set; }
        [JsonProperty("isTranslationSupported")]
        public bool IsTranslationSupported { get; set; }
        [JsonProperty("isTransliterationSupported")]
        public bool IsTransliterationSupported { get; set; }
        [JsonProperty("alternatives")]
        public IEnumerable<LanguageAlternative> Alternatives { get; set; }

        internal class LanguageAlternative
        {
            [JsonProperty("language")]
            public string Language { get; set; }
            [JsonProperty("score")]
            public float Score { get; set; }
            [JsonProperty("isTranslationSupported")]
            public bool IsTranslationSupported { get; set; }
            [JsonProperty("isTransliterationSupported")]
            public bool IsTransliterationSupported { get; set; }
        }
    }

    internal class DetectLanguageResponse
    {
        public IEnumerable<DetectLanguageResult> Languages { get; set; }
    }
}
