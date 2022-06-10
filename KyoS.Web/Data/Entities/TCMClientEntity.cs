using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class TCMClientEntity
    {
        public int Id { get; set; }
       
        public CaseMannagerEntity Casemanager { get; set; }
        public ClientEntity Client { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string CaseNumber { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [DataType(DataType.Date)]
        public DateTime DataOpen { get; set; }

        [DataType(DataType.Date)]
        public DateTime DataClose { get; set; }
        
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int Period { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public StatusType Status { get; set; }

        public TCMIntakeFormEntity TCMIntakeForm { get; set; }

        public TCMIntakeConsentForTreatmentEntity TcmIntakeConsentForTreatment { get; set; }

        public List<TCMIntakeConsentForReleaseEntity> TcmIntakeConsentForRelease { get; set; }

        public TCMIntakeConsumerRightsEntity TcmIntakeConsumerRights { get; set; }

        public TCMIntakeAcknowledgementHippaEntity TcmIntakeAcknowledgementHipa { get; set; }

        public TCMIntakeOrientationChecklistEntity TCMIntakeOrientationChecklist { get; set; }

        public TCMIntakeAdvancedDirectiveEntity TCMIntakeAdvancedDirective { get; set; }

        public TCMIntakeForeignLanguageEntity TCMIntakeForeignLanguage { get; set; }

        public TCMIntakeWelcomeEntity TCMIntakeWelcome { get; set; }

        public TCMIntakeNonClinicalLogEntity TCMIntakeNonClinicalLog { get; set; }

        public TCMIntakeMiniMentalEntity TCMIntakeMiniMental { get; set; }
    }
}
