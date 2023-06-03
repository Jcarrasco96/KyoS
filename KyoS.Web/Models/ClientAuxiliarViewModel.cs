using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class ClientAuxiliarViewModel : ClientEntity
    {
        public IEnumerable<MTPReviewEntity> MTPRList { get; set; }
    }
}
