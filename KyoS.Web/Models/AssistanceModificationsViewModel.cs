using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class AssistanceModificationsViewModel
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Workdays { get; set; }
    }
}
