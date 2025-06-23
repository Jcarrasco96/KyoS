using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMClientInfoViewModel
    {
        public string Info { get; set; }
        public string Description { get; set; }
        
    }
}
