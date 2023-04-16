using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace KyoS.Web.Helpers
{
    public class FileHelper : IFileHelper
    {
        public byte[] Zip(List<FileContentResult> fileContentList)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (FileContentResult file in fileContentList)
                    {
                        var entry = archive.CreateEntry(file.FileDownloadName, CompressionLevel.Fastest);
                        using (var zipStream = entry.Open())
                        {
                            zipStream.Write(file.FileContents, 0, file.FileContents.Length);
                        }
                    }
                }
                return ms.ToArray();
            }
        }
    }
}
