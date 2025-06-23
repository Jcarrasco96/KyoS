using KyoS.Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class GroupEntity
    {
        public int Id { get; set; }
        [Display(Name = "AM")]
        public bool Am { get; set; }
        [Display(Name = "PM")]
        public bool Pm { get; set; }
        public ServiceType Service { get; set; }
        public FacilitatorEntity Facilitator { get; set; }
        public IEnumerable<ClientEntity> Clients { get; set; }
        
        public string Meridian 
        {
            get
            {
                return (this.Am) ? "AM" : "PM";
            } 
        }
        public bool SharedSession { get; set; }
        public ScheduleEntity Schedule { get; set; }

        public string Name { get; set; }
    }
}
