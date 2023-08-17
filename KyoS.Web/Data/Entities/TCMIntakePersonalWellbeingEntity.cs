using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakePersonalWellbeingEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        public int TcmClient_FK { get; set; }

        [Range(1, 10, ErrorMessage = "Can only be between 1 .. 15")]
        //[StringLength(2, ErrorMessage = "Max 2 digits")]
        public int Living { get; set; }

        [Range(1, 10, ErrorMessage = "Can only be between 1 .. 10")]
        //[StringLength(2, ErrorMessage = "Max 2 digits")]
        public int Health { get; set; }

        [Range(1, 10, ErrorMessage = "Can only be between 1 .. 10")]
        //[StringLength(2, ErrorMessage = "Max 2 digits")] 
        public int Life { get; set; }

        [Range(1, 10, ErrorMessage = "Can only be between 1 .. 10")]
        //[StringLength(2, ErrorMessage = "Max 2 digits")]
        public int Relationships { get; set; }

        [Range(1, 10, ErrorMessage = "Can only be between 1 .. 10")]
        //[StringLength(2, ErrorMessage = "Max 2 digits")]
        public int Feel { get; set; }

        [Range(1, 10, ErrorMessage = "Can only be between 1 .. 10")]
        //[StringLength(2, ErrorMessage = "Max 2 digits")]
        public int Community { get; set; }

        [Range(1, 10, ErrorMessage = "Can only be between 1 .. 10")]
        //[StringLength(2, ErrorMessage = "Max 2 digits")]
        public int Security { get; set; }

        [Range(1, 10, ErrorMessage = "Can only be between 1 .. 10")]
        //[StringLength(2, ErrorMessage = "Max 2 digits")]
        public int Religion { get; set; }

        public string AdmissionedFor { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }
    }
}
