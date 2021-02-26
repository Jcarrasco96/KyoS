using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace KyoS.Web.Helpers
{
    public class ImageHelper : IImageHelper
    {
        public async Task<string> UploadImageAsync(IFormFile imageFile, string folder)
        {
            string guid = Guid.NewGuid().ToString();
            string file = $"{guid}.jpg";
            string path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\{folder}", file);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"~/images/{folder}/{file}";
        }

        public byte[] ImageToByteArray(string imageFilePath)
        {
            FileStream stream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            
            List<byte> buffers = new List<byte>();
            
            using (BinaryReader br = new BinaryReader(stream))
            {
                byte[] chunk = br.ReadBytes(1024);

                while (chunk.Length > 0)
                {
                    buffers.AddRange(chunk);
                    chunk = br.ReadBytes(1024);
                }
            }

            return buffers.ToArray();
        }

        public string TrimPath(string path)
        {
            string init = path.Substring(1);
            return init.Replace("/", "\\");            
        }
    }
}
