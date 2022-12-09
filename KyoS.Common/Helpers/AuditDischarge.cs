using KyoS.Common.Enums;
using System;
using System.Collections.Generic;

namespace KyoS.Common.Helpers
{
    public class AuditDischarge
    {
        public string NameClient { get; set; }
        public string Service { get; set; }
        public string AdmissionDate { get; set; }
        public int Active { get; set; }
    }
}
