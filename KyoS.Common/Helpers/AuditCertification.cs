using KyoS.Common.Enums;
using System;
using System.Collections.Generic;

namespace KyoS.Common.Helpers
{
    public class AuditCertification
    {
        public string TCMName { get; set; }
        public string CourseName { get; set; }
        public string ExpirationDate { get; set; }
        public int Active { get; set; }
        public string Description { get; set; }
    }
}
