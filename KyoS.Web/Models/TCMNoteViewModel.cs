using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMNoteViewModel : TCMNoteEntity
    {
        public int IdCaseManager { get; set; }
        public int IdTCMClient { get; set; }
        public int IdTCMNote { get; set; }
        public int IdTCMWorday { get; set; }

        public int TotalMinutes { get; set; }
        public int TotalUnits { get; set; }
        //este campo lo uso para saber de que pagina se viene
        public int Origin { get; set; }

    }
}
