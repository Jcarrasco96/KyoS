using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class IncidentReportEntity : AuditableEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        [Display(Name = "Date of Employer Signaturee")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }

        public FacilitatorEntity Facilitator { get; set; }

        public SupervisorEntity Supervisor { get; set; }

        public DocumentsAssistantEntity DocumentAssisstant { get; set; }

        [Display(Name = "Date of Incident")]
        [DataType(DataType.Date)]

        public DateTime DateIncident { get; set; }

        [Display(Name = "Date of Report")]
        [DataType(DataType.Date)]

        public DateTime DateReport { get; set; }

        [Display(Name = "Time of Incident")]
        [DataType(DataType.Time)]

        public DateTime TimeIncident { get; set; }

        public string Location { get; set; }
        public string DescriptionIncident { get; set; }
        public bool Injured { get; set; }
        public string Injured_Description { get; set; }

        public bool Witnesses { get; set; }
        public string Witnesses_Contact { get; set; }

        public string AdmissionFor { get; set; }
    }
}
