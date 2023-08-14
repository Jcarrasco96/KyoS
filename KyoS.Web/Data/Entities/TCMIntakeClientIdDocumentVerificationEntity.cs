using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakeClientIdDocumentVerificationEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        public int TcmClient_FK { get; set; }

        public string Id_DriverLicense { get; set; }

        public string Social { get; set; }

        public string MedicaidId { get; set; }

        public string MedicareCard { get; set; }

        public string HealthPlan { get; set; }

        public string Passport_Resident { get; set; }
        
        public string Other_Name { get; set; }
        
        public string Other_Identification { get; set; }

        public string AdmissionedFor { get; set; }

        [Display(Name = "Date of Legal Guardian Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureLegalGuardianOrClient { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }
    }
}
