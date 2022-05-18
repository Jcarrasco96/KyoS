using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class MTPEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }        

        [Display(Name = "Treatment plan developed date")]
        [DataType(DataType.Date)]
        
        public DateTime MTPDevelopedDate { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime EndTime { get; set; }

        [Display(Name = "Level of care")]
        public string LevelCare { get; set; }

        [Display(Name = "Initial discharge criteria")]
        public string InitialDischargeCriteria { get; set; }

        public IEnumerable<GoalEntity> Goals { get; set; }

        [Display(Name = "Modality")]
        public string Modality { get; set; }

        [Display(Name = "Frecuency")]
        public string Frecuency { get; set; }

        [Display(Name = "Months of treatment")]
        public int? NumberOfMonths { get; set; }

        public string Setting { get; set; }

        public bool Active { get; set; }

        public List<AdendumEntity> AdendumList { get; set; }

        public List<MTPReviewEntity> MtpReviewList { get; set; }
    }
}
