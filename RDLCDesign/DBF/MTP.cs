namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MTP
    {
        public int Id { get; set; }

        public int? ClientId { get; set; }

        
        public DateTime AdmisionDate { get; set; }

        public DateTime MTPDevelopedDate { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string LevelCare { get; set; }

        public string InitialDischargeCriteria { get; set; }

        public string Modality { get; set; }

        public string Frecuency { get; set; }

        public int? NumberOfMonths { get; set; }

        public string Setting { get; set; }
    }
}
