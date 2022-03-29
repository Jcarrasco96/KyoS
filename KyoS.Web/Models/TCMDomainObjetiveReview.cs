using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using KyoS.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace KyoS.Web.Models
{
    public class TCMDomainObjetiveReview
    {
        public int ID { get; set; }
        public int Status { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Updates_Changes { get; set; }
        public string Addendum_ServicePlan { get; set; }
        public List<TCMObjetiveReview> ObjectiveList { get; set; }
    }
}