﻿using KyoS.Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class SupervisorEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Firm")]
        public string Firm { get; set; }

        [Display(Name = "Code")]
        public string Code { get; set; }

        public string LinkedUser { get; set; }
        
        [Display(Name = "Signature")]
        public string SignaturePath { get; set; }

        public ClinicEntity Clinic { get; set; }

        public ICollection<NoteEntity> Notes { get; set; }

        public ICollection<NotePEntity> NotesP { get; set; }

        public ICollection<IndividualNoteEntity> IndividualNotes { get; set; }

        public ICollection<GroupNoteEntity> GroupNotes { get; set; }

        public string RaterEducation { get; set; }

        public string RaterFMHCertification { get; set; }
        public List<ReferralFormEntity> ReferralList { get; set; }

        public StatusType Status { get; set; }
    }
}
