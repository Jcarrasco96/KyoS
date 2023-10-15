using System;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;
using KyoS.Common.Enums;
using System.Collections.Generic;

namespace KyoS.Web.Data.Entities
{
    public class DischargeEntity : AuditableEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }

        [Display(Name = "Date of Report")]
        [DataType(DataType.Date)]
        public DateTime DateReport { get; set; }

        [Display(Name = "Date of Discharge")]
        [DataType(DataType.Date)]
        public DateTime DateDischarge { get; set; }

        public bool ProgramPSR { get; set; }
        public bool ProgramInd { get; set; }
        public bool ProgramGroup { get; set; }
        public bool ProgramClubHouse { get; set; }

        public bool Planned { get; set; }

        public bool CompletedTreatment { get; set; }
        public bool Termination { get; set; }
        public bool ClientTransferred { get; set; }
        public bool NonCompliant { get; set; }
        public bool LeftBefore { get; set; }
        public bool Administrative { get; set; }
        public bool Other { get; set; }
        public string Other_Explain { get; set; }

        public bool PrognosisGood { get; set; }
        public bool PrognosisFair { get; set; }
        public bool PrognosisGuarded { get; set; }
        public bool PrognosisPoor { get; set; }

        public bool ClinicalStable { get; set; }
        public bool ClinicalUnstable { get; set; }
        public bool ClinicalUnpredictable { get; set; }
        public bool ClinicalCoherente { get; set; }
        public bool ClinicalIncoherente { get; set; }
        public bool ClinicalInRemission { get; set; }

        public string ReferralsTo { get; set; }

        public string DischargeDiagnosis{ get; set; }

        public string AdmissionedFor { get; set; }

        [Display(Name = "Date of Client Signature")]
        [DataType(DataType.Date)]
        public DateTime DateSignaturePerson { get; set; }

        [Display(Name = "Date of Staff Signature")]
        [DataType(DataType.Date)]
        public DateTime DateSignatureEmployee { get; set; }

        [Display(Name = "Date of Supervisor Signature")]
        [DataType(DataType.Date)]
        public DateTime DateSignatureSupervisor { get; set; }

        public SupervisorEntity Supervisor { get; set; }

        public DischargeStatus Status { get; set; }

        public ServiceType TypeService { get; set; }

        public IEnumerable<MessageEntity> Messages { get; set; }

        [Display(Name = "Date of Admission Service")]
        [DataType(DataType.Date)]
        public DateTime DateAdmissionService { get; set; }

        //Join Commission
        public bool JoinCommission { get; set; }
        //1 Admission
        public string SummaryOfPresentingProblems { get; set; }
        public string TreatmentSummary { get; set; }
        //2 Progress
        public bool SignificantProgress { get; set; }
        public bool MinimalProgress { get; set; }
        public bool Regression { get; set; }
        public bool ModerateProgress { get; set; }
        public bool NoProgress { get; set; }
        public bool UnableToDetermine { get; set; }

        // 3 Discharge
        //public bool PlanCompletely { get; set; }  -- Code Line 32 (CompletedTreatment field)
        public bool PlanCompletePartially { get; set; }
        //public bool VoluntarilyRefused { get; set; }  --code line 36(LeftBefore field)
        //public bool NonComplianceRules { get; set; }  --code line 35(NonCompliant field)
        public bool ClientMoveOutArea { get; set; }
        //public bool ClientTransferred { get; set; }  --code line 34(ClientTransferred field)
        public bool ExtendedHospitalization { get; set; }
        //public bool ServiceNotCovered { get; set; }  --code line 33(Termination field)
        //public bool Other { get; set; }  --code line 38(Other field)
        //public string OtherExplain { get; set; }  --code line 39(Other field)

        // 4 Follow up
        public string Follow_up { get; set; }


    }
}
