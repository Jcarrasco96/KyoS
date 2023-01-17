using System;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace KyoS.Web.Models
{
    public class BIOTypeViewModel
    {
        public int IdClient { get; set; }

        public int IdType { get; set; }

        public IEnumerable<SelectListItem> Types { get; set; }
    }
}
