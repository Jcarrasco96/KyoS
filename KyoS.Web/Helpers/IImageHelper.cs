using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
namespace KyoS.Web.Helpers
{
    public interface IImageHelper
    {
        Task<string> UploadImageAsync(IFormFile imageFile, string folder);
        byte[] ImageToByteArray(string imagePath);
        string TrimPath(string path);
    }
}
