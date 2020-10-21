using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class ActivityViewModel : ActivityEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Theme")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a theme.")]
        public int IdTheme { get; set; }
        public IEnumerable<SelectListItem> Themes { get; set; }
    }
}
