using AspNetCore.Reporting;
using FastReport;
using FastReport.Data;
using FastReport.Export.PdfSimple;
using FastReport.Utils;
using FastReport.Web;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        #region Group functions
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

            ReportResult result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return result.MainStream;
        }
        #endregion

        #region Generics reports functions
        public Stream DailyAssistanceReport(List<Workday_Client> workdayClientList)
        {
            List<Workday_Client> am_list = workdayClientList.Where(wc => wc.Session == "AM").ToList();
            List<Workday_Client> pm_list = workdayClientList.Where(wc => wc.Session == "PM").ToList();

            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\Generics\\rptDailyAssistance.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetFacilitatorDS(workdayClientList.First().Facilitator));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Facilitators");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetClinicDS(workdayClientList.First().Facilitator.Clinic));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clinics");

            dataSet = new DataSet();
            DataTable dt = GetWorkdaysClientsDS(am_list);
            int to = 14 - am_list.Count();
            for (int i = 0; i < to; i++)
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            0,
                                            string.Empty,
                                            true,
                                            0,
                                            string.Empty
                                        });
            }

            dataSet.Tables.Add(dt);
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients");

            dataSet = new DataSet();
            dt = GetWorkdaysClientsDS(pm_list);
            to = 14 - pm_list.Count();
            for (int i = 0; i < to; i++)
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            0,
                                            string.Empty,
                                            true,
                                            0,
                                            string.Empty
                                        });
            }

            dataSet.Tables.Add(dt);
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients1");

            string date = workdayClientList.First().Workday.Date.ToShortDateString();
            string additionalComments1 = string.Empty;
            string additionalComments2 = string.Empty;
            foreach (Workday_Client item in am_list)
            {
                if (item != null)
                {
                    if (!item.Present)
                    {
                        if (additionalComments1 != string.Empty)
                        {
                            additionalComments1 = $"{additionalComments1}\n{item.ClientName} - {item.CauseOfNotPresent}";
                        }
                        else
                        {
                            additionalComments1 = $"{item.ClientName} - {item.CauseOfNotPresent}";
                        }
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
                        {
                            additionalComments2 = $"{additionalComments2}\n{item.ClientName} - {item.CauseOfNotPresent}";
                        }
                        else
                        {
                            additionalComments2 = $"{item.ClientName} - {item.CauseOfNotPresent}";
                        }
                    }
                }
            }

            //signatures images                      
            string path = string.Empty;
            if (!string.IsNullOrEmpty(workdayClientList.First().Facilitator.Clinic.LogoPath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClientList.First().Facilitator.Clinic.LogoPath)}");
            }

            PictureObject pic1 = WebReport.Report.FindObject("Picture1") as PictureObject;
            pic1.Image = new Bitmap(path);

            PictureObject pic2 = WebReport.Report.FindObject("Picture2") as PictureObject;
            pic2.Image = new Bitmap(path);

            WebReport.Report.SetParameterValue("dateNote", date);
            WebReport.Report.SetParameterValue("session1", "AM");
            WebReport.Report.SetParameterValue("session2", "PM");
            WebReport.Report.SetParameterValue("additionalComments1", additionalComments1);
            WebReport.Report.SetParameterValue("additionalComments2", additionalComments2);

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }
        public Stream PrintIndividualSign(List<Workday_Client> workdayClientList)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\Generics\\rptIndividualSign.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetFacilitatorDS(workdayClientList.First().Facilitator));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Facilitators");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetClinicDS(workdayClientList.First().Facilitator.Clinic));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clinics");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(workdayClientList.First().Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetWorkdaysItemsDS(workdayClientList));
            WebReport.Report.RegisterData(dataSet.Tables[0], "WorkdaysItems");

            //images                      
            string path = string.Empty;
            if (!string.IsNullOrEmpty(workdayClientList.First().Facilitator.Clinic.LogoPath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClientList.First().Facilitator.Clinic.LogoPath)}");
            }

            PictureObject pic1 = WebReport.Report.FindObject("Picture1") as PictureObject;
            pic1.Image = new Bitmap(path);

            string session = workdayClientList.First().Session;
            WebReport.Report.SetParameterValue("session1", session);

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }
        #endregion

        #region Absences Notes functions
        public Stream LarkinAbsenceNoteReport(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\AbsencesNotes\\rptAbsenceNoteLARKINBEHAVIOR.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetWorkdayClientDS(workdayClient));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients");
            DataSet dataSet1 = new DataSet();
            dataSet1.Tables.Add(GetClientDS(workdayClient.Client));
            WebReport.Report.RegisterData(dataSet1.Tables[0], "Clients");
            DataSet dataSet2 = new DataSet();
            dataSet2.Tables.Add(GetFacilitatorDS(workdayClient.Facilitator));
            WebReport.Report.RegisterData(dataSet2.Tables[0], "Facilitators");

            string date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            string dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            WebReport.Report.SetParameterValue("dateNote", date);
            WebReport.Report.SetParameterValue("dateFacilitator", dateFacilitator);

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream AdvancedGroupMCAbsenceNoteReport(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\AbsencesNotes\\rptAbsenceNoteAdvancedGroupMC.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetWorkdayClientDS(workdayClient));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients");
            DataSet dataSet1 = new DataSet();
            dataSet1.Tables.Add(GetClientDS(workdayClient.Client));
            WebReport.Report.RegisterData(dataSet1.Tables[0], "Clients");
            DataSet dataSet2 = new DataSet();
            dataSet2.Tables.Add(GetFacilitatorDS(workdayClient.Facilitator));
            WebReport.Report.RegisterData(dataSet2.Tables[0], "Facilitators");

            string date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            string dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            WebReport.Report.SetParameterValue("dateNote", date);
            WebReport.Report.SetParameterValue("dateFacilitator", dateFacilitator);

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream AtlanticGroupMCAbsenceNoteReport(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\AbsencesNotes\\rptAbsenceNoteAtlanticGroupMC.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetWorkdayClientDS(workdayClient));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients");
            DataSet dataSet1 = new DataSet();
            dataSet1.Tables.Add(GetClientDS(workdayClient.Client));
            WebReport.Report.RegisterData(dataSet1.Tables[0], "Clients");
            DataSet dataSet2 = new DataSet();
            dataSet2.Tables.Add(GetFacilitatorDS(workdayClient.Facilitator));
            WebReport.Report.RegisterData(dataSet2.Tables[0], "Facilitators");

            string date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            string dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            WebReport.Report.SetParameterValue("dateNote", date);
            WebReport.Report.SetParameterValue("dateFacilitator", dateFacilitator);

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream SolAndVidaAbsenceNoteReport(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\AbsencesNotes\\rptAbsenceNoteSolAndVida.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetWorkdayClientDS(workdayClient));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients");
            DataSet dataSet1 = new DataSet();
            dataSet1.Tables.Add(GetClientDS(workdayClient.Client));
            WebReport.Report.RegisterData(dataSet1.Tables[0], "Clients");
            DataSet dataSet2 = new DataSet();
            dataSet2.Tables.Add(GetFacilitatorDS(workdayClient.Facilitator));
            WebReport.Report.RegisterData(dataSet2.Tables[0], "Facilitators");

            string date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            string dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            WebReport.Report.SetParameterValue("dateNote", date);
            WebReport.Report.SetParameterValue("dateFacilitator", dateFacilitator);

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream DavilaAbsenceNoteReport(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\AbsencesNotes\\rptAbsenceNoteDavila.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetWorkdayClientDS(workdayClient));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients");
            DataSet dataSet1 = new DataSet();
            dataSet1.Tables.Add(GetClientDS(workdayClient.Client));
            WebReport.Report.RegisterData(dataSet1.Tables[0], "Clients");
            DataSet dataSet2 = new DataSet();
            dataSet2.Tables.Add(GetFacilitatorDS(workdayClient.Facilitator));
            WebReport.Report.RegisterData(dataSet2.Tables[0], "Facilitators");

            string date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            string dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            WebReport.Report.SetParameterValue("dateNote", date);
            WebReport.Report.SetParameterValue("dateFacilitator", dateFacilitator);

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream HealthAndBeautyAbsenceNoteReport(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\AbsencesNotes\\rptAbsenceNoteHealthAndBeauty.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetWorkdayClientDS(workdayClient));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients");
            DataSet dataSet1 = new DataSet();
            dataSet1.Tables.Add(GetClientDS(workdayClient.Client));
            WebReport.Report.RegisterData(dataSet1.Tables[0], "Clients");
            DataSet dataSet2 = new DataSet();
            dataSet2.Tables.Add(GetFacilitatorDS(workdayClient.Facilitator));
            WebReport.Report.RegisterData(dataSet2.Tables[0], "Facilitators");

            string date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            string dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            WebReport.Report.SetParameterValue("dateNote", date);
            WebReport.Report.SetParameterValue("dateFacilitator", dateFacilitator);

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream FloridaSocialHSAbsenceNoteReport(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\AbsencesNotes\\rptAbsenceNoteFloridaSocialHS.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetWorkdayClientDS(workdayClient));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients");
            DataSet dataSet1 = new DataSet();
            dataSet1.Tables.Add(GetClientDS(workdayClient.Client));
            WebReport.Report.RegisterData(dataSet1.Tables[0], "Clients");
            DataSet dataSet2 = new DataSet();
            dataSet2.Tables.Add(GetFacilitatorDS(workdayClient.Facilitator));
            WebReport.Report.RegisterData(dataSet2.Tables[0], "Facilitators");

            string date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            string dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            WebReport.Report.SetParameterValue("dateNote", date);
            WebReport.Report.SetParameterValue("dateFacilitator", dateFacilitator);

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream DemoClinic1AbsenceNoteReport(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\AbsencesNotes\\rptAbsenceNoteDemoClinic1.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetWorkdayClientDS(workdayClient));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients");
            DataSet dataSet1 = new DataSet();
            dataSet1.Tables.Add(GetClientDS(workdayClient.Client));
            WebReport.Report.RegisterData(dataSet1.Tables[0], "Clients");
            DataSet dataSet2 = new DataSet();
            dataSet2.Tables.Add(GetFacilitatorDS(workdayClient.Facilitator));
            WebReport.Report.RegisterData(dataSet2.Tables[0], "Facilitators");

            string date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            string dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            WebReport.Report.SetParameterValue("dateNote", date);
            WebReport.Report.SetParameterValue("dateFacilitator", dateFacilitator);

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream DemoClinic2AbsenceNoteReport(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\AbsencesNotes\\rptAbsenceNoteDemoClinic2.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetWorkdayClientDS(workdayClient));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients");
            DataSet dataSet1 = new DataSet();
            dataSet1.Tables.Add(GetClientDS(workdayClient.Client));
            WebReport.Report.RegisterData(dataSet1.Tables[0], "Clients");
            DataSet dataSet2 = new DataSet();
            dataSet2.Tables.Add(GetFacilitatorDS(workdayClient.Facilitator));
            WebReport.Report.RegisterData(dataSet2.Tables[0], "Facilitators");

            string date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            string dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            WebReport.Report.SetParameterValue("dateNote", date);
            WebReport.Report.SetParameterValue("dateFacilitator", dateFacilitator);

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        #endregion

        #region MTP functions
        public Stream LarkinMTPReport(MTPEntity mtp)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\MTPs\\rptMTPLarkinBehavior.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(mtp.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetMtpDS(mtp));
            WebReport.Report.RegisterData(dataSet.Tables[0], "MTPs");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDiagnosticsListDS(mtp.Client.Clients_Diagnostics.ToList()));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Diagnostics");

            List<GoalEntity> goals1 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives1 = new List<ObjetiveEntity>();
            List<GoalEntity> goals2 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives2 = new List<ObjetiveEntity>();
            List<GoalEntity> goals3 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives3 = new List<ObjetiveEntity>();
            List<GoalEntity> goals4 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives4 = new List<ObjetiveEntity>();
            List<GoalEntity> goals5 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives5 = new List<ObjetiveEntity>();

            int i = 0;

            foreach (GoalEntity item in mtp.Goals)
            {
                if (i == 0)
                {
                    goals1 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives1 = item.Objetives.ToList();
                    }
                }
                if (i == 1)
                {
                    goals2 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives2 = item.Objetives.ToList();
                    }
                }
                if (i == 2)
                {
                    goals3 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives3 = item.Objetives.ToList();
                    }
                }
                if (i == 3)
                {
                    goals4 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives4 = item.Objetives.ToList();
                    }
                }
                if (i == 4)
                {
                    goals5 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives5 = item.Objetives.ToList();
                    }
                }
                i = ++i;
            }

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals5");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives5");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream SolAndVidaMTPReport(MTPEntity mtp)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\MTPs\\rptMTPSolAndVida.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(mtp.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetMtpDS(mtp));
            WebReport.Report.RegisterData(dataSet.Tables[0], "MTPs");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDiagnosticsListDS(mtp.Client.Clients_Diagnostics.ToList()));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Diagnostics");

            List<GoalEntity> goals1 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives1 = new List<ObjetiveEntity>();
            List<GoalEntity> goals2 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives2 = new List<ObjetiveEntity>();
            List<GoalEntity> goals3 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives3 = new List<ObjetiveEntity>();
            List<GoalEntity> goals4 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives4 = new List<ObjetiveEntity>();
            List<GoalEntity> goals5 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives5 = new List<ObjetiveEntity>();

            int i = 0;

            foreach (GoalEntity item in mtp.Goals)
            {
                if (i == 0)
                {
                    goals1 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives1 = item.Objetives.ToList();
                    }
                }
                if (i == 1)
                {
                    goals2 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives2 = item.Objetives.ToList();
                    }
                }
                if (i == 2)
                {
                    goals3 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives3 = item.Objetives.ToList();
                    }
                }
                if (i == 3)
                {
                    goals4 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives4 = item.Objetives.ToList();
                    }
                }
                if (i == 4)
                {
                    goals5 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives5 = item.Objetives.ToList();
                    }
                }
                i = ++i;
            }

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals5");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives5");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream HealthAndBeautyMTPReport(MTPEntity mtp)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\MTPs\\rptMTPHealthAndBeauty.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(mtp.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetMtpDS(mtp));
            WebReport.Report.RegisterData(dataSet.Tables[0], "MTPs");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDiagnosticsListDS(mtp.Client.Clients_Diagnostics.ToList()));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Diagnostics");

            List<GoalEntity> goals1 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives1 = new List<ObjetiveEntity>();
            List<GoalEntity> goals2 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives2 = new List<ObjetiveEntity>();
            List<GoalEntity> goals3 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives3 = new List<ObjetiveEntity>();
            List<GoalEntity> goals4 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives4 = new List<ObjetiveEntity>();
            List<GoalEntity> goals5 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives5 = new List<ObjetiveEntity>();

            int i = 0;

            foreach (GoalEntity item in mtp.Goals)
            {
                if (i == 0)
                {
                    goals1 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives1 = item.Objetives.ToList();
                    }
                }
                if (i == 1)
                {
                    goals2 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives2 = item.Objetives.ToList();
                    }
                }
                if (i == 2)
                {
                    goals3 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives3 = item.Objetives.ToList();
                    }
                }
                if (i == 3)
                {
                    goals4 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives4 = item.Objetives.ToList();
                    }
                }
                if (i == 4)
                {
                    goals5 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives5 = item.Objetives.ToList();
                    }
                }
                i = ++i;
            }

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals5");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives5");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream DavilaMTPReport(MTPEntity mtp)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\MTPs\\rptMTPDavila.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(mtp.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetMtpDS(mtp));
            WebReport.Report.RegisterData(dataSet.Tables[0], "MTPs");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDiagnosticsListDS(mtp.Client.Clients_Diagnostics.ToList()));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Diagnostics");

            List<GoalEntity> goals1 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives1 = new List<ObjetiveEntity>();
            List<GoalEntity> goals2 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives2 = new List<ObjetiveEntity>();
            List<GoalEntity> goals3 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives3 = new List<ObjetiveEntity>();
            List<GoalEntity> goals4 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives4 = new List<ObjetiveEntity>();
            List<GoalEntity> goals5 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives5 = new List<ObjetiveEntity>();

            int i = 0;

            foreach (GoalEntity item in mtp.Goals)
            {
                if (i == 0)
                {
                    goals1 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives1 = item.Objetives.ToList();
                    }
                }
                if (i == 1)
                {
                    goals2 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives2 = item.Objetives.ToList();
                    }
                }
                if (i == 2)
                {
                    goals3 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives3 = item.Objetives.ToList();
                    }
                }
                if (i == 3)
                {
                    goals4 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives4 = item.Objetives.ToList();
                    }
                }
                if (i == 4)
                {
                    goals5 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives5 = item.Objetives.ToList();
                    }
                }
                i = ++i;
            }

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals5");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives5");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream AdvancedGroupMCMTPReport(MTPEntity mtp)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\MTPs\\rptMTPAdvancedGroupMC.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(mtp.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetMtpDS(mtp));
            WebReport.Report.RegisterData(dataSet.Tables[0], "MTPs");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDiagnosticsListDS(mtp.Client.Clients_Diagnostics.ToList()));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Diagnostics");

            List<GoalEntity> goals1 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives1 = new List<ObjetiveEntity>();
            List<GoalEntity> goals2 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives2 = new List<ObjetiveEntity>();
            List<GoalEntity> goals3 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives3 = new List<ObjetiveEntity>();
            List<GoalEntity> goals4 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives4 = new List<ObjetiveEntity>();
            List<GoalEntity> goals5 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives5 = new List<ObjetiveEntity>();

            int i = 0;

            foreach (GoalEntity item in mtp.Goals)
            {
                if (i == 0)
                {
                    goals1 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives1 = item.Objetives.ToList();
                    }
                }
                if (i == 1)
                {
                    goals2 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives2 = item.Objetives.ToList();
                    }
                }
                if (i == 2)
                {
                    goals3 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives3 = item.Objetives.ToList();
                    }
                }
                if (i == 3)
                {
                    goals4 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives4 = item.Objetives.ToList();
                    }
                }
                if (i == 4)
                {
                    goals5 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives5 = item.Objetives.ToList();
                    }
                }
                i = ++i;
            }

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals5");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives5");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream AtlanticGroupMCMTPReport(MTPEntity mtp)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\MTPs\\rptMTPAtlanticGroupMC.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(mtp.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetMtpDS(mtp));
            WebReport.Report.RegisterData(dataSet.Tables[0], "MTPs");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDiagnosticsListDS(mtp.Client.Clients_Diagnostics.ToList()));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Diagnostics");

            List<GoalEntity> goals1 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives1 = new List<ObjetiveEntity>();
            List<GoalEntity> goals2 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives2 = new List<ObjetiveEntity>();
            List<GoalEntity> goals3 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives3 = new List<ObjetiveEntity>();
            List<GoalEntity> goals4 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives4 = new List<ObjetiveEntity>();
            List<GoalEntity> goals5 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives5 = new List<ObjetiveEntity>();

            int i = 0;

            foreach (GoalEntity item in mtp.Goals)
            {
                if (i == 0)
                {
                    goals1 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives1 = item.Objetives.ToList();
                    }
                }
                if (i == 1)
                {
                    goals2 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives2 = item.Objetives.ToList();
                    }
                }
                if (i == 2)
                {
                    goals3 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives3 = item.Objetives.ToList();
                    }
                }
                if (i == 3)
                {
                    goals4 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives4 = item.Objetives.ToList();
                    }
                }
                if (i == 4)
                {
                    goals5 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives5 = item.Objetives.ToList();
                    }
                }
                i = ++i;
            }

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals5");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives5");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream FloridaSocialHSMTPReport(MTPEntity mtp)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\MTPs\\rptMTPFloridaSocialHS.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(mtp.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetMtpDS(mtp));
            WebReport.Report.RegisterData(dataSet.Tables[0], "MTPs");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDiagnosticsListDS(mtp.Client.Clients_Diagnostics.ToList()));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Diagnostics");

            List<GoalEntity> goals1 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives1 = new List<ObjetiveEntity>();
            List<GoalEntity> goals2 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives2 = new List<ObjetiveEntity>();
            List<GoalEntity> goals3 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives3 = new List<ObjetiveEntity>();
            List<GoalEntity> goals4 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives4 = new List<ObjetiveEntity>();
            List<GoalEntity> goals5 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives5 = new List<ObjetiveEntity>();

            int i = 0;

            foreach (GoalEntity item in mtp.Goals)
            {
                if (i == 0)
                {
                    goals1 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives1 = item.Objetives.ToList();
                    }
                }
                if (i == 1)
                {
                    goals2 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives2 = item.Objetives.ToList();
                    }
                }
                if (i == 2)
                {
                    goals3 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives3 = item.Objetives.ToList();
                    }
                }
                if (i == 3)
                {
                    goals4 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives4 = item.Objetives.ToList();
                    }
                }
                if (i == 4)
                {
                    goals5 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives5 = item.Objetives.ToList();
                    }
                }
                i = ++i;
            }

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals5");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives5");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream DemoClinic1MTPReport(MTPEntity mtp)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\MTPs\\rptMTPDemoClinic1.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(mtp.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetMtpDS(mtp));
            WebReport.Report.RegisterData(dataSet.Tables[0], "MTPs");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDiagnosticsListDS(mtp.Client.Clients_Diagnostics.ToList()));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Diagnostics");

            List<GoalEntity> goals1 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives1 = new List<ObjetiveEntity>();
            List<GoalEntity> goals2 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives2 = new List<ObjetiveEntity>();
            List<GoalEntity> goals3 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives3 = new List<ObjetiveEntity>();
            List<GoalEntity> goals4 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives4 = new List<ObjetiveEntity>();
            List<GoalEntity> goals5 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives5 = new List<ObjetiveEntity>();

            int i = 0;

            foreach (GoalEntity item in mtp.Goals)
            {
                if (i == 0)
                {
                    goals1 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives1 = item.Objetives.ToList();
                    }
                }
                if (i == 1)
                {
                    goals2 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives2 = item.Objetives.ToList();
                    }
                }
                if (i == 2)
                {
                    goals3 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives3 = item.Objetives.ToList();
                    }
                }
                if (i == 3)
                {
                    goals4 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives4 = item.Objetives.ToList();
                    }
                }
                if (i == 4)
                {
                    goals5 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives5 = item.Objetives.ToList();
                    }
                }
                i = ++i;
            }

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals5");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives5");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }

        public Stream DemoClinic2MTPReport(MTPEntity mtp)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\MTPs\\rptMTPDemoClinic2.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(mtp.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetMtpDS(mtp));
            WebReport.Report.RegisterData(dataSet.Tables[0], "MTPs");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDiagnosticsListDS(mtp.Client.Clients_Diagnostics.ToList()));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Diagnostics");

            List<GoalEntity> goals1 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives1 = new List<ObjetiveEntity>();
            List<GoalEntity> goals2 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives2 = new List<ObjetiveEntity>();
            List<GoalEntity> goals3 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives3 = new List<ObjetiveEntity>();
            List<GoalEntity> goals4 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives4 = new List<ObjetiveEntity>();
            List<GoalEntity> goals5 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives5 = new List<ObjetiveEntity>();

            int i = 0;

            foreach (GoalEntity item in mtp.Goals)
            {
                if (i == 0)
                {
                    goals1 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives1 = item.Objetives.ToList();
                    }
                }
                if (i == 1)
                {
                    goals2 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives2 = item.Objetives.ToList();
                    }
                }
                if (i == 2)
                {
                    goals3 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives3 = item.Objetives.ToList();
                    }
                }
                if (i == 3)
                {
                    goals4 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives4 = item.Objetives.ToList();
                    }
                }
                if (i == 4)
                {
                    goals5 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives5 = item.Objetives.ToList();
                    }
                }
                i = ++i;
            }

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives1));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives1");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives2");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives3));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives3");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives4));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives4");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetGoalsListDS(goals5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Goals5");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetObjetivesListDS(objetives5));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Objetives5");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }
        #endregion

        #region Approved Notes functions
        public Stream DavilaNoteReportSchema4(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\ApprovedNotes\\rptNoteDAVILA3.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetWorkdayClientDS(workdayClient));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Clients");
            DataSet dataSet1 = new DataSet();
            dataSet1.Tables.Add(GetClientDS(workdayClient.Client));
            WebReport.Report.RegisterData(dataSet1.Tables[0], "Clients");            
            DataSet dataSet2 = new DataSet();
            dataSet2.Tables.Add(GetFacilitatorDS(workdayClient.Facilitator));
            WebReport.Report.RegisterData(dataSet2.Tables[0], "Facilitators");
            DataSet dataSet3 = new DataSet();
            dataSet3.Tables.Add(GetSupervisorDS(workdayClient.Note.Supervisor));
            WebReport.Report.RegisterData(dataSet3.Tables[0], "Supervisors");
            DataSet dataSet4 = new DataSet();
            dataSet4.Tables.Add(GetNoteDS(workdayClient.Note));
            WebReport.Report.RegisterData(dataSet4.Tables[0], "Notes");

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetNoteActivityDS(item));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Notes_Activities");

                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetActivityDS(item.Activity));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Activities1");

                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetThemeDS(item.Activity.Theme));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Themes1");
                    
                    if (item.Objetive != null)
                    {
                        num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                        goal_text = item.Objetive.Goal.Name;
                        num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                        obj_text = item.Objetive.Description;
                    }
                }
                if (i == 1)
                {
                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetActivityDS(item.Activity));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Activities2");

                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetThemeDS(item.Activity.Theme));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Themes2");

                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 2)
                {
                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetActivityDS(item.Activity));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Activities3");

                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetThemeDS(item.Activity.Theme));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Themes3");

                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }                
                i = ++i;
            }

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            dataSet = new DataSet();
            dataSet.Tables.Add(GetSignaturesDS(stream1, stream2));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Signatures");

            WebReport.Report.SetParameterValue("datenote", date);
            WebReport.Report.SetParameterValue("dateFacilitator", dateFacilitator);
            WebReport.Report.SetParameterValue("dateSupervisor", dateSupervisor);
            WebReport.Report.SetParameterValue("num_of_goal", num_of_goal);
            WebReport.Report.SetParameterValue("goal_text", goal_text);
            WebReport.Report.SetParameterValue("num_of_obj", num_of_obj);
            WebReport.Report.SetParameterValue("obj_text", obj_text);            

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;            

            return stream;
        }
        #endregion

        #region System.Data functions 
        private DataTable GetWorkdayClientDS(Workday_Client workdayClient)
        {
            DataTable dt = new DataTable
            {
                TableName = "Workday_Client"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("WorkdayId", typeof(int));
            dt.Columns.Add("ClientId", typeof(int));
            dt.Columns.Add("Session", typeof(string));
            dt.Columns.Add("Present", typeof(bool));
            dt.Columns.Add("FacilitatorId", typeof(int));
            dt.Columns.Add("CauseOfNotPresent", typeof(string));

            dt.Rows.Add(new object[]
                                        {
                                            workdayClient.Id,
                                            workdayClient.Workday.Id,
                                            workdayClient.Client.Id,
                                            workdayClient.Session,
                                            workdayClient.Present,
                                            workdayClient.Facilitator.Id,
                                            workdayClient.CauseOfNotPresent
                                        });

            return dt;
        }

        private DataTable GetWorkdaysClientsDS(List<Workday_Client> listWorkdayClient)
        {
            DataTable dt = new DataTable
            {
                TableName = "Workday_Client"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("WorkdayId", typeof(int));
            dt.Columns.Add("ClientId", typeof(int));
            dt.Columns.Add("Session", typeof(string));
            dt.Columns.Add("Present", typeof(bool));
            dt.Columns.Add("FacilitatorId", typeof(int));
            dt.Columns.Add("CauseOfNotPresent", typeof(string));

            foreach (Workday_Client item in listWorkdayClient)
            {
                dt.Rows.Add(new object[]
                                        {
                                            item.Id,
                                            item.Workday.Id,
                                            item.Client.Id,
                                            item.ClientName,
                                            item.Present,
                                            item.Facilitator.Id,
                                            item.CauseOfNotPresent
                                        });
            }

            return dt;
        }

        private DataTable GetWorkdaysItemsDS(List<Workday_Client> listWorkdayClient)
        {
            DataTable dt = new DataTable
            {
                TableName = "WorkdaysItems"
            };

            // Create columns
            dt.Columns.Add("DayOfWeek", typeof(string));
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("Present", typeof(bool));

            listWorkdayClient = listWorkdayClient.OrderBy(wc => wc.Workday.Date).ToList();

            foreach (Workday_Client item in listWorkdayClient)
            {
                dt.Rows.Add(new object[]
                                        {
                                            item.Workday.Day,
                                            item.Workday.Date,
                                            item.Present
                                        });
            }

            return dt;
        }

        private DataTable GetClientDS(ClientEntity client)
        {
            DataTable dt = new DataTable
            {
                TableName = "Client"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Gender", typeof(int));
            dt.Columns.Add("Code", typeof(string));
            dt.Columns.Add("ClinicId", typeof(int));
            dt.Columns.Add("DateOfBirth", typeof(DateTime));
            dt.Columns.Add("MedicalID", typeof(string));
            dt.Columns.Add("Status", typeof(int));
            dt.Columns.Add("GroupId", typeof(int));

            dt.Rows.Add(new object[]
                                        {
                                            client.Id,
                                            client.Name,
                                            client.Gender,
                                            client.Code,
                                            client.Clinic.Id,
                                            client.DateOfBirth,
                                            client.MedicaidID,
                                            client.Status,
                                            0
                                        });

            return dt;
        }

        private DataTable GetFacilitatorDS(FacilitatorEntity facilitator)
        {
            DataTable dt = new DataTable
            {
                TableName = "Facilitator"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Codigo", typeof(string));
            dt.Columns.Add("ClinicId", typeof(int));
            dt.Columns.Add("Status", typeof(int));
            dt.Columns.Add("LinkedUser", typeof(string));
            dt.Columns.Add("SignaturePath", typeof(string));

            dt.Rows.Add(new object[]
                                        {
                                            facilitator.Id,
                                            facilitator.Name,
                                            facilitator.Codigo,
                                            facilitator.Clinic.Id,
                                            facilitator.Status,
                                            facilitator.LinkedUser,
                                            facilitator.SignaturePath
                                        });

            return dt;
        }

        private DataTable GetSupervisorDS(SupervisorEntity supervisor)
        {
            DataTable dt = new DataTable
            {
                TableName = "Supervisor"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Firm", typeof(string));
            dt.Columns.Add("Code", typeof(string));
            dt.Columns.Add("ClinicId", typeof(int));            
            dt.Columns.Add("LinkedUser", typeof(string));
            dt.Columns.Add("SignaturePath", typeof(string));

            dt.Rows.Add(new object[]
                                        {
                                            supervisor.Id,
                                            supervisor.Name,
                                            supervisor.Firm,
                                            supervisor.Code,
                                            supervisor.Clinic.Id,
                                            supervisor.LinkedUser,
                                            supervisor.SignaturePath
                                        });

            return dt;
        }

        private DataTable GetClinicDS(ClinicEntity clinic)
        {
            DataTable dt = new DataTable
            {
                TableName = "Clinic"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("LogoPath", typeof(string));
            dt.Columns.Add("Schema", typeof(int));

            dt.Rows.Add(new object[]
                                        {
                                            clinic.Id,
                                            clinic.Name,
                                            clinic.LogoPath,
                                            clinic.Schema
                                        });

            return dt;
        }

        private DataTable GetMtpDS(MTPEntity mtp)
        {
            DataTable dt = new DataTable
            {
                TableName = "MTP"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("ClientId", typeof(int));
            dt.Columns.Add("AdmisionDate", typeof(DateTime));
            dt.Columns.Add("MTPDevelopedDate", typeof(DateTime));
            dt.Columns.Add("StartTime", typeof(DateTime));
            dt.Columns.Add("EndTime", typeof(DateTime));            
            dt.Columns.Add("LevelCare", typeof(string));
            dt.Columns.Add("InitialDischargeCriteria", typeof(string));
            dt.Columns.Add("Modality", typeof(string));
            dt.Columns.Add("Frecuency", typeof(string));
            dt.Columns.Add("NumberOfMonths", typeof(string));
            dt.Columns.Add("Setting", typeof(string));

            dt.Rows.Add(new object[]
                                        {
                                            mtp.Id,
                                            mtp.Client.Id,
                                            mtp.AdmisionDate,
                                            mtp.MTPDevelopedDate,
                                            mtp.StartTime,
                                            mtp.EndTime,
                                            mtp.LevelCare,
                                            mtp.InitialDischargeCriteria,
                                            mtp.Modality,
                                            mtp.Frecuency,
                                            mtp.NumberOfMonths,
                                            mtp.Setting
                                        });

            return dt;
        }

        private DataTable GetNoteDS(NoteEntity note)
        {
            DataTable dt = new DataTable
            {
                TableName = "Note"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("PlanNote", typeof(string));
            dt.Columns.Add("Status", typeof(int));
            dt.Columns.Add("Workday_Client_FK", typeof(int));
            dt.Columns.Add("Adequate", typeof(bool));
            dt.Columns.Add("AdequateAC", typeof(bool));
            dt.Columns.Add("Anxious", typeof(bool));
            dt.Columns.Add("Congruent", typeof(bool));
            dt.Columns.Add("Depressed", typeof(bool));
            dt.Columns.Add("Dramatized", typeof(bool));
            dt.Columns.Add("Euphoric", typeof(bool));
            dt.Columns.Add("Euthymic", typeof(bool));
            dt.Columns.Add("Fair", typeof(bool));
            dt.Columns.Add("Faulty", typeof(bool));
            dt.Columns.Add("Guarded", typeof(bool));
            dt.Columns.Add("Hostile", typeof(bool));
            dt.Columns.Add("Impaired", typeof(bool));
            dt.Columns.Add("Inadequate", typeof(bool));
            dt.Columns.Add("Irritable", typeof(bool));
            dt.Columns.Add("Limited", typeof(bool));
            dt.Columns.Add("MildlyImpaired", typeof(bool));
            dt.Columns.Add("Motivated", typeof(bool));
            dt.Columns.Add("Negativistic", typeof(bool));
            dt.Columns.Add("Normal", typeof(bool));
            dt.Columns.Add("NotPerson", typeof(bool));
            dt.Columns.Add("NotPlace", typeof(bool));
            dt.Columns.Add("NotTime", typeof(bool));
            dt.Columns.Add("Optimistic", typeof(bool));
            dt.Columns.Add("OrientedX3", typeof(bool));
            dt.Columns.Add("Present", typeof(bool));
            dt.Columns.Add("SeverelyImpaired", typeof(bool));
            dt.Columns.Add("ShortSpanned", typeof(bool));
            dt.Columns.Add("SupervisorId", typeof(int));
            dt.Columns.Add("Unmotivated", typeof(bool));
            dt.Columns.Add("Withdrawn", typeof(bool));
            dt.Columns.Add("DateOfApprove", typeof(DateTime));
            dt.Columns.Add("Workday_CientId", typeof(int));
            dt.Columns.Add("Decompensating", typeof(bool));
            dt.Columns.Add("MinimalProgress", typeof(bool));
            dt.Columns.Add("ModerateProgress", typeof(bool));
            dt.Columns.Add("NoProgress", typeof(bool));
            dt.Columns.Add("Regression", typeof(bool));
            dt.Columns.Add("SignificantProgress", typeof(bool));
            dt.Columns.Add("UnableToDetermine", typeof(bool));
            dt.Columns.Add("Setting", typeof(string));
            dt.Columns.Add("MTPId", typeof(int));
            dt.Columns.Add("Schema", typeof(int));

            dt.Rows.Add(new object[]
                                        {
                                            note.Id,
                                            note.PlanNote,
                                            note.Status,
                                            note.Workday_Client_FK,
                                            note.Adequate,
                                            note.AdequateAC,
                                            note.Anxious,
                                            note.Congruent,
                                            note.Depressed,
                                            note.Dramatized,
                                            note.Euphoric,
                                            note.Euthymic,
                                            note.Fair,
                                            note.Faulty,
                                            note.Guarded,
                                            note.Hostile,
                                            note.Impaired,
                                            note.Inadequate,
                                            note.Irritable,
                                            note.Limited,
                                            note.MildlyImpaired,
                                            note.Motivated,
                                            note.Negativistic,
                                            note.Normal,
                                            note.NotPerson,
                                            note.NotPlace,
                                            note.NotTime,
                                            note.Optimistic,
                                            note.OrientedX3,
                                            note.Present,
                                            note.SeverelyImpaired,
                                            note.ShortSpanned,
                                            note.Supervisor.Id,
                                            note.Unmotivated,
                                            note.Withdrawn,
                                            note.DateOfApprove,
                                            note.Workday_Cient.Id,
                                            note.Decompensating,
                                            note.MinimalProgress,
                                            note.ModerateProgress,
                                            note.NoProgress,
                                            note.Regression,
                                            note.SignificantProgress,
                                            note.UnableToDetermine,
                                            note.Setting,
                                            note.MTPId,
                                            note.Schema
            });

            return dt;
        }

        private DataTable GetNoteActivityDS(Note_Activity noteActivity)
        {
            DataTable dt = new DataTable
            {
                TableName = "NoteActivity"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("NoteId", typeof(int));
            dt.Columns.Add("ActivityId", typeof(int));            
            dt.Columns.Add("AnswerClient", typeof(string));
            dt.Columns.Add("AnswerFacilitator", typeof(string));
            dt.Columns.Add("ObjetiveId", typeof(int));            

            dt.Rows.Add(new object[]
                                        {
                                            noteActivity.Id,
                                            noteActivity.Note.Id,
                                            noteActivity.Activity.Id,
                                            noteActivity.AnswerClient,
                                            noteActivity.AnswerFacilitator,
                                            noteActivity.Objetive.Id                                            
            });

            return dt;
        }

        private DataTable GetThemeDS(ThemeEntity theme)
        {
            DataTable dt = new DataTable
            {
                TableName = "Theme"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Day", typeof(int));
            dt.Columns.Add("ClinicId", typeof(int));            

            dt.Rows.Add(new object[]
                                        {
                                            theme.Id,
                                            theme.Name,
                                            theme.Day,
                                            theme.Clinic.Id
            });

            return dt;
        }

        private DataTable GetActivityDS(ActivityEntity activity)
        {
            DataTable dt = new DataTable
            {
                TableName = "Activity"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("ThemeId", typeof(int));
            dt.Columns.Add("DateCreated", typeof(DateTime));
            dt.Columns.Add("DateOfApprove", typeof(DateTime));
            dt.Columns.Add("FacilitatorId", typeof(int));
            dt.Columns.Add("Status", typeof(int));
            dt.Columns.Add("SupervisorId", typeof(int));

            dt.Rows.Add(new object[]
                                        {
                                            activity.Id,
                                            activity.Name,
                                            activity.Theme.Id,
                                            activity.DateCreated,
                                            activity.DateOfApprove,
                                            0,
                                            activity.Status,
                                            0
            });

            return dt;
        }

        private DataTable GetDiagnosticsListDS(List<Client_Diagnostic> diagnosticList)
        {
            DataTable dt = new DataTable
            {
                TableName = "Diagnostics"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Code", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedOn", typeof(DateTime));
            dt.Columns.Add("LastModifiedBy", typeof(string));
            dt.Columns.Add("LastModifiedOn", typeof(DateTime));

            foreach (Client_Diagnostic item in diagnosticList)
            {

                dt.Rows.Add(new object[]
                                        {
                                            item.Diagnostic.Id,
                                            item.Diagnostic.Code,
                                            item.Diagnostic.Description,
                                            item.Diagnostic.CreatedBy,
                                            item.Diagnostic.CreatedOn,
                                            item.Diagnostic.LastModifiedBy,
                                            item.Diagnostic.LastModifiedOn,
                                        });
            }

            return dt;
        }

        private DataTable GetGoalsListDS(List<GoalEntity> goalsList)
        {
            DataTable dt = new DataTable
            {
                TableName = "Goal"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("AreaOfFocus", typeof(string));
            dt.Columns.Add("MTPId", typeof(int));
            dt.Columns.Add("Number", typeof(int));

            foreach (GoalEntity item in goalsList)
            {

                dt.Rows.Add(new object[]
                                        {
                                            item.Id,
                                            item.Name,
                                            item.AreaOfFocus,
                                            item.MTP.Id,
                                            item.Number
                                        });
            }

            return dt;
        }

        private DataTable GetObjetivesListDS(List<ObjetiveEntity> objetivesList)
        {
            DataTable dt = new DataTable
            {
                TableName = "Objetive"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Objetive", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("DateOpened", typeof(DateTime));
            dt.Columns.Add("DateTarget", typeof(DateTime));
            dt.Columns.Add("DateResolved", typeof(DateTime));
            dt.Columns.Add("Intervention", typeof(string));
            dt.Columns.Add("GoalId", typeof(int));

            foreach (ObjetiveEntity item in objetivesList)
            {

                dt.Rows.Add(new object[]
                                        {
                                            item.Id,
                                            item.Objetive,
                                            item.Description,
                                            item.DateOpened,
                                            item.DateTarget,
                                            item.DateResolved,
                                            item.Intervention,
                                            0
                                        });
            }

            return dt;
        }

        private DataTable GetSignaturesDS(byte[] supervisorSignature, byte[] facilitatorSignature)
        {
            DataTable dt = new DataTable
            {
                TableName = "Signature"
            };

            // Create columns
            dt.Columns.Add("supervisorSignature", typeof(byte[]));
            dt.Columns.Add("facilitatorSignature", typeof(byte[]));
            

            dt.Rows.Add(new object[]
                                        {
                                            supervisorSignature,
                                            facilitatorSignature
                                        });
            

            return dt;
        }

        #endregion
    }
}
