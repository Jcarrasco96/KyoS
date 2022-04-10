using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class IntakeOrientationChecklistEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }


        [Display(Name = "Date of Legal Guardian Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureLegalGuardian { get; set; }

        [Display(Name = "Date of Person Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignaturePerson { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }

        public bool TourFacility { get; set; }
        public bool Rights { get; set; }
        public bool PoliceGrievancce { get; set; }
        public bool Insent { get; set; }
        public bool Services { get; set; }
        public bool Access { get; set; }
        public bool Code { get; set; }
        public bool Confidentiality { get; set; }
        public bool Methods { get; set; }
        public bool Explanation { get; set; }
        public bool Fire { get; set; }
        public bool PoliceTobacco { get; set; }
        public bool PoliceIllicit { get; set; }
        public bool PoliceWeapons { get; set; }
        public bool Identification { get; set; }
        public bool Program { get; set; }
        public bool Purpose { get; set; }
        public bool IndividualPlan { get; set; }
        public bool Discharge { get; set; }
        public bool AgencyPolice { get; set; }
        public bool AgencyExpectation { get; set; }
        public bool Education { get; set; }
        public bool TheAbove { get; set; }

        public bool Documents { get; set; }
    }
}
