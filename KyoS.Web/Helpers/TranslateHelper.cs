using System;
using System.Net;
using System.Web;

namespace KyoS.Web.Helpers
{
    public class TranslateHelper : ITranslateHelper
    {
        public string TranslateText(string fromLanguage, string toLanguage, string textToTranslate)
        {
            string url = string.Empty;
            string traslation = string.Empty;
            string result = string.Empty;
            WebClient webClient = new WebClient
            {
                Encoding = System.Text.Encoding.UTF8
            };
            try
            {
                string[] sentences = textToTranslate.Split('.');
                foreach (string item in sentences)
                {
                    if (item != string.Empty)
                    {
                        url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLanguage}&tl={toLanguage}&dt=t&q={HttpUtility.UrlEncode(item)}";
                        result = webClient.DownloadString(url);
                        result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
                        traslation = $"{traslation}{result}. ";
                    }
                }
                return traslation;
            }
            catch
            {
                return "Error. It's not possible to translate";
            }
        }
    }
}
