using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
namespace KyoS.Web.Helpers
{
    public interface IImageHelper
    {
        Task<string> UploadImageAsync(IFormFile imageFile, string folder);
        Task<string> UploadFileAsync(IFormFile file, string folder);
        byte[] ImageToByteArray(string imagePath);
        string TrimPath(string path);
        Task<string> UploadSignatureAsync(string dataUrl, string folder);
    }
}
