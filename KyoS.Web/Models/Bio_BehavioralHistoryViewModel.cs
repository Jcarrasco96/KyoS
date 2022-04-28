using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Entities;

namespace KyoS.Web.Models
{
    public class Bio_BehavioralHistoryViewModel : Bio_BehavioralHistoryEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdClient { get; set; }
    }
}
