using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMAssessmentMedicalProblemEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMAssessmentEntity TcmAssessment { get; set; }

        public string MedicalProblem { get; set; }

        public bool Client { get; set; }
        public bool Family { get; set; }

        public string Comments { get; set; }

    }
}
