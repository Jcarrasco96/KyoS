using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakeCoordinationCareEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        public int TcmClient_FK { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]

        public DateTime Date { get; set; }

        public string AdmissionedFor { get; set; }

        [Display(Name = "Date of Legal Guardian Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureLegalGuardian { get; set; }

        [Display(Name = "Date of Person Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignaturePerson { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }

        public bool Documents { get; set; }

        public bool InformationToRelease { get; set; }
        public bool InformationTorequested { get; set; }

        public bool PCP { get; set; }
        public bool Specialist { get; set; }
        public string SpecialistText { get; set; }

        public bool InformationVerbal { get; set; }
        public bool InformationWrited { get; set; }
        public bool InformationFascimile { get; set; }
        public bool InformationElectronic { get; set; }
        public bool InformationAllBefore { get; set; }
        public bool InformationNonKnown { get; set; }

        public bool IAuthorize { get; set; }
        public bool IRefuse { get; set; }

    }
}
