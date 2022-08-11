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

        public string Outcome { get; set; }

        public string NextStep { get; set; }

        public TCMClientEntity TCMClient { get; set; }

        public CaseMannagerEntity CaseManager { get; set; }

        public List<TCMNoteActivityEntity> TCMNoteActivity { get; set; }
    }
}
