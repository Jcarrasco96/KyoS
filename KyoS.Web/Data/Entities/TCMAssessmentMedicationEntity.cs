using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMAssessmentMedicationEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMAssessmentEntity TcmAssessment { get; set; }

        public string ReasonPurpose { get; set; }

        public string Name { get; set; }

        public string Dosage { get; set; }

        public string Frequency { get; set; }

        public string Prescriber { get; set; }
        
        
    }
}
