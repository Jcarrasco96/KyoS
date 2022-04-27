﻿using KyoS.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class IntakeScreeningEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }

        public string InformationGatheredBy { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateDischarge { get; set; }

        [Display(Name = "Date of Client Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureClient { get; set; }

        [Display(Name = "Date of Witness Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureWitness { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }

        [Display(Name = "Client Is Status")]
        public IntakeClientIsStatus ClientIsStatus { get; set; }

        [Display(Name = "Behavior Is Status")]
        public IntakeBehaviorIsStatus BehaviorIsStatus { get; set; }

        [Display(Name = "Speech Is Status")]
        public IntakeSpeechIsStatus SpeechIsStatus { get; set; }

        public bool DoesClientKnowHisName { get; set; }

        public bool DoesClientKnowTodayDate { get; set; }

        public bool DoesClientKnowWhereIs { get; set; }

        public bool DoesClientKnowTimeOfDay { get; set; }

        public bool EmergencyContact { get; set; }

    }
}
