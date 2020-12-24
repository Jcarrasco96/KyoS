using AspNetCore.Reporting;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public class ReportService : IReportService
    {
        private readonly DataContext _context;        

        public ReportService(DataContext context)
        {
            _context = context;            
        }
        public byte[] GenerateReportAsync(string reportName)
        {
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Report\\Groups\\{1}.rdlc", fileDirPath, reportName);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            System.Text.Encoding.GetEncoding("windows-1252");
            LocalReport report = new LocalReport(rdlcFilePath);

            GroupEntity groupEntity = _context.Groups.Find(1);

            report.AddDataSource("DataSet1", groupEntity);
            var result = report.Execute(GetRenderType("pdf"), 1, parameters);
            return result.MainStream;
        }

        public RenderType GetRenderType(string reportType)
        {
            var renderType = RenderType.Pdf;
            switch (reportType.ToLower())
            {
                default:
                case "pdf":
                    renderType = RenderType.Pdf;
                    break;
                case "word":
                    renderType = RenderType.Word;
                    break;
                case "excel":
                    renderType = RenderType.Excel;
                    break;
            }

            return renderType;
        }
    }
}
