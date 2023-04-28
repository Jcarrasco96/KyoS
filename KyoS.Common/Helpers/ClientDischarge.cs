using KyoS.Common.Enums;
using System;
using System.Collections.Generic;

namespace KyoS.Common.Helpers
{
    public class ClientDischarge
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public GenderType Gender { get; set; }
        public string Code { get; set; }
        public DateTime AdmisionDate { get; set; }
        public ServiceType Service { get; set; }
        public StatusType Status { get; set; }
        public string FacilitatorName { get; set; }
    }
}
