using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMDocumentViewModel
    {
        public int Id { get; set; }

        public int IdTCMClient { get; set; }
        public IEnumerable<DocumentEntity> Documents { get; set; }
    }
}
