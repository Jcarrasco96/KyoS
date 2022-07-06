using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakeInterventionEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMIntakeInterventionLogEntity TcmInterventionLog { get; set; }

        public DateTime Date { get; set; }
        public string Activity { get; set; }

    }
}
