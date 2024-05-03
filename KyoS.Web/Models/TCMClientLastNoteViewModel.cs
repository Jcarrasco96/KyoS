using KyoS.Common.Enums;
using KyoS.Web.Data.Entities;
using System;
using System.Collections.Generic;

namespace KyoS.Web.Models
{
    public class TCMClientLastNoteViewModel
    {
        public string CaseManager { get; set; }
        public string ClientName { get; set; }
        public string TCMCaseNumber { get; set; }
        public string DateOpen { get; set; }
        public string DateClose { get; set; }
        public string LastServiceDate { get; set; }
        public int Days { get; set; }
        public StatusType Status { get; set; }
        public GenderType Gender { get; set; }
        public string LastServiceName { get; set; }

    }
}
