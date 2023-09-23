﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMAssessmentPastCurrentServiceEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMAssessmentEntity TcmAssessment { get; set; }

        public string TypeService { get; set; }

        public string ProviderAgency { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateReceived{ get; set; }

        public EffectivenessType Efectiveness { get; set; }
    }
}
