namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Workday_Client
    {
        public int Id { get; set; }

        public int? WorkdayId { get; set; }

        public int? ClientId { get; set; }

        public bool Present { get; set; }

        public string Session { get; set; }
    }
}
