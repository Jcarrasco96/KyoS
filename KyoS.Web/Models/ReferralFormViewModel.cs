using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace KyoS.Web.Models
{
    public class ReferralFormViewModel : ReferralFormEntity
    {
        [Display(Name = "Supervisor")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Supervisor.")]
       
        public int IdSupervisor { get; set; }

        [Display(Name = "Facilitator")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Facilitator.")]

        public int IdFacilitator { get; set; }
        /* public int IdClient { get; set; }

        public int IdFacilitatorPSR { get; set; }
        public int IdFacilitatorInd { get; set; }
        public int IdFacilitatorGroup { get; set; }*/

        public IEnumerable<SelectListItem> SupervisorList { get; set; }
        public IEnumerable<SelectListItem> FacilitatorList { get; set; }
        public int IdClient { get; set; }
    }
}
