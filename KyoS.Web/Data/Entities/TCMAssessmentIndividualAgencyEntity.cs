using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMAssessmentIndividualAgencyEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMAssessmentEntity TcmAssessment { get; set; }

        public string Name { get; set; }

        public string Agency { get; set; }

        public string RelationShip { get; set; }


    }
}
