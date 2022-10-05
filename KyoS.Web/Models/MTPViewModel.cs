using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class MTPViewModel : MTPEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a client.")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }
        public IEnumerable<GoalsTempEntity> GoalTempList { get; set; }
        public bool Review { get; set; }

        public int IdDocumentAssistant { get; set; }

    }
}
