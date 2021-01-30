namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Objetives
    {
        public int Id { get; set; }

        
        public string Objetive { get; set; }


        public string Description { get; set; }


        public DateTime DateOpened { get; set; }


        public DateTime DateTarget { get; set; }

     
        public DateTime DateResolved { get; set; }


        public string Intervention { get; set; }

        public int? GoalId { get; set; }
    }
}
