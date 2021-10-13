using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class HealthInsuranceViewModel : HealthInsuranceEntity
    {
        [Display(Name = "Attachment")]        
        public IFormFile DocumentFile { get; set; }

        public int IdClinic { get; set; }
    }
}
