using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMIntakeMedicalHistoryViewModel : TCMIntakeMedicalHistoryEntity
    {
        [Display(Name = "TCM Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a TCM Client.")]

        public int IdTCMClient { get; set; }
        public int IdDoctor { get; set; }

    }
}
