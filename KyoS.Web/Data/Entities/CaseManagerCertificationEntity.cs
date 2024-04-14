using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class CaseManagerCertificationEntity : AuditableEntity
    {
        public int Id { get; set; }

        public CaseMannagerEntity TCM { get; set; }
       
        public CourseEntity Course { get; set; }

        public string Name { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Certificate Date")]
        [DataType(DataType.Date)]
        public DateTime CertificateDate { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Expiration Date")]
        [DataType(DataType.Date)]
        public DateTime ExpirationDate { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Certification Number")]
        public string CertificationNumber { get; set; }
    }
}
