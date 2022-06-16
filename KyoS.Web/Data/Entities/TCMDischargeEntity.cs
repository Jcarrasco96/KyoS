using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class TCMDischargeEntity
    {
        public int Id { get; set; }

        public int TcmServicePlan_FK { get; set; }

        public TCMServicePlanEntity TcmServicePlan { get; set; }

        [Display(Name = "Date of Staffing")]
        [DataType(DataType.Date)]

        public DateTime StaffingDate { get; set; }

        [Display(Name = "Date of Discharge")]
        [DataType(DataType.Date)]

        public DateTime DischargeDate { get; set; }

        [Display(Name = "Present Problems")]
        public string PresentProblems { get; set; }

        [Display(Name = "ProgressToward")]
        public string ProgressToward { get; set; }

        [Display(Name = "Date of Staff Signature")]
        [DataType(DataType.Date)]

        public DateTime StaffSignatureDate { get; set; }

        [Display(Name = "Date of Supervisor Signature")]
        [DataType(DataType.Date)]

        public DateTime SupervisorSignatureDate { get; set; }

        public int Approved { get; set; }

        public bool AllServiceInPlace { get; set; }
        public bool Referred { get; set; }
        public bool ClientLeftVoluntarily { get; set; }
        public bool NonComplianceWithAgencyRules { get; set; }
        public bool ClientMovedOutArea { get; set; }
        public bool LackOfProgress { get; set; }
        public bool Other { get; set; }
        public string Other_Explain { get; set; }
        public bool AdministrativeDischarge { get; set; }
        public string AdministrativeDischarge_Explain { get; set; }

        public List <TCMDischargeFollowUpEntity> TcmDischargeFollowUp { get; set; }
        public List<TCMDischargeServiceStatusEntity> TcmDischargeServiceStatus { get; set; }
    }
}
