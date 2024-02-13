using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class TCMNoteActivityTempEntity
    {
        public int Id { get; set; }

        public int IdTCMClient { get; set; }

        public int IdSetting { get; set; }

        public string Setting { get; set; }

        public int IdTCMDomain { get; set; }

        public string TCMDomainCode { get; set; }

        public string DescriptionOfService { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime StartTime { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime EndTime { get; set; }

        public DateTime DateOfServiceOfNote { get; set; }

        public int Minutes { get; set; }

        public string UserName { get; set; }

        public int IdTCMServiceActivity { get; set; }

        public string ServiceName { get; set; }
        public bool Billable { get; set; }
    }
}
