using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace KyoS.Web.Models
{
    public class TCMIntakeAppendixJViewModel : TCMIntakeAppendixJEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdTCMClient { get; set; }

        public IEnumerable<SelectListItem> TcmClients { get; set; }

        [Display(Name = "Classification")]
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "You must select a Classification.")]

        public int IdType { get; set; }

        public IEnumerable<SelectListItem> AppendixJTypes { get; set; }
    }
}
