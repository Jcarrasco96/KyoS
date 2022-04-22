using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class IntakeConsentForTreatmentEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }

        [Display(Name = "Date of Legal Guardian Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureLegalGuardian { get; set; }

        [Display(Name = "Date of Person Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignaturePerson { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]
        
        public DateTime DateSignatureEmployee { get; set; }

        public string AdmissionedFor { get; set; }

        public bool AuthorizeStaff { get; set; }
        public bool AuthorizeRelease { get; set; }
        public bool Underestand { get; set; }
        public bool Aggre { get; set; }
        public bool Aggre1 { get; set; }
        public bool Certify { get; set; }
        public bool Certify1 { get; set; }
        public bool Documents { get; set; }

    }
}
