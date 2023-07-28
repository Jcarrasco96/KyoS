using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class ReferredTempViewModel : ReferredTempEntity
    {
        [Display(Name = "Referred")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int Id_Referred { get; set; }
        public IEnumerable<SelectListItem> Referreds { get; set; }

        public int IdServiceAgency { get; set; }
        public IEnumerable<SelectListItem> ServiceAgency { get; set; }

        public int IdType { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }
    }
}
