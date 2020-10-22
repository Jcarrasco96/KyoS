using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class NoteViewModel : NoteEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Activity")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a activity.")]
        public int IdActivity { get; set; }
        public IEnumerable<SelectListItem> Activities { get; set; }

        [Display(Name = "Facilitator")]
        [Range(1, int.MaxValue, ErrorMessage = "Optionally select a facilitator.")]
        public int IdFacilitator { get; set; }
        public IEnumerable<SelectListItem> Facilitators { get; set; }

        [Display(Name = "Client")]
        [Range(1, int.MaxValue, ErrorMessage = "Optionally select a client.")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        [Display(Name = "Classification")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a classification.")]
        public int IdClassification { get; set; }
        public IEnumerable<SelectListItem> Classifications { get; set; }
    }
}
