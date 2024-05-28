using KyoS.Web.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMNoteUpdatePaidViewModel
    {
        public int IdTCMNote { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DatePaid { get; set; }

        public string DateInterval { get; set; }
        public int IdCaseManager { get; set; }
        public int IdClient { get; set; }
        public int Billed { get; set; }
    }
}
