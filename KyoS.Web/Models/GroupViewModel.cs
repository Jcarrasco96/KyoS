using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class GroupViewModel : GroupEntity
    {
        [Display(Name = "Facilitator")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Facilitator.")]
        public int IdFacilitator { get; set; }
        public IEnumerable<SelectListItem> Facilitators { get; set; }
    }
}
