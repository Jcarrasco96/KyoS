using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class MTPReviewViewModel : MTPReviewEntity
    {
        public int IdMTP { get; set; }
        public int Origin { get; set; }

        public IEnumerable<GoalsTempEntity> GoalTempList { get; set; }
       
        public int IdDocumentAssistant { get; set; }
        public int IdIndFacilitator { get; set; }
        public int IdFacilitator { get; set; }
    }
}
