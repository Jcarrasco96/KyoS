using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMNoteEntity : AuditableEntity
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Service")]
        public DateTime DateOfService { get; set; }

        [Display(Name = "Service Code:")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ServiceCode { get; set; }

        public NoteStatus Status { get; set; }

        [Display(Name = "Outcome")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Outcome { get; set; }

        [Display(Name = "Next Step")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string NextStep { get; set; }

        public TCMClientEntity TCMClient { get; set; }

       // public CaseMannagerEntity CaseManager { get; set; }

        public List<TCMNoteActivityEntity> TCMNoteActivity { get; set; }

        public IEnumerable<TCMMessageEntity> TCMMessages { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BilledDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        public bool DeniedBill { get; set; }

        public string CodeBill { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ApprovedDate { get; set; }

        public bool Sign { get; set; }

        public BillDmsEntity BillDms { get; set; }
        public TCMPayStubEntity PayStub { get; set; }

    }
}
