using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakeColumbiaSuicideEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        public int TcmClient_FK { get; set; }

        public bool HaveYouWishedPastMonth { get; set; }
        public RiskType HaveYouWishedPastMonth_Value { get; set; }

        public bool HaveYouWishedLifeTime { get; set; }
        public RiskType HaveYouWishedLifeTime_Value { get; set; }

        public bool HaveYouActuallyPastMonth { get; set; }
        public RiskType HaveYouActuallyPastMonth_Value { get; set; }

        public bool HaveYouActuallyLifeTime { get; set; }
        public RiskType HaveYouActuallyLifeTime_Value { get; set; }

        public bool HaveYouBeenPastMonth { get; set; }
        public RiskType HaveYouBeenPastMonth_Value { get; set; }

        public bool HaveYouBeenLifeTime { get; set; }
        public RiskType HaveYouBeenLifeTime_Value { get; set; }

        public bool HaveYouHadPastMonth { get; set; }
        public RiskType HaveYouHadPastMonth_Value { get; set; }

        public bool HaveYouHadLifeTime { get; set; }
        public RiskType HaveYouHadLifeTime_Value { get; set; }

        public bool HaveYouStartedPastMonth { get; set; }
        public RiskType HaveYouStartedPastMonth_Value { get; set; }

        public bool HaveYouStartedLifeTime { get; set; }
        public RiskType HaveYouStartedLifeTime_Value { get; set; }

        public bool HaveYouEver { get; set; }
        public RiskType HaveYouEver_Value { get; set; }

        public bool HaveYouEverIfYes { get; set; }
        public RiskType HaveYouEverIfYes_Value { get; set; }

        public string AdmissionedFor { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }
    }
}
