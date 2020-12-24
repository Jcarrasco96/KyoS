using AspNetCore.Reporting;

namespace KyoS.Web.Helpers
{
    public interface IReportService
    {
        byte[] GenerateReportAsync(string reportName);
        RenderType GetRenderType(string reportType);
    }
}