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
    public class TCMAssessmentViewModel : TCMAssessmentEntity
    {

        public List<Client_Referred> Client_Referred_List { get; set; }

        public string Psychiatrist_Name { get; set; }
        public string Psychiatrist_Phone { get; set; }
        public string Psychiatrist_Address { get; set; }
        public string Psychiatrist_CityStateZip { get; set; }

        public string PCP_Name { get; set; }
        public string PCP_Phone { get; set; }
        public string PCP_Address { get; set; }
        public string PCP_CityStateZip { get; set; }

        public int IdYesNoNAWe { get; set; }
        public IEnumerable<SelectListItem> YesNoNAs { get; set; }

        public int IdResidentStatus { get; set; }
        public IEnumerable<SelectListItem> ResidentStatuss { get; set; }
        
        public int IdEmploymentStatus { get; set; }
        public IEnumerable<SelectListItem> EmploymentStatuss { get; set; }

        public int IdFrecuencyActive { get; set; }
        public IEnumerable<SelectListItem> FrecuencyActiveList { get; set; }

        public int IdYesNoNAPregnancy { get; set; }

        public int IdYesNoNAAreChild { get; set; }

    }
}
