using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace KyoS.Web.Helpers
{
    public class TranslateHelper : ITranslateHelper
    {
        public async Task<string> TranslateText(string fromLanguage, string toLanguage, string textToTranslate)
        {
            string traslation = string.Empty;
            string result;
            
            var client = new HttpClient();
            try
            {
                string[] sentences = textToTranslate.Split('.');
                foreach (string text in sentences)
                {
                    if (text != string.Empty)
                    {
                        var request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri($"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLanguage}&tl={toLanguage}&dt=t&q={HttpUtility.UrlEncode(text)}"),
                            Headers =
                            {
                                { "user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 " +
                                         "(KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36" }
                            },
                        };
                        using (var response = await client.SendAsync(request))
                        {
                            response.EnsureSuccessStatusCode();
                            result = await response.Content.ReadAsStringAsync();
                            result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
                            traslation = $"{traslation}{result}. ";
                        }
                    }
                }
                return traslation;
            }
            catch (Exception)
            {
                return textToTranslate;
            }
        }
        public async Task<string> TranslateTextAux(string fromLanguage, string toLanguage, string textToTranslate)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://nlp-translation.p.rapidapi.com/v1/translate?text=Hello%2C%20world!!&to=es&from=en"),
                Headers =
                    {
                        { "x-rapidapi-host", "nlp-translation.p.rapidapi.com" },
                        { "x-rapidapi-key", "3297a3b5a2msh20bb50ffc96cac2p1ba7b7jsn3b5fb87c3e46" },
                    },
            };
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            return body;
        }
    }
}
