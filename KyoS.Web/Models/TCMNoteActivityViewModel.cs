using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMNoteActivityViewModel : TCMNoteActivityEntity
    {
        public int IdTCMNote { get; set; }
        public int IdTCMDomain { get; set; }

        //[Required(ErrorMessage = "The field {0} is mandatory.")]
        
       // public IEnumerable<SelectListItem> Domains { get; set; }
    }
}
