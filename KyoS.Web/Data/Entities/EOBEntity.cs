using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class EOBEntity
    {

        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateBill { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateBillClose { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateBillPayment { get; set; }
        public decimal Amount { get; set; }
        public int Units { get; set; }
        public StatusBill StatusBill { get; set; }

        public List<Workday_Client> Workday_Clients { get; set; }
        public List<TCMNoteEntity> TCMNotes { get; set; }
        public List<EOBDetailsEntity> EOBDetails { get; set; }

    }
}
