using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakeClientSignatureVerificationEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        public int TcmClient_FK { get; set; }

        public string AdmissionedFor { get; set; }

        [Display(Name = "Date of Legal Guardian Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureLegalGuardianOrClient { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }
    }
}
