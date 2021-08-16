using System;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class IncidentEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Description { get; set; }
        public DateTime? SolvedDate { get; set; }
        public string SolvedBy { get; set; }
        public IncidentsStatus Status { get; set; }
        public UserEntity UserCreatedBy { get; set; }
    }
}
