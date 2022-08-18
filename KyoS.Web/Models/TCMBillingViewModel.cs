using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMBillingViewModel
    {
        [Display(Name = "Client")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        public static implicit operator TCMBillingViewModel(AddProgressNoteViewModel v)
        {
            throw new NotImplementedException();
        }
    }
}
