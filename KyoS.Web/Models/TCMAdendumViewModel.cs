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
    public class TCMAdendumViewModel : TCMAdendumEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime Date_Identified { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int ID_TcmServicePlan { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int ID_TcmDominio { get; set; }

        public string Needs_Identified { get; set; }
        public string Long_term { get; set; }

        public IEnumerable<SelectListItem> ListTcmServicePlan { get; set; }
        public IEnumerable<SelectListItem> TcmDominio { get; set; }
        
    }
}
