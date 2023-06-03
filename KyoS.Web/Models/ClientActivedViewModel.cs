using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class ClientActivedViewModel : ClientEntity
    {
        public int Days { get; set; }
        public string DocumentType { get; set; }
        public int MtpId { get; set; }
        public int DaysInd { get; set; }
        public string DocumentTypeInd { get; set; }
    }
}
