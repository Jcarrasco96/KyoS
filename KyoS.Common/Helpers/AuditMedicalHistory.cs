using KyoS.Common.Enums;
using System;
using System.Collections.Generic;

namespace KyoS.Common.Helpers
{
    public class AuditMedicalHistory
    {
        public string Name { get; set; }
        public string AdmissionDate { get; set; }
        public string Description { get; set; }
        public int Active { get; set; }
    }
}
