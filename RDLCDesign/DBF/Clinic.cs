namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Clinic
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string LogoPath { get; set; }
    }
}
