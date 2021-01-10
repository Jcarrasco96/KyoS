using KyoS.Common.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace KyoS.Web.Helpers
{
    public interface ICombosHelper
    {
        IEnumerable<SelectListItem> GetComboRoles();
        IEnumerable<SelectListItem> GetComboUserNamesByRolesClinic(UserType userType, int idClinic);
        IEnumerable<SelectListItem> GetComboDays();
        IEnumerable<SelectListItem> GetComboThemes();
        IEnumerable<SelectListItem> GetComboThemesByClinic(int idClinic);
        IEnumerable<SelectListItem> GetComboFacilitators();
        IEnumerable<SelectListItem> GetComboFacilitatorsByClinic(int idClinic);
        IEnumerable<SelectListItem> GetComboClients();
        IEnumerable<SelectListItem> GetComboClientsByClinic(int idClinic);
        IEnumerable<SelectListItem> GetComboActivities();
        IEnumerable<SelectListItem> GetComboActivitiesByTheme(int idTheme);
        IEnumerable<SelectListItem> GetComboClassifications();
        IEnumerable<SelectListItem> GetComboClinics();
        IEnumerable<SelectListItem> GetComboGender();
        IEnumerable<SelectListItem> GetComboClientStatus();
        IEnumerable<SelectListItem> GetComboGroups();
    }
}
