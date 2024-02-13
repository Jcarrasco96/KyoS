using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class TCMNotePendingByPayStubViewModel
    {
        public int Id { get; set; }
        public int AmountTCMNotes { get; set; }
        public int AmountCMHNotes { get; set; }
        public int Units { get; set; }
        public decimal Amount { get; set; }

        [DataType(DataType.Date)]
        public DateTime DatePayStubClose { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Filtro")]
        public int IdFiltro { get; set; }
        public IEnumerable<SelectListItem> FiltroPayStubList { get; set; }
        public int CantTCM { get; set; }

        [DataType(DataType.Date)]
        public DateTime DatePayStub { get; set; }

        public List<TCMNoteEntity> TCMNoteList { get; set; }
        [DataType(DataType.Date)]
        public DateTime DatePayStubPayment { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        public int IdStatus { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }

        public int IdCaseManager { get; set; }
        public List<TCMPayStubDetailsEntity> TCMPaystubDetails { get; set; }
    }
}
