namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Supervisor
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Firm { get; set; }

        public string Code { get; set; }

        public int? ClinicId { get; set; }

        public string LinkedUser { get; set; }
    }
}
