using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMAssessmentDrugEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMAssessmentEntity TcmAssessment { get; set; }

        public string SustanceName { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateBegin { get; set; }

        public int Age  { get; set; }

        public string Frequency { get; set; }

        public string LastTimeUsed { get; set; }

    }
}
