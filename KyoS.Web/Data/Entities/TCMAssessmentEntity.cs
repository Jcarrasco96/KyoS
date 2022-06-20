using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMAssessmentEntity : AuditableEntity
    {
        public int Id { get; set; }

        public int TcmClient_FK { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateAssessment { get; set; }

        //SECTION 2
        public bool ClientInput { get; set; }
        public bool Family { get; set; }
        public bool Referring { get; set; }
        public bool School { get; set; }
        public bool Treating { get; set; }
        public bool Caregiver { get; set; }
        public bool Review { get; set; }
        public bool Other { get; set; }
        public string OtherExplain { get; set; }

        //SECTION 3
        public string PresentingProblems { get; set; }

        //SECTION 4
        public string ChildMother { get; set; }
        public string ChildFather { get; set; }
        public bool Married { get; set; }
        public bool Divorced { get; set; }
        public bool Separated { get; set; }
        public bool NeverMarried { get; set; }
        public bool AreChild { get; set; }
        public string AreChildName { get; set; }
        public string AreChildPhone { get; set; }
        public string AreChildAddress { get; set; }
        public string AreChildCity { get; set; }
        public bool MayWe { get; set; }
        public bool MayWeNA { get; set; }



        public int Approved { get; set; }

       
    }
}
