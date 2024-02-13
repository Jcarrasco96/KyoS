using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMTransferViewModel : TCMTransferEntity
    {
        public int IdCaseManagerFrom { get; set; }
        public IEnumerable<SelectListItem> TCMsFrom { get; set; }
        public int IdTCMClient { get; set; }
        public IEnumerable<SelectListItem> TCMClients { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Case Manager")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a TCM.")]
        public int IdCaseManagerTo { get; set; }
        public IEnumerable<SelectListItem> TCMsTo { get; set; }
        public int IdTCMSupervisor { get; set; }
       


    }
}
