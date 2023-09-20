using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakeAppendixJEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        public int TcmClient_FK { get; set; }

        public bool IsAwaiting { get; set; }
        public bool HasBeen { get; set; }
        public bool HasHad { get; set; }
        public bool IsAt { get; set; }
        public bool IsExperiencing { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]

        public DateTime Date { get; set; }

        public string AdmissionedFor { get; set; }

        [Display(Name = "Date of Supervisor Signature")]
        [DataType(DataType.Date)]

        public DateTime SupervisorSignatureDate { get; set; }

        public int Approved { get; set; }

        public TCMSupervisorEntity TcmSupervisor { get; set; }

        public bool IsEnrolled { get; set; }
        public bool HasAMental2 { get; set; }
        public bool RequiresServices { get; set; }
        public bool Lacks { get; set; }
        public bool RequiresOngoing { get; set; }
        public bool HasAMental6 { get; set; }
        public bool IsNotReceiving { get; set; }
        public bool Meets { get; set; }
        public bool HasRecolated { get; set; }
    }
}
