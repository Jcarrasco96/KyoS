using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class DocumentViewModel : DocumentEntity
    {
        [Display(Name = "Choose document")]
        [Required]
        public IFormFile DocumentFile { get; set; }

        [Display(Name = "Description")]
        public int IdDescription { get; set; }
        public IEnumerable<SelectListItem> Descriptions { get; set; }
    }
}
