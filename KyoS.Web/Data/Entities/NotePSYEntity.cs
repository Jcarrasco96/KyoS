using KyoS.Common.Enums;
using KyoS.Web.Data.Abstract;
using KyoS.Web.Data.Contracts;
using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class NotePSYEntity : AuditableEntity
    {
        public int Id { get; set; }

        public string NamePSY { get; set; }
        public string Description { get; set; }
        
        public ClientEntity Client { get; set; }

        [Display(Name = "Date of Service")]
        [DataType(DataType.Date)]
        public DateTime DateService { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime InitialTime { get; set; }

        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime EndTime { get; set; }
    }
}
