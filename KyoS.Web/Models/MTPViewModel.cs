using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class MTPViewModel : MTPEntity
    {
        [Display(Name = "Client")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        public IEnumerable<DiagnosisTempEntity> DiagnosesTemp { get; set; }
    }
}
