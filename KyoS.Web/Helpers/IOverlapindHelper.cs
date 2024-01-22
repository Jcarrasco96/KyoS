using KyoS.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface IOverlapindHelper
    {
        bool OverlapingDocumentsAssistant(int idDocumentAssistant, DateTime initialTime, DateTime endTime);
       
    }
}
