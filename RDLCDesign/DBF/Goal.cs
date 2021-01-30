namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Goal
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string AreaOfFocus { get; set; }

        public int? MTPId { get; set; }

        public int Number { get; set; }
    }
}
