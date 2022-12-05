using KyoS.Common.Enums;
using System;
using System.Collections.Generic;

namespace KyoS.Common.Helpers
{
    public class AuditGroups
    {
        public string Date { get; set; }
        public string Facilitator { get; set; }
        public string Session { get; set; }
        public int Count { get; set; }
        public int Active { get; set; }
    }
}
