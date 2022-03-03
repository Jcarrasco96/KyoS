using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class TCMServicePlanEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int IDCaseManager { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateServicePlan{ get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int CaseNumber { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateIntake { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateAssessment { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateCertification { get; set; }

        public string Strengths { get; set; }

        public string Weakness{ get; set; }

        public string DischargerCriteria { get; set; }

        public bool Active { get; set; }

       // public CaseMannagerEntity CaseManager { get; set; }
        public ClientEntity Client { get; set; }
        public ClinicEntity Clinic { get; set; }
        public List <TCMDomainEntity> TCMDomain { get; set; } 
    }
}
