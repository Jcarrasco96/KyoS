using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMNoteActivityEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMNoteEntity TCMNote { get; set; }

        public string Setting { get; set; }

        public TCMDomainEntity TCMDomain { get; set; }

        public string DescriptionOfService { get; set; }

        [DataType(DataType.Time)]
        public DateTime? StartTime { get; set; }

        [DataType(DataType.Time)]
        public DateTime? EndTime { get; set; }

        public int Minutes { get; set; }

    }
}
