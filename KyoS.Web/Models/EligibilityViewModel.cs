using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class EligibilityViewModel : EligibilityEntity
    {
        public int IdClient { get; set; }

        [Display(Name = "Choose document")]
        public IFormFile DocumentFile { get; set; }

        
    }
}
