using KyoS.Web.Data.Contracts;
using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace KyoS.Web.Data.Entities
{
    public class SettingEntity : AuditableEntity
    {
        public int Id { get; set; }
        public int Clinic_FK { get; set; }
        public ClinicEntity Clinic { get; set; }
        public bool AvailableCreateNewWorkdays { get; set; }
        public bool MentalHealthClinic { get; set; }
        public bool TCMClinic { get; set; }
        public bool MHClassificationOfGoals { get; set; }
        public bool MHProblems { get; set; }
        public bool TCMSupervisorEdit { get; set; }
        public bool BillSemanalMH { get; set; }
        public bool IndNoteForAppointment { get; set; }

        [DataType(DataType.Time)]
        public DateTime TCMInitialTime { get; set; }

        [DataType(DataType.Time)]
        public DateTime TCMEndTime { get; set; }
        public bool LockTCMNoteForUnits { get; set; }

        public int UnitsForDayForClient { get; set; }
    }
}
