using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class AddProgressNoteViewModel
    {
        public DateTime Date { get; set; }

        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a client.")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }
    }
}
