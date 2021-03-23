namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Note
    {
        public int Id { get; set; }
                
        public string PlanNote { get; set; }

        public int Status { get; set; }

        public int Workday_Client_FK { get; set; }

        public bool Adequate { get; set; }

        public bool AdequateAC { get; set; }

        public bool Anxious { get; set; }

        public bool Congruent { get; set; }

        public bool Depressed { get; set; }

        public bool Dramatized { get; set; }

        public bool Euphoric { get; set; }

        public bool Euthymic { get; set; }

        public bool Fair { get; set; }

        public bool Faulty { get; set; }

        public bool Guarded { get; set; }

        public bool Hostile { get; set; }

        public bool Impaired { get; set; }

        public bool Inadequate { get; set; }

        public bool Irritable { get; set; }

        public bool Limited { get; set; }

        public bool MildlyImpaired { get; set; }

        public bool Motivated { get; set; }

        public bool Negativistic { get; set; }

        public bool Normal { get; set; }

        public bool NotPerson { get; set; }

        public bool NotPlace { get; set; }

        public bool NotTime { get; set; }

        public bool Optimistic { get; set; }

        public bool OrientedX3 { get; set; }

        public bool Present { get; set; }

        public bool SeverelyImpaired { get; set; }

        public bool ShortSpanned { get; set; }

        public int? SupervisorId { get; set; }

        public bool Unmotivated { get; set; }

        public bool Withdrawn { get; set; }

        public bool SignificantProgress { get; set; }

        public bool ModerateProgress { get; set; }

        public bool MinimalProgress { get; set; }

        public bool NoProgress { get; set; }

        public bool Regression { get; set; }

        public bool Decompensating { get; set; }

        public bool UnableToDetermine { get; set; }
    }
}
