namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;


    public partial class Diagnosis
    {
        public int Id { get; set; }
        
        public string Code { get; set; }
       
        public string Description { get; set; }

        public int? MTPId { get; set; }
    }
}
