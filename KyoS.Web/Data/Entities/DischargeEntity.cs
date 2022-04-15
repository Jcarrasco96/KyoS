using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;


namespace KyoS.Web.Data.Entities
{
    public class DischargeEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }

        [Display(Name = "Date of Report")]
        [DataType(DataType.Date)]

        public DateTime DateReport { get; set; }

        [Display(Name = "Date of Discharge")]
        [DataType(DataType.Date)]

        public DateTime DateDischarge { get; set; }

        public bool Planned { get; set; }

        public string ReasonDischarge { get; set; }

        public string BriefHistory { get; set; }

        public string CourseTreatment { get; set; }

        public string ConditionalDischarge { get; set; }

        public string ReferralFor1 { get; set; }

        public string ReferralAgency1 { get; set; }

        public string ReferralContactPersonal1 { get; set; }

        public string ReferralPhone1 { get; set; }

        public string ReferralHoursOperation1 { get; set; }

        public string ReferralFor2 { get; set; }

        public string ReferralAgency2 { get; set; }

        public string ReferralContactPersonal2 { get; set; }

        public string ReferralPhone2 { get; set; }

        public string ReferralHoursOperation2 { get; set; }

        public string FollowDischarge { get; set; }

        public bool TreatmentPlanObjCumpl { get; set; }

        public bool AgencyDischargeClient { get; set; }

        public bool ClientDischargeAgainst { get; set; }

        public bool ClientDeceased { get; set; }

        public bool ClientMoved { get; set; }

        public bool PhysicallyUnstable { get; set; }

        public bool Hospitalization { get; set; }

        public bool ClientReferred { get; set; }

        public bool Others { get; set; }

        public string AdmissionedFor { get; set; }

        [Display(Name = "Date of Client Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignaturePerson { get; set; }

        [Display(Name = "Date of Staff Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }
    }
}
