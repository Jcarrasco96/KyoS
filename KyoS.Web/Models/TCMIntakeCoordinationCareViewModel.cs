using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMIntakeCoordinationCareViewModel : TCMIntakeCoordinationCareEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdTCMClient { get; set; }

        public string PCP_Name { get; set; }
        public string PCP_Address { get; set; }
        public string PCP_CityStateZip { get; set; }
        public string PCP_Phone { get; set; }
        public string PCP_FaxNumber { get; set; }
    }
}
