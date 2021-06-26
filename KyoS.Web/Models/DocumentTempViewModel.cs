using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class DocumentTempViewModel : DocumentTempEntity
    {
        [Display(Name = "Choose document")]
        [Required]
        public IFormFile DocumentFile { get; set; }
    }
}
