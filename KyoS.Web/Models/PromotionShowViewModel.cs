using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class PromotionShowViewModel : PromotionEntity
    {

        [Display(Name = "Photo")]
        public IFormFile PhotoFile { get; set; }
        public int IdPromotion { get; set; }
        public string PhotoPath { get; set; }
    }
}
