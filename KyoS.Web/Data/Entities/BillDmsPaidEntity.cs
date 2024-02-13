using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class BillDmsPaidEntity
    {
        public int Id { get; set; }
        public BillDmsEntity Bill { get; set; }
        [DataType(DataType.Date)]
        public DateTime DatePaid { get; set; }
        public decimal Amount { get; set; }
        public PaidOrigi OrigePaid { get; set; }

    }
    
}
