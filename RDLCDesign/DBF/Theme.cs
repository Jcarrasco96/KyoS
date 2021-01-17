namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Theme
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Day { get; set; }

        public int? ClinicId { get; set; }
    }
}
