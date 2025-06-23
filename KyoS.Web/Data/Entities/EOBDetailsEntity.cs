using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class EOBDetailsEntity
    {
        public int Id { get; set; }
        public EOBEntity EOB { get; set; }
        public int IdCLient { get; set; }
        public int IdWorkddayClient { get; set; }
        public int IdTCMNotes { get; set; }
        public ServiceAgency ServiceAgency { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateService { get; set; }

        public int Unit { get; set; }

        public StatusBill StatusBill { get; set; }
        [DataType(DataType.Date)]
        public DateTime PaidDate { get; set; }

        public string NameClient { get; set; }
        public decimal Amount { get; set; }
        public bool MedicaidBill { get; set; }
    }
}
