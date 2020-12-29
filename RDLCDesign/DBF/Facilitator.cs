namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Facilitator
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Codigo { get; set; }

        public int? ClinicId { get; set; }
    }
}
