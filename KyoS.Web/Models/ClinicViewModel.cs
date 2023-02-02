using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class ClinicViewModel : ClinicEntity
    {
        [Display(Name = "Logo")]
        public IFormFile LogoFile { get; set; }

        [Display(Name = "Signature Clinical Director")]
        public IFormFile SignatureFile { get; set; }
    }
}
