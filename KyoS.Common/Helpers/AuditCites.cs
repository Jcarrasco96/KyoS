using KyoS.Common.Enums;
using System;
using System.Collections.Generic;

namespace KyoS.Common.Helpers
{
    public class AuditCites
    {
        public DateTime Date { get; set; }
        public string ClientName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Active { get; set; }
    }
}
