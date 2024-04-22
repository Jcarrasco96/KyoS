using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMServiceNoteViewModel
    {
        //[Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Service")]
        [Range(0, int.MaxValue, ErrorMessage = "You must select a Service.")]

        public int IdTCMService { get; set; }
        public IEnumerable<SelectListItem> TCMServices { get; set; }

        [Display(Name = "Activity")]
        [Range(0, int.MaxValue, ErrorMessage = "You must select a Activity.")]

        public int IdTCMActivity { get; set; }
        public IEnumerable<SelectListItem> TCMServicesActivity { get; set; }

        public List<TCMNoteActivityEntity> TCMNoteActivities { get; set; }
    }
}
