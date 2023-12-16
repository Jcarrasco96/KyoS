using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class ReferralFormEntity : AuditableEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }

        public string NameClient { get; set; }
        public string CaseNumber { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public string SSN { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string PrimaryPhone { get; set; }
        public string SecondaryPhone { get; set; }
        public string LegalGuardianName { get; set; }
        public string LegalGuardianPhone { get; set; }
        public string Dx { get; set; }
        public string Dx_Description { get; set; }
        public string ReferredBy_Name { get; set; }
        public string ReferredBy_Title { get; set; }
        public string ReferredBy_Phone { get; set; }
        public string MedicaidId { get; set; }
        public string HMO { get; set; }
        public int UnitsApproved { get; set; }
        [DataType(DataType.Date)]
        public DateTime AuthorizedDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime ExperatedDate { get; set; }
        public string Program { get; set; }
        public string AssignedTo { get; set; }
        public string NameSupervisor { get; set; }
        public string Comments { get; set; }
        public bool CaseAcceptedFacilitator { get; set; }
        public bool CaseAcceptedSupervisor { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Date of Assigned")]
        public DateTime DateAssigned { get; set; }

        public bool FacilitatorSign { get; set; }
        public bool SupervisorSign { get; set; }

        public SupervisorEntity Supervisor { get; set; }
        public FacilitatorEntity Facilitator { get; set; }

        public string AssignedBy { get; set; }
    }
}
