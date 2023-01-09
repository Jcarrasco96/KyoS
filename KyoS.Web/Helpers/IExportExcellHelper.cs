using KyoS.Web.Data.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface IExportExcellHelper
    {

        #region Bill for week
        byte[] ExportFacilitatorHelper(List<FacilitatorEntity> aFacilitator);
        byte[] ExportBillForWeekHelper(List<Workday_Client> workday_Client, string Periodo, string ClinicName, string data);

        #endregion

        #region Utils functions
        byte[] ConvertStreamToByteArray(Stream stream);
        #endregion

    }
}
