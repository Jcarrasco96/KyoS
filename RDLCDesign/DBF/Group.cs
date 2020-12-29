namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Group
    {
        public int Id { get; set; }

        public bool Am { get; set; }

        public bool Pm { get; set; }

        public int? FacilitatorId { get; set; }
    }
}
