using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class PayStubDetailsEntity
    {
        public int Id { get; set; }
        public PayStubEntity TCMPayStub { get; set; }
        public int IdDocuAssisstant { get; set; }
        public int IdWorkdayClient { get; set; }
        public int IdFacilitator { get; set; }
      
        [DataType(DataType.Date)]
        public DateTime DateService { get; set; }

        public int Unit { get; set; }
        public decimal Amount { get; set; }
        public string ClientName { get; set; }
        public string DocumentName { get; set; }

    }
}
