using KyoS.Web.Data.Entities;
using KyoS.Web.Models;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface IConverterHelper
    {
        ClinicEntity ToClinicEntity(ClinicViewModel model, string path, bool isNew);
        ClinicViewModel ToClinicViewModel(ClinicEntity model);
        ThemeEntity ToThemeEntity(ThemeViewModel model, bool isNew);
        ThemeViewModel ToThemeViewModel(ThemeEntity model);
        Task<ActivityEntity> ToActivityEntity(ActivityViewModel model, bool isNew);
        ActivityViewModel ToActivityViewModel(ActivityEntity model);
    }
}
