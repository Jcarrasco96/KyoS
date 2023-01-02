using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class DeleteViewModel
    {
        public int Id_Element { get; set; }
        public string Desciption { get; set; }
        
    }
}
