using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMTransferEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TCMClient { get; set; }

        public string LegalGuardianName { get; set; }
        public string LegalGuardianPhone { get; set; }
        public bool ChangeInformation { get; set; }

        public string Address { get; set; }
        public string CityStateZip { get; set; }
        public string PrimaryPhone { get; set; }
        public string OtherPhone { get; set; }

        public string TransferFollow { get; set; }
        

        [DataType(DataType.Date)]
        [Display(Name = "Opening Date")]
        public DateTime OpeningDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Service Plan or Last SPR")]
        public DateTime DateServicePlanORLastSPR { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of last Service ")]
        public DateTime DateLastService { get; set; }

        public bool HasClientChart { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Audit")]
        public DateTime DateAudit { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Audit Sign")]
        public DateTime DateAuditSign { get; set; }

        public CaseMannagerEntity TCMAssignedTo { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Opening Date")]
        public DateTime OpeningDateAssignedTo { get; set; }

        public TCMSupervisorEntity TCMSupervisor { get; set; }

        public CaseMannagerEntity TCMAssignedFrom { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Transfer Date")]
        public DateTime EndTransferDate { get; set; }
        public bool TCMAssignedFromAccept { get; set; }
        public bool TCMAssignedToAccept { get; set; }
        public bool Return { get; set; }

        public bool TCMSupervisorAccept { get; set; }
    }
}
