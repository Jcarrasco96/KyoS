using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMAssessmentSurgeryEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMAssessmentEntity TcmAssessment { get; set; }

        public string TypeSurgery { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public string Hospital { get; set; }

        public string Outcome { get; set; }

    }
}
