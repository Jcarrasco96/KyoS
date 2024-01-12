using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMPayStubEntity
    {

        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime DatePayStub { get; set; }
        [DataType(DataType.Date)]
        public DateTime DatePayStubClose { get; set; }
        [DataType(DataType.Date)]
        public DateTime DatePayStubPayment { get; set; }
        public decimal Amount { get; set; }
        public int Units { get; set; }
        public StatusTCMPaystub StatusPayStub { get; set; }

        public CaseMannagerEntity CaseMannager { get; set; }
        public List<TCMNoteEntity> TCMNotes { get; set; }
        public List<TCMPayStubDetailsEntity> TCMPayStubDetails { get; set; }

      
    }
}
