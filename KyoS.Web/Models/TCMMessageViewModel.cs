using KyoS.Web.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class TCMMessageViewModel : TCMMessageEntity
    {
        public int IdTCMNote { get; set; }
        
        public int IdTCMAssessment { get; set; }

        public int IdTCMFarsForm { get; set; }

        public int IdTCMServiceplan { get; set; }

        public int IdTCMServiceplanReview { get; set; }

        public int IdTCMAddendum { get; set; }

        public int IdTCMDischarge { get; set; }

        public int Origin { get; set; }
    }
}
