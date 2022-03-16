using KyoS.Common.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace KyoS.Web.Helpers
{
    public interface ICombosHelper
    {
        IEnumerable<SelectListItem> GetComboRoles();
        IEnumerable<SelectListItem> GetComboUserNames();
        IEnumerable<SelectListItem> GetComboUserNamesByRolesClinic(UserType userType, int idClinic);
        IEnumerable<SelectListItem> GetComboDays();
        IEnumerable<SelectListItem> GetComboThemes();
        IEnumerable<SelectListItem> GetComboThemesByClinic(int idClinic);
        IEnumerable<SelectListItem> GetComboThemesByClinic3(int idClinic);
        IEnumerable<SelectListItem> GetComboFacilitators();
        IEnumerable<SelectListItem> GetComboFacilitatorsByClinic(int idClinic, bool blank = false);
        IEnumerable<SelectListItem> GetComboClients();
        IEnumerable<SelectListItem> GetComboClientsByClinic(int idClinic);
        IEnumerable<SelectListItem> GetComboActiveClientsByClinic(int idClinic);
        IEnumerable<SelectListItem> GetComboActiveClientsPSRByClinic(int idClinic);
        IEnumerable<SelectListItem> GetComboClientsForIndNotes(int idClinic, int idWeek, int idFacilitator);
        IEnumerable<SelectListItem> GetComboActivities();
        IEnumerable<SelectListItem> GetComboActivitiesByTheme(int idTheme, int idFacilitator, DateTime date);
        IEnumerable<SelectListItem> GetComboClassifications();
        IEnumerable<SelectListItem> GetComboClinics();
        IEnumerable<SelectListItem> GetComboGender();
        IEnumerable<SelectListItem> GetComboClientStatus();
        IEnumerable<SelectListItem> GetComboGroups();
        IEnumerable<SelectListItem> GetComboGoals(int idMTP);
        IEnumerable<SelectListItem> GetComboObjetives(int idGoal);
        IEnumerable<SelectListItem> GetComboRelationships();
        IEnumerable<SelectListItem> GetComboRaces();
        IEnumerable<SelectListItem> GetComboMaritals();
        IEnumerable<SelectListItem> GetComboEthnicities();
        IEnumerable<SelectListItem> GetComboLanguages();
        IEnumerable<SelectListItem> GetComboReferredsByClinic(string idUser);
        IEnumerable<SelectListItem> GetComboEmergencyContactsByClinic(string idUser);
        IEnumerable<SelectListItem> GetComboDoctorsByClinic(string idUser);
        IEnumerable<SelectListItem> GetComboPsychiatristsByClinic(string idUser);
        IEnumerable<SelectListItem> GetComboLegalGuardiansByClinic(string idUser);
        IEnumerable<SelectListItem> GetComboDiagnosticsByClinic(string idUser);
        IEnumerable<SelectListItem> GetComboDocumentDescriptions();
        IEnumerable<SelectListItem> GetComboIncidentsStatus();
        IEnumerable<SelectListItem> GetComboActiveInsurancesByClinic(int idClinic);
        IEnumerable<SelectListItem> GetComboServices();
        IEnumerable<SelectListItem> GetComboCaseManager();
        IEnumerable<SelectListItem> GetComboCasemannagersByClinic(int idClinic);
        IEnumerable<SelectListItem> GetComboClientsForTCMCaseNotOpen(int idClinic);
        IEnumerable<SelectListItem> GetComboClientsForTCMCaseOpen(int idClinic);
        IEnumerable<SelectListItem> GetComboServicesNotUsed(int idServicePlan);
        IEnumerable<SelectListItem> GetComboStagesNotUsed(string codeDomain);
        IEnumerable<SelectListItem> GetComboServicesPlan(int idClinic);
        IEnumerable<SelectListItem> GetComboTCMServices();
        IEnumerable<SelectListItem> GetComboTCMStages();

    }
}
