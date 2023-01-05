using KyoS.Web.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace KyoS.Web.Models
{
    public class FarsFormViewModel : FarsFormEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdClient { get; set; }
        public int IdSupervisor { get; set; }
        public int Origin { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Type")]
        [Range(0, int.MaxValue, ErrorMessage = "You must select a Type.")]
        public int IdType { get; set; }
        public IEnumerable<SelectListItem> FarsType { get; set; }
    }
}
