using KyoS.Web.Data;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using AspNetCore.Reporting;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;
using KyoS.Common.Helpers;

namespace KyoS.Web.Helpers
{
    public class ReportHelper : IReportHelper
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webhostEnvironment;
        private readonly IImageHelper _imageHelper;
        public ReportHelper(DataContext context, IWebHostEnvironment webHostEnvironment, IImageHelper imageHelper)
        {
            _context = context;
            _webhostEnvironment = webHostEnvironment;
            _imageHelper = imageHelper;
        }

        public async Task<byte[]> GroupAsyncReport(int id)
        {
            string mimetype = "";
            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\Groups\\rptGroup.rdlc";            
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            System.Text.Encoding.GetEncoding("windows-1252");
            
            LocalReport report = new LocalReport(rdlcFilePath);

            GroupEntity groupEntity = await _context.Groups
                                                    .Include(f => f.Facilitator)
                                                    .ThenInclude(c => c.Clinic).FirstOrDefaultAsync(g => g.Id == id);

            List<ClinicEntity> clinics = new List<ClinicEntity> { groupEntity.Facilitator.Clinic };
            List<GroupEntity> groups = new List<GroupEntity> { groupEntity };
            List<ClientEntity> clients = _context.Clients.Where(c => c.Group.Id == groupEntity.Id).ToList();
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { groupEntity.Facilitator };

            report.AddDataSource("dsGroups", groups);
            report.AddDataSource("dsClinics", clinics);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsClients", clients);

            /*var sesion = (groupEntity.Am) ? "Session: AM" : "Session: PM";
            parameters.Add("sesion", sesion);*/

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return result.MainStream;
        }

        public async Task<byte[]> DailyAssistanceAsyncReport(List<Workday_Client> workdayClientList)
        {
            List<Workday_Client> am_list = workdayClientList.Where(wc => wc.Session == "AM").ToList();
            List<Workday_Client> pm_list = workdayClientList.Where(wc => wc.Session == "PM").ToList();

            //report
            string mimetype = "";
            int extension = 1;           
            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\Generics\\rptDailyAssistance.rdlc";            
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            System.Text.Encoding.GetEncoding("windows-1252");

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClientList.First().Facilitator.Clinic.LogoPath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClientList.First().Facilitator.Clinic.LogoPath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }

            var date = workdayClientList.First().Workday.Date.ToShortDateString();
            var additionalComments1 = string.Empty;
            var additionalComments2 = string.Empty;
            foreach (Workday_Client item in am_list)
            {
                if (item != null)
                {
                    if (!item.Present)
                    {
                        if (additionalComments1 != string.Empty)
                            additionalComments1 = $"{additionalComments1}\n{item.ClientName} - {item.CauseOfNotPresent}";
                        else
                            additionalComments1 = $"{item.ClientName} - {item.CauseOfNotPresent}";
                    }
                }
            }
            foreach (Workday_Client item in pm_list)
            {
                if (item != null)
                {
                    if (!item.Present)
                    {
                        if (additionalComments2 != string.Empty)
                            additionalComments2 = $"{additionalComments2}\n{item.ClientName} - {item.CauseOfNotPresent}";
                        else
                            additionalComments2 = $"{item.ClientName} - {item.CauseOfNotPresent}";
                    }
                }
            }

            //datasource            
            List<ClinicEntity> clinics = new List<ClinicEntity> { workdayClientList.First().Facilitator.Clinic };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClientList.First().Facilitator };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };
            List<ParametersOfDailyAssistance> parametersList = new List<ParametersOfDailyAssistance> { new ParametersOfDailyAssistance 
                                                                                                         {
                                                                                                          date = date,
                                                                                                          session1 = "AM",
                                                                                                          session2 = "PM",
                                                                                                          AdditionalComments1 = additionalComments1,
                                                                                                          AdditionalComments2 = additionalComments2
                                                                                                          } 
                                                                                                      };
            int to = 14 - am_list.Count();
            for (int i = 0; i < to; i++)
            {
                am_list.Add(null);
            }
            to = 14 - pm_list.Count();
            for (int k = 0; k < to; k++)
            {
                pm_list.Add(null);
            }
            report.AddDataSource("dsWorkdays_Clients1", am_list);
            report.AddDataSource("dsWorkdays_Clients2", pm_list);
            report.AddDataSource("dsClinics", clinics);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsImages", images);
            report.AddDataSource("dsParameters", parametersList);          

            var result = report.Execute(RenderType.Pdf, extension, parameters, mimetype);            
            return result.MainStream;
        }

        public async Task<byte[]> LarkinAbsenceNoteAsyncReport(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\Notes\\rptAbsenceNoteLARKINBEHAVIOR.rdlc";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            System.Text.Encoding.GetEncoding("windows-1252");

            LocalReport report = new LocalReport(rdlcFilePath);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<ParametersOfAbsenceNoteLarkin> parametersList = new List<ParametersOfAbsenceNoteLarkin> { new ParametersOfAbsenceNoteLarkin { date = date, dateFacilitator = dateFacilitator } };

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", null);
            report.AddDataSource("dsParameters", parametersList);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);            
            return result.MainStream;
        }

        public async Task<byte[]> SolAndVidaAbsenceNoteAsyncReport(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";            
            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\Notes\\rptAbsenceNoteSolAndVida.rdlc";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            System.Text.Encoding.GetEncoding("windows-1252");

            LocalReport report = new LocalReport(rdlcFilePath);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<ParametersOfAbsenceNoteLarkin> parametersList = new List<ParametersOfAbsenceNoteLarkin> { new ParametersOfAbsenceNoteLarkin { date = date, dateFacilitator = dateFacilitator } };

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", null);
            report.AddDataSource("dsParameters", parametersList);

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return result.MainStream;
        }
    }
}
