using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface ITranslateHelper
    {
        Task<string> TranslateText(string fromLanguage, string toLanguage, string textToTranslate);
    }
}
