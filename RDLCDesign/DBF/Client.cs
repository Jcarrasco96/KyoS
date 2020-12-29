namespace RDLCDesign.DBF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Client
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Gender { get; set; }

        public string Code { get; set; }

        public int? ClinicId { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string MedicalID { get; set; }

        public int Status { get; set; }

        public int? GroupId { get; set; }
    }
}
