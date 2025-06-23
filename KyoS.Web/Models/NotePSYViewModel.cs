using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class NotePSYViewModel : NotePSYEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Client")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]
        public int IdClient { get; set; }

        public IEnumerable<SelectListItem> Clients { get; set; }
    }
}
