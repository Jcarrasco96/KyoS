using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMPayStubDetailsEntity
    {
        public int Id { get; set; }
        public TCMPayStubEntity Bill { get; set; }
        public int IdCaseManager { get; set; }
        public int IdTCMNotes { get; set; }
      
        [DataType(DataType.Date)]
        public DateTime DateService { get; set; }

        public int Unit { get; set; }
        public decimal Amount { get; set; }

    }
}
