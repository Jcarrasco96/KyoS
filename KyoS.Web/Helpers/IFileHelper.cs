using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace KyoS.Web.Helpers
{
    public interface IFileHelper
    {
        public byte[] Zip(List<FileContentResult> fileContentList);
        public byte[] FileToByteArray(string filePath);
    }
}
