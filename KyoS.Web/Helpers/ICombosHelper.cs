using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace KyoS.Web.Helpers
{
    public interface ICombosHelper
    {
        IEnumerable<SelectListItem> GetComboRoles();
        IEnumerable<SelectListItem> GetComboDays();
        IEnumerable<SelectListItem> GetComboThemes();

        IEnumerable<SelectListItem> GetComboFacilitators();
        IEnumerable<SelectListItem> GetComboClients();
        IEnumerable<SelectListItem> GetComboActivities();
        IEnumerable<SelectListItem> GetComboClassifications();
    }
}
