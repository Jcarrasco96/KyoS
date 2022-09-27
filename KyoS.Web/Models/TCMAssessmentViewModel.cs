using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMAssessmentViewModel : TCMAssessmentEntity
    {

        public List<Client_Referred> Client_Referred_List { get; set; }
    }
}
