using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class PayStubEntity
    {

        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime DatePayStub { get; set; }
        [DataType(DataType.Date)]
        public DateTime DatePayStubClose { get; set; }
        [DataType(DataType.Date)]
        public DateTime DatePayStubPayment { get; set; }
        public decimal Amount { get; set; }
        public int Units { get; set; }
        public StatusTCMPaystub StatusPayStub { get; set; }

        public List<Workday_Client> WordayClients { get; set; }
        public List<BioEntity> Bios { get; set; }
        public List<BriefEntity> Brief { get; set; }
        public List<MTPEntity> MTPs { get; set; }
        public List<FarsFormEntity> Fars { get; set; }
        public List<IntakeMedicalHistoryEntity> MedicalHistory { get; set; }
        public List<PayStubDetailsEntity> PayStubDetails { get; set; }
        public UserType Role { get; set; }

        public FacilitatorEntity Facilitator { get; set; }
        public DocumentsAssistantEntity Doc_Assisstant { get; set; }

        public int CantClient { get; set; }
    }
}
