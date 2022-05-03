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

        #region PSR general reports
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

        #region PSR Absense Notes reports
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

        #region MTP reports
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

        #region Approved PSR Notes reports
        public Stream FloridaSocialHSNoteReportSchema3(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\ApprovedNotes\\rptFloridaSocialHSNote2.frx";

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
            dataSet3.Tables.Add(GetSupervisorDS(workdayClient.NoteP.Supervisor));
            WebReport.Report.RegisterData(dataSet3.Tables[0], "Supervisors");
            DataSet dataSet4 = new DataSet();
            dataSet4.Tables.Add(GetNotePDS(workdayClient.NoteP));
            WebReport.Report.RegisterData(dataSet4.Tables[0], "NotesP");

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (NoteP_Activity item in workdayClient.NoteP.NotesP_Activities)
            {
                if (i == 0)
                {
                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetNotePActivityDS(item));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "NotesP_Activities1");

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
                    dataSet.Tables.Add(GetNotePActivityDS(item));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "NotesP_Activities2");

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
                    dataSet.Tables.Add(GetNotePActivityDS(item));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "NotesP_Activities3");

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
                if (i == 3)
                {
                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetNotePActivityDS(item));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "NotesP_Activities4");

                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetActivityDS(item.Activity));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Activities4");

                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetThemeDS(item.Activity.Theme));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Themes4");

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

            i = 0;
            List<Workday_Activity_Facilitator> waf = workdayClient.Workday.Workdays_Activities_Facilitators
                                                                          .Where(waf => waf.Facilitator.Id == workdayClient.Facilitator.Id)
                                                                          .ToList();
            foreach (Workday_Activity_Facilitator item in waf)
            {
                if (i == 0)
                {
                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetWorkdayActivityFacilitatorDS(item));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Activities_Facilitators1");                    
                }
                if (i == 1)
                {
                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetWorkdayActivityFacilitatorDS(item));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Activities_Facilitators2");
                }
                if (i == 2)
                {
                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetWorkdayActivityFacilitatorDS(item));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Activities_Facilitators3");
                }
                if (i == 3)
                {
                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetWorkdayActivityFacilitatorDS(item));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "Workdays_Activities_Facilitators4");
                }
                i = ++i;
            }

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.NoteP.DateOfApprove.Value.ToShortDateString();

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.NoteP.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.NoteP.Supervisor.SignaturePath)}");
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

        #region Approved Individual Notes reports
        public Stream DavilaIndNoteReportSchema1(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\ApprovedNotes\\rptIndNoteDAVILA0.frx";

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
            dataSet3.Tables.Add(GetSupervisorDS(workdayClient.IndividualNote.Supervisor));
            WebReport.Report.RegisterData(dataSet3.Tables[0], "Supervisors");
            DataSet dataSet4 = new DataSet();
            dataSet4.Tables.Add(GetIndividualNoteDS(workdayClient.IndividualNote));
            WebReport.Report.RegisterData(dataSet4.Tables[0], "IndividualNotes");

            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;

            if (workdayClient.IndividualNote.Objective != null)
            {
                num_of_goal = $"GOAL #{workdayClient.IndividualNote.Objective.Goal.Number}:";
                goal_text = workdayClient.IndividualNote.Objective.Goal.Name;
                num_of_obj = $"OBJ {workdayClient.IndividualNote.Objective.Objetive}:";
                obj_text = workdayClient.IndividualNote.Objective.Description;
            }

            var date = $"{workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.IndividualNote
                .DateOfApprove.Value.ToShortDateString();

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.IndividualNote.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.IndividualNote.Supervisor.SignaturePath)}");
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

        public Stream FloridaSocialHSIndNoteReportSchema1(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\ApprovedNotes\\rptIndNoteFloridaSocialHS0.frx";

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
            dataSet3.Tables.Add(GetSupervisorDS(workdayClient.IndividualNote.Supervisor));
            WebReport.Report.RegisterData(dataSet3.Tables[0], "Supervisors");
            DataSet dataSet4 = new DataSet();
            dataSet4.Tables.Add(GetIndividualNoteDS(workdayClient.IndividualNote));
            WebReport.Report.RegisterData(dataSet4.Tables[0], "IndividualNotes");

            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;

            if (workdayClient.IndividualNote.Objective != null)
            {
                num_of_goal = $"GOAL #{workdayClient.IndividualNote.Objective.Goal.Number}:";
                goal_text = workdayClient.IndividualNote.Objective.Goal.Name;
                num_of_obj = $"OBJ {workdayClient.IndividualNote.Objective.Objetive}:";
                obj_text = workdayClient.IndividualNote.Objective.Description;
            }

            var date = $"{workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.IndividualNote
                .DateOfApprove.Value.ToShortDateString();

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.IndividualNote.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.IndividualNote.Supervisor.SignaturePath)}");
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

        #region Approved Group Notes reports
        public Stream DavilaGroupNoteReportSchema1(Workday_Client workdayClient)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\ApprovedNotes\\rptGroupNoteDAVILA0.frx";

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
            dataSet3.Tables.Add(GetSupervisorDS(workdayClient.GroupNote.Supervisor));
            WebReport.Report.RegisterData(dataSet3.Tables[0], "Supervisors");
            DataSet dataSet4 = new DataSet();
            dataSet4.Tables.Add(GetGroupNoteDS(workdayClient.GroupNote));
            WebReport.Report.RegisterData(dataSet4.Tables[0], "GroupNotes");

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (GroupNote_Activity item in workdayClient.GroupNote.GroupNotes_Activities)
            {
                if (i == 0)
                {
                    dataSet = new DataSet();
                    dataSet.Tables.Add(GetGroupNoteActivityDS(item));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "GroupNotes_Activities1");

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
                    dataSet.Tables.Add(GetGroupNoteActivityDS(item));
                    WebReport.Report.RegisterData(dataSet.Tables[0], "GroupNotes_Activities2");

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
                i = ++i;
            }

            var date = $"{workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.GroupNote.DateOfApprove.Value.ToShortDateString();

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.GroupNote.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.GroupNote.Supervisor.SignaturePath)}");
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

        #region Intake reports
        public Stream FloridaSocialHSIntakeReport(IntakeScreeningEntity intake)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\Intakes\\rptIntakeFloridaSocialHS.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(intake.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetClinicDS(intake.Client.Clinic));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clinics");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetEmergencyContactDS(intake.Client.EmergencyContact));
            WebReport.Report.RegisterData(dataSet.Tables[0], "EmergencyContacts");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetLegalGuardianDS(intake.Client.LegalGuardian));
            WebReport.Report.RegisterData(dataSet.Tables[0], "LegalGuardians");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeScreeningsDS(intake.Client.IntakeScreening));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeScreenings");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeConsentForTreatmentDS(intake.Client.IntakeConsentForTreatment));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeConsentForTreatment");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeConsentForReleaseDS(intake.Client.IntakeConsentForRelease));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeConsentForRelease");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeConsumerRightsDS(intake.Client.IntakeConsumerRights));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeConsumerRights");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeAcknowledgementDS(intake.Client.IntakeAcknowledgementHipa));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeAcknowledgement");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeAccessToServicesDS(intake.Client.IntakeAccessToServices));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeAccessToServices");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeOrientationCheckListDS(intake.Client.IntakeOrientationChecklist));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeOrientationCheckList");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeFeeAgreementDS(intake.Client.IntakeFeeAgreement));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeFeeAgreement");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeTuberculosisDS(intake.Client.IntakeTuberculosis));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeTuberculosis");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeTransportationDS(intake.Client.IntakeTransportation));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeTransportation");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeConsentPhotographDS(intake.Client.IntakeConsentPhotograph));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeConsentPhotograph");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetIntakeMedicalHistoryDS(intake.Client.IntakeMedicalHistory));
            WebReport.Report.RegisterData(dataSet.Tables[0], "IntakeMedicalHistory");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDischargeDS(intake.Client.Discharge));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Discharge");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }
        #endregion

        #region Fars Form reports
        public Stream FloridaSocialHSFarsReport(FarsFormEntity fars)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\Fars\\rptFarsFloridaSocialHS.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(fars.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetClinicDS(fars.Client.Clinic));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clinics");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetEmergencyContactDS(fars.Client.EmergencyContact));
            WebReport.Report.RegisterData(dataSet.Tables[0], "EmergencyContacts");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetLegalGuardianDS(fars.Client.LegalGuardian));
            WebReport.Report.RegisterData(dataSet.Tables[0], "LegalGuardians");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetFarsDS(fars));
            WebReport.Report.RegisterData(dataSet.Tables[0], "FarsForm");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }
        #endregion

        #region Discharge reports
        public Stream FloridaSocialHSDischargeReport(DischargeEntity discharge)
        {
            WebReport WebReport = new WebReport();

            string rdlcFilePath = $"{_webhostEnvironment.WebRootPath}\\Reports\\Discharges\\rptDischargeFloridaSocialHS.frx";

            RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));
            WebReport.Report.Load(rdlcFilePath);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(GetClientDS(discharge.Client));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clients");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetClinicDS(discharge.Client.Clinic));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Clinics");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetEmergencyContactDS(discharge.Client.EmergencyContact));
            WebReport.Report.RegisterData(dataSet.Tables[0], "EmergencyContacts");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetLegalGuardianDS(discharge.Client.LegalGuardian));
            WebReport.Report.RegisterData(dataSet.Tables[0], "LegalGuardians");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDiagnosticsListDS(discharge.Client.Clients_Diagnostics.ToList()));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Diagnostics");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetDischargeDS(discharge));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Discharge");

            dataSet = new DataSet();
            dataSet.Tables.Add(GetMedicationsListDS(discharge.Client.MedicationList.ToList()));
            WebReport.Report.RegisterData(dataSet.Tables[0], "Medication");

            WebReport.Report.Prepare();

            Stream stream = new MemoryStream();
            WebReport.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;

            return stream;
        }
        #endregion

        #region Utils functions
        public byte[] ConvertStreamToByteArray(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
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
            dt.Columns.Add("GroupSize", typeof(int));

            dt.Rows.Add(new object[]
                                        {
                                            workdayClient.Id,
                                            workdayClient.Workday.Id,
                                            (workdayClient.Client == null) ? 0 : workdayClient.Client.Id,
                                            workdayClient.Session,
                                            workdayClient.Present,
                                            workdayClient.Facilitator.Id,
                                            workdayClient.CauseOfNotPresent,
                                            (workdayClient.GroupSize != null) ? workdayClient.GroupSize : 0
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
            dt.Columns.Add("GroupSize", typeof(int));

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
                                            item.CauseOfNotPresent,
                                            (item.GroupSize != null) ? item.GroupSize : 0
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
            dt.Columns.Add("AlternativeAddress", typeof(string));
            dt.Columns.Add("City", typeof(string));
            dt.Columns.Add("Country", typeof(string));
            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedOn", typeof(DateTime));
            dt.Columns.Add("DoctorId", typeof(int));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("EmergencyContactId", typeof(int));
            dt.Columns.Add("Ethnicity", typeof(int));
            dt.Columns.Add("FullAddress", typeof(string));
            dt.Columns.Add("LastModifiedBy", typeof(string));
            dt.Columns.Add("LastModifiedOn", typeof(DateTime));
            dt.Columns.Add("LegalGuardianId", typeof(int));
            dt.Columns.Add("MaritalStatus", typeof(int));
            dt.Columns.Add("MedicaidID", typeof(string));
            dt.Columns.Add("OtherLanguage", typeof(string));
            dt.Columns.Add("PhotoPath", typeof(string));
            dt.Columns.Add("PreferredLanguage", typeof(int));
            dt.Columns.Add("PsychiatristId", typeof(int));
            dt.Columns.Add("Race", typeof(int));
            dt.Columns.Add("ReferredId", typeof(int));
            dt.Columns.Add("SSN", typeof(string));
            dt.Columns.Add("SignPath", typeof(string));
            dt.Columns.Add("State", typeof(string));
            dt.Columns.Add("Telephone", typeof(string));
            dt.Columns.Add("TelephoneSecondary", typeof(string));
            dt.Columns.Add("RelationShipOfLegalGuardian", typeof(int));
            dt.Columns.Add("Service", typeof(int));
            dt.Columns.Add("IndividualTherapyFacilitatorId", typeof(int));
            dt.Columns.Add("ZipCode", typeof(string));
            dt.Columns.Add("AdmisionDate", typeof(DateTime));
            dt.Columns.Add("PlaceOfBirth", typeof(string));
            dt.Columns.Add("RelationShipOfEmergencyContact", typeof(int));

            if (client != null)
            {
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
                                            0,
                                            client.AlternativeAddress,
                                            client.City,
                                            client.Country,
                                            client.CreatedBy,
                                            client.CreatedOn,
                                            0,
                                            client.Email,
                                            0,
                                            client.Ethnicity,
                                            client.FullAddress,
                                            client.LastModifiedBy,
                                            client.LastModifiedOn,
                                            0,
                                            client.MaritalStatus,
                                            client.MedicaidID,
                                            client.OtherLanguage,
                                            client.PhotoPath,
                                            client.PreferredLanguage,
                                            0,
                                            client.Race,
                                            0,
                                            client.SSN,
                                            client.SignPath,
                                            client.State,
                                            client.Telephone,
                                            client.TelephoneSecondary,
                                            client.RelationShipOfLegalGuardian,
                                            client.Service,
                                            0,
                                            client.ZipCode,
                                            client.AdmisionDate,
                                            client.PlaceOfBirth,
                                            client.RelationShipOfEmergencyContact
            }) ;
            }
            else
            {
                dt.Rows.Add(new object[]
                                            {
                                            0,
                                            string.Empty,
                                            Common.Enums.GenderType.Female,
                                            string.Empty,
                                            0,
                                            new DateTime(),
                                            string.Empty,
                                            Common.Enums.StatusType.Close,
                                            0,
                                            string.Empty,
                                            string.Empty,

                                            string.Empty,
                                            string.Empty,
                                            new DateTime(),
                                            0,
                                            string.Empty,
                                            0,
                                            Common.Enums.EthnicityType.HispanicLatino,
                                            string.Empty,
                                            string.Empty,
                                            new DateTime(),
                                            0,
                                            Common.Enums.MaritalStatus.Single,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            Common.Enums.PreferredLanguage.English,
                                            0,
                                            Common.Enums.RaceType.Black,
                                            0,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            Common.Enums.RelationshipType.Brother,
                                            Common.Enums.ServiceType.Group,
                                            0,
                                            string.Empty,
                                            new DateTime(),
                                            string.Empty,
                                            0,
                                            Common.Enums.RelationshipType.Brother
                                            });
            }

            return dt;
        }

        private DataTable GetEmergencyContactDS(EmergencyContactEntity contact)
        {
            DataTable dt = new DataTable
            {
                TableName = "EmergencyContacts"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedOn", typeof(DateTime));
            dt.Columns.Add("LastModifiedBy", typeof(string));
            dt.Columns.Add("LastModifiedOn", typeof(DateTime));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Address", typeof(string));
            dt.Columns.Add("Telephone", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Country", typeof(string));
            dt.Columns.Add("City", typeof(string));
            dt.Columns.Add("State", typeof(string));
            dt.Columns.Add("ZipCode", typeof(string));
            dt.Columns.Add("TelephoneSecondary", typeof(string));
            dt.Columns.Add("AddressLine2", typeof(string));           

            if (contact != null)
            {
                dt.Rows.Add(new object[]
                                            {
                                            contact.Id,
                                            contact.CreatedBy,
                                            contact.CreatedOn,
                                            contact.LastModifiedBy,
                                            contact.LastModifiedOn,
                                            contact.Name,
                                            contact.Address,
                                            contact.Telephone,
                                            contact.Email,
                                            contact.Country,
                                            contact.City,
                                            contact.State,
                                            contact.ZipCode,
                                            contact.TelephoneSecondary,
                                            contact.AdressLine2
            });
            }
            else
            {
                dt.Rows.Add(new object[]
                                            {
                                            0,
                                            string.Empty,
                                            new DateTime(),
                                            string.Empty,
                                            new DateTime(),                                            
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,                                            
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty
                                            });
            }

            return dt;
        }

        private DataTable GetLegalGuardianDS(LegalGuardianEntity legal)
        {
            DataTable dt = new DataTable
            {
                TableName = "LegalGuardians"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("CreatedBy", typeof(string));
            dt.Columns.Add("CreatedOn", typeof(DateTime));
            dt.Columns.Add("LastModifiedBy", typeof(string));
            dt.Columns.Add("LastModifiedOn", typeof(DateTime));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Address", typeof(string));
            dt.Columns.Add("Telephone", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Country", typeof(string));
            dt.Columns.Add("City", typeof(string));
            dt.Columns.Add("State", typeof(string));
            dt.Columns.Add("ZipCode", typeof(string));
            dt.Columns.Add("TelephoneSecondary", typeof(string));
            dt.Columns.Add("AddressLine2", typeof(string));

            if (legal != null)
            {
                dt.Rows.Add(new object[]
                                            {
                                            legal.Id,
                                            legal.CreatedBy,
                                            legal.CreatedOn,
                                            legal.LastModifiedBy,
                                            legal.LastModifiedOn,
                                            legal.Name,
                                            legal.Address,
                                            legal.Telephone,
                                            legal.Email,
                                            legal.Country,
                                            legal.City,
                                            legal.State,
                                            legal.ZipCode,
                                            legal.TelephoneSecondary,
                                            legal.AdressLine2
            });
            }
            else
            {
                dt.Rows.Add(new object[]
                                            {
                                            0,
                                            string.Empty,
                                            new DateTime(),
                                            string.Empty,
                                            new DateTime(),
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty
                                            });
            }

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
            dt.Columns.Add("Address", typeof(string));
            dt.Columns.Add("CEO", typeof(string));
            dt.Columns.Add("City", typeof(string));
            dt.Columns.Add("FaxNo", typeof(string));
            dt.Columns.Add("Phone", typeof(string));
            dt.Columns.Add("State", typeof(string));
            dt.Columns.Add("ZipCode", typeof(string));

            dt.Rows.Add(new object[]
                                        {
                                            clinic.Id,
                                            clinic.Name,
                                            clinic.LogoPath,
                                            clinic.Schema,
                                            clinic.Address,
                                            clinic.CEO,
                                            clinic.City,
                                            clinic.FaxNo,
                                            clinic.Phone,
                                            clinic.State,
                                            clinic.ZipCode
                                        });

            return dt;
        }

        private DataTable GetIntakeScreeningsDS(IntakeScreeningEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeScreening"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("InformationGatheredBy", typeof(string));            
            dt.Columns.Add("DateSignatureClient", typeof(DateTime));
            dt.Columns.Add("DateSignatureWitness", typeof(DateTime));
            dt.Columns.Add("ClientIsStatus", typeof(int));
            dt.Columns.Add("BehaviorIsStatus", typeof(int));
            dt.Columns.Add("SpeechIsStatus", typeof(int));
            dt.Columns.Add("DoesClientKnowHisName", typeof(bool));
            dt.Columns.Add("DoesClientKnowTodayDate", typeof(bool));
            dt.Columns.Add("DoesClientKnowWhereIs", typeof(bool));
            dt.Columns.Add("DoesClientKnowTimeOfDay", typeof(bool));
            dt.Columns.Add("DateDischarge", typeof(DateTime));

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.InformationGatheredBy,                                            
                                            intake.DateSignatureClient,
                                            intake.DateSignatureWitness,
                                            intake.ClientIsStatus,
                                            intake.BehaviorIsStatus,
                                            intake.SpeechIsStatus,
                                            intake.DoesClientKnowHisName,
                                            intake.DoesClientKnowTodayDate,
                                            intake.DoesClientKnowWhereIs,
                                            intake.DoesClientKnowTimeOfDay,
                                            intake.DateDischarge
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            string.Empty,                                            
                                            new DateTime(),
                                            new DateTime(),
                                            0,
                                            0,
                                            0,
                                            false,
                                            false,
                                            false,
                                            false,
                                            new DateTime()                                            
                                       });
            }

            return dt;
        }

        private DataTable GetIntakeConsentForTreatmentDS(IntakeConsentForTreatmentEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeConsentForTreatment"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateSignatureLegalGuardian", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));            
            dt.Columns.Add("AuthorizeStaff", typeof(bool));
            dt.Columns.Add("AuthorizeRelease", typeof(bool));
            dt.Columns.Add("Underestand", typeof(bool));
            dt.Columns.Add("Aggre", typeof(bool));
            dt.Columns.Add("Aggre1", typeof(bool));
            dt.Columns.Add("Certify", typeof(bool));
            dt.Columns.Add("Certify1", typeof(bool));
            dt.Columns.Add("Documents", typeof(bool));

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.DateSignatureLegalGuardian,
                                            intake.DateSignaturePerson,
                                            intake.DateSignatureEmployee,
                                            intake.AuthorizeStaff,
                                            intake.AuthorizeRelease,
                                            intake.Underestand,
                                            intake.Aggre,
                                            intake.Aggre1,
                                            intake.Certify,
                                            intake.Certify1,
                                            intake.Documents
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,                                            
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),                                            
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false
                                       });
            }

            return dt;
        }

        private DataTable GetIntakeConsentForReleaseDS(IntakeConsentForReleaseEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeConsentForRelease"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateSignatureLegalGuardian", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));
            dt.Columns.Add("ToRelease", typeof(bool));
            dt.Columns.Add("Discaherge", typeof(bool));
            dt.Columns.Add("SchoolRecord", typeof(bool));
            dt.Columns.Add("ProgressReports", typeof(bool));
            dt.Columns.Add("IncidentReport", typeof(bool));
            dt.Columns.Add("PsychologycalEvaluation", typeof(bool));
            dt.Columns.Add("History", typeof(bool));
            dt.Columns.Add("LabWork", typeof(bool));
            dt.Columns.Add("HospitalRecord", typeof(bool));
            dt.Columns.Add("Other", typeof(bool));
            dt.Columns.Add("Other_Explain", typeof(string));
            dt.Columns.Add("Documents", typeof(bool));
            dt.Columns.Add("ForPurpose_CaseManagement", typeof(bool));
            dt.Columns.Add("ForPurpose_Other", typeof(bool));
            dt.Columns.Add("ForPurpose_OtherExplain", typeof(string));
            dt.Columns.Add("ForPurpose_Treatment", typeof(bool));
            dt.Columns.Add("InForm_Facsimile", typeof(bool));
            dt.Columns.Add("InForm_VerbalInformation", typeof(bool));
            dt.Columns.Add("InForm_WrittenRecords", typeof(bool));

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.DateSignatureLegalGuardian,
                                            intake.DateSignaturePerson,
                                            intake.DateSignatureEmployee,
                                            intake.ToRelease,
                                            intake.Discaherge,
                                            intake.SchoolRecord,
                                            intake.ProgressReports,
                                            intake.IncidentReport,
                                            intake.PsychologycalEvaluation,
                                            intake.History,
                                            intake.LabWork,
                                            intake.HospitalRecord,
                                            intake.Other,
                                            intake.Other_Explain,
                                            intake.Documents,
                                            intake.ForPurpose_CaseManagement,
                                            intake.ForPurpose_Other,
                                            intake.ForPurpose_OtherExplain,
                                            intake.ForPurpose_Treatment,
                                            intake.InForm_Facsimile,
                                            intake.InForm_VerbalInformation,
                                            intake.InForm_WrittenRecords
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,                                            
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            string.Empty,
                                            false,
                                            false,
                                            false,
                                            string.Empty,
                                            false,
                                            false,
                                            false,
                                            false
                                        });
            }
            

            return dt;
        }

        private DataTable GetIntakeConsumerRightsDS(IntakeConsumerRightsEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeConsumerRights"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateSignatureLegalGuardian", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));
            dt.Columns.Add("Documents", typeof(bool));
            dt.Columns.Add("ServedOf", typeof(string));            

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.DateSignatureLegalGuardian,
                                            intake.DateSignaturePerson,
                                            intake.DateSignatureEmployee,
                                            intake.Documents,
                                            intake.ServedOf
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),
                                            false,                                            
                                            string.Empty                                            
                                        });
            }


            return dt;
        }

        private DataTable GetIntakeAcknowledgementDS(IntakeAcknowledgementHippaEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeAcknowledgement"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateSignatureLegalGuardian", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));
            dt.Columns.Add("Documents", typeof(bool));           

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.DateSignatureLegalGuardian,
                                            intake.DateSignaturePerson,
                                            intake.DateSignatureEmployee,
                                            intake.Documents
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),
                                            false
                                        });
            }


            return dt;
        }

        private DataTable GetIntakeAccessToServicesDS(IntakeAccessToServicesEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeAccessToServices"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateSignatureLegalGuardian", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));            
            dt.Columns.Add("Documents", typeof(bool));            

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.DateSignatureLegalGuardian,
                                            intake.DateSignaturePerson,
                                            intake.DateSignatureEmployee,                                            
                                            intake.Documents
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),
                                            false                                            
                                        });
            }


            return dt;
        }

        private DataTable GetIntakeOrientationCheckListDS(IntakeOrientationChecklistEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeOrientationCheckList"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateSignatureLegalGuardian", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));
            dt.Columns.Add("TourFacility", typeof(bool));
            dt.Columns.Add("Rights", typeof(bool));
            dt.Columns.Add("PoliceGrievancce", typeof(bool));
            dt.Columns.Add("Insent", typeof(bool));
            dt.Columns.Add("Services", typeof(bool));
            dt.Columns.Add("Access", typeof(bool));
            dt.Columns.Add("Code", typeof(bool));
            dt.Columns.Add("Confidentiality", typeof(bool));
            dt.Columns.Add("Methods", typeof(bool));
            dt.Columns.Add("Explanation", typeof(bool));
            dt.Columns.Add("Fire", typeof(bool));
            dt.Columns.Add("PoliceTobacco", typeof(bool));
            dt.Columns.Add("PoliceIllicit", typeof(bool));
            dt.Columns.Add("PoliceWeapons", typeof(bool));
            dt.Columns.Add("Identification", typeof(bool));
            dt.Columns.Add("Program", typeof(bool));
            dt.Columns.Add("Purpose", typeof(bool));
            dt.Columns.Add("IndividualPlan", typeof(bool));
            dt.Columns.Add("Discharge", typeof(bool));
            dt.Columns.Add("AgencyPolice", typeof(bool));
            dt.Columns.Add("AgencyExpectation", typeof(bool));
            dt.Columns.Add("Education", typeof(bool));
            dt.Columns.Add("TheAbove", typeof(bool));
            dt.Columns.Add("Documents", typeof(bool));

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.DateSignatureLegalGuardian,
                                            intake.DateSignaturePerson,
                                            intake.DateSignatureEmployee,
                                            intake.TourFacility,
                                            intake.Rights,
                                            intake.PoliceGrievancce,
                                            intake.Insent,
                                            intake.Services,
                                            intake.Access,
                                            intake.Code,
                                            intake.Confidentiality,
                                            intake.Methods,
                                            intake.Explanation,
                                            intake.Fire,
                                            intake.PoliceTobacco,
                                            intake.PoliceIllicit,
                                            intake.PoliceWeapons,
                                            intake.Identification,
                                            intake.Program,
                                            intake.Purpose,
                                            intake.IndividualPlan,
                                            intake.Discharge,
                                            intake.AgencyPolice,
                                            intake.AgencyExpectation,
                                            intake.Education,
                                            intake.TheAbove,
                                            intake.Documents
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,                                           
                                            false,
                                            false,
                                            false,                                            
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false
                                        });
            }


            return dt;
        }

        private DataTable GetIntakeFeeAgreementDS(IntakeFeeAgreementEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeFeeAgreement"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateSignatureLegalGuardian", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));
            dt.Columns.Add("Documents", typeof(bool));

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.DateSignatureLegalGuardian,
                                            intake.DateSignaturePerson,
                                            intake.DateSignatureEmployee,
                                            intake.Documents
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),
                                            false
                                        });
            }


            return dt;
        }

        private DataTable GetIntakeTuberculosisDS(IntakeTuberculosisEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeTuberculosis"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateSignatureLegalGuardian", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));
            dt.Columns.Add("DoYouCurrently", typeof(bool));
            dt.Columns.Add("DoYouBring", typeof(bool));
            dt.Columns.Add("DoYouCough", typeof(bool));
            dt.Columns.Add("DoYouSweat", typeof(bool));
            dt.Columns.Add("DoYouHaveFever", typeof(bool));
            dt.Columns.Add("HaveYouLost", typeof(bool));
            dt.Columns.Add("DoYouHaveChest", typeof(bool));
            dt.Columns.Add("If2OrMore", typeof(bool));
            dt.Columns.Add("HaveYouRecently", typeof(bool));
            dt.Columns.Add("AreYouRecently", typeof(bool));
            dt.Columns.Add("IfYesWhich", typeof(bool));
            dt.Columns.Add("DoYouOr", typeof(bool));
            dt.Columns.Add("HaveYouEverBeen", typeof(bool));
            dt.Columns.Add("HaveYouEverWorked", typeof(bool));
            dt.Columns.Add("HaveYouEverHadOrgan", typeof(bool));
            dt.Columns.Add("HaveYouEverConsidered", typeof(bool));
            dt.Columns.Add("HaveYouEverHadAbnormal", typeof(bool));
            dt.Columns.Add("If3OrMore", typeof(bool));
            dt.Columns.Add("HaveYouEverHadPositive", typeof(bool));
            dt.Columns.Add("IfYesWhere", typeof(string));
            dt.Columns.Add("When", typeof(string));
            dt.Columns.Add("HaveYoyEverBeenTold", typeof(bool));
            dt.Columns.Add("AgencyExpectation", typeof(bool));
            dt.Columns.Add("If1OrMore", typeof(bool));
            dt.Columns.Add("Documents", typeof(bool));           

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.DateSignatureLegalGuardian,
                                            intake.DateSignaturePerson,
                                            intake.DateSignatureEmployee,
                                            intake.DoYouCurrently,
                                            intake.DoYouBring,
                                            intake.DoYouCough,
                                            intake.DoYouSweat,
                                            intake.DoYouHaveFever,
                                            intake.HaveYouLost,
                                            intake.DoYouHaveChest,
                                            intake.If2OrMore,
                                            intake.HaveYouRecently,
                                            intake.AreYouRecently,
                                            intake.IfYesWhich,
                                            intake.DoYouOr,
                                            intake.HaveYouEverBeen,
                                            intake.HaveYouEverWorked,
                                            intake.HaveYouEverHadOrgan,
                                            intake.HaveYouEverConsidered,
                                            intake.HaveYouEverHadAbnormal,
                                            intake.If3OrMore,
                                            intake.HaveYouEverHadPositive,
                                            intake.IfYesWhere,
                                            intake.When,
                                            intake.HaveYoyEverBeenTold,
                                            intake.AgencyExpectation,
                                            intake.If1OrMore,
                                            intake.Documents
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            string.Empty,
                                            string.Empty,
                                            false,
                                            false,
                                            false,                                           
                                            false
                                        });
            }


            return dt;
        }

        private DataTable GetIntakeTransportationDS(IntakeTransportationEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeTransportation"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateSignatureLegalGuardian", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));
            dt.Columns.Add("Documents", typeof(bool));

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.DateSignatureLegalGuardian,
                                            intake.DateSignaturePerson,
                                            intake.DateSignatureEmployee,
                                            intake.Documents
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),
                                            false
                                        });
            }


            return dt;
        }

        private DataTable GetIntakeConsentPhotographDS(IntakeConsentPhotographEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeConsentPhotograph"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateSignatureLegalGuardian", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));
            dt.Columns.Add("Photograph", typeof(bool));
            dt.Columns.Add("Filmed", typeof(bool));
            dt.Columns.Add("VideoTaped", typeof(bool));
            dt.Columns.Add("Interviwed", typeof(bool));
            dt.Columns.Add("NoneOfTheForegoing", typeof(bool));
            dt.Columns.Add("Publication", typeof(bool));
            dt.Columns.Add("Broadcast", typeof(bool));
            dt.Columns.Add("Markrting", typeof(bool));
            dt.Columns.Add("ByTODocument", typeof(bool));
            dt.Columns.Add("Documents", typeof(bool));            
            dt.Columns.Add("Other", typeof(string));
            

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.DateSignatureLegalGuardian,
                                            intake.DateSignaturePerson,
                                            intake.DateSignatureEmployee,
                                            intake.Photograph,
                                            intake.Filmed,
                                            intake.VideoTaped,
                                            intake.Interviwed,
                                            intake.NoneOfTheForegoing,
                                            intake.Publication,
                                            intake.Broadcast,
                                            intake.Markrting,
                                            intake.ByTODocument,
                                            intake.Documents,
                                            intake.Other
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            string.Empty                                            
                                        });
            }


            return dt;
        }
        
        private DataTable GetIntakeMedicalHistoryDS(IntakeMedicalHistoryEntity intake)
        {
            DataTable dt = new DataTable
            {
                TableName = "IntakeMedicalHistory"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateSignatureLegalGuardian", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));
            dt.Columns.Add("PrimaryCarePhysician", typeof(string));
            dt.Columns.Add("AddressPhysician", typeof(string));
            dt.Columns.Add("City", typeof(string));
            dt.Columns.Add("State", typeof(string));
            dt.Columns.Add("ZipCode", typeof(string));            
            dt.Columns.Add("Diphtheria", typeof(bool));
            dt.Columns.Add("Mumps", typeof(bool));
            dt.Columns.Add("Poliomyelitis", typeof(bool));
            dt.Columns.Add("RheumaticFever", typeof(bool));
            dt.Columns.Add("WhoopingCough", typeof(bool));
            dt.Columns.Add("Tuberculosis", typeof(bool));
            dt.Columns.Add("ScarletFever", typeof(bool));
            dt.Columns.Add("Hepatitis", typeof(bool));
            dt.Columns.Add("HighBloodPressure", typeof(bool));
            dt.Columns.Add("KidneyTrouble", typeof(bool));
            dt.Columns.Add("KidneyStones", typeof(bool));
            dt.Columns.Add("BloodInUrine", typeof(bool));
            dt.Columns.Add("BurningUrine", typeof(bool));
            dt.Columns.Add("PainfulUrination", typeof(bool));
            dt.Columns.Add("EyeTrouble", typeof(bool));
            dt.Columns.Add("HearingTrouble", typeof(bool));
            dt.Columns.Add("Fractures", typeof(bool));
            dt.Columns.Add("EarInfections", typeof(bool));
            dt.Columns.Add("FrequentNoseBleeds", typeof(bool));
            dt.Columns.Add("FrequentSoreThroat", typeof(bool));
            dt.Columns.Add("Hoarseness", typeof(bool));
            dt.Columns.Add("Allergies", typeof(bool));
            dt.Columns.Add("Allergies_Describe", typeof(string));         
            dt.Columns.Add("StomachPain", typeof(bool));
            dt.Columns.Add("BlackStools", typeof(bool));
            dt.Columns.Add("NightSweats", typeof(bool));
            dt.Columns.Add("FrequentVomiting", typeof(bool));
            dt.Columns.Add("SkinTrouble", typeof(bool));
            dt.Columns.Add("PainfulMuscles", typeof(bool));
            dt.Columns.Add("PainfulJoints", typeof(bool));
            dt.Columns.Add("BackPain", typeof(bool));
            dt.Columns.Add("SeriousInjury", typeof(bool));
            dt.Columns.Add("Surgery", typeof(bool));
            dt.Columns.Add("Arthritis", typeof(bool));
            dt.Columns.Add("Hemorrhoids", typeof(bool));
            dt.Columns.Add("WeightLoss", typeof(bool));
            dt.Columns.Add("FrequentHeadaches", typeof(bool));
            dt.Columns.Add("Fainting", typeof(bool));
            dt.Columns.Add("ConvulsionsOrFits", typeof(bool));
            dt.Columns.Add("LossOfMemory", typeof(bool));
            dt.Columns.Add("Nervousness", typeof(bool));
            dt.Columns.Add("ChronicCough", typeof(bool));
            dt.Columns.Add("CoughingOfBlood", typeof(bool));
            dt.Columns.Add("VenerealDisease", typeof(bool));
            dt.Columns.Add("FrequentColds", typeof(bool));
            dt.Columns.Add("HeartPalpitation", typeof(bool));
            dt.Columns.Add("ChestPain", typeof(bool));
            dt.Columns.Add("ShortnessOfBreath", typeof(bool));
            dt.Columns.Add("SwellingOfFeet", typeof(bool));
            dt.Columns.Add("SwollenAnkles", typeof(bool));
            dt.Columns.Add("ChronicIndigestion", typeof(bool));
            dt.Columns.Add("VomitingOfBlood", typeof(bool));
            dt.Columns.Add("Jaundice", typeof(bool));
            dt.Columns.Add("Constipation", typeof(bool));
            dt.Columns.Add("BloodyStools", typeof(bool));
            dt.Columns.Add("Cancer", typeof(bool));
            dt.Columns.Add("Diabetes", typeof(bool));
            dt.Columns.Add("HayFever", typeof(bool));
            dt.Columns.Add("Hernia", typeof(bool));
            dt.Columns.Add("HeadInjury", typeof(bool));
            dt.Columns.Add("Rheumatism", typeof(bool));
            dt.Columns.Add("Epilepsy", typeof(bool));
            dt.Columns.Add("VaricoseVeins", typeof(bool));
            dt.Columns.Add("Anemia", typeof(bool));
            dt.Columns.Add("InfectiousDisease", typeof(bool));
            dt.Columns.Add("FamilyDiabetes", typeof(bool));
            dt.Columns.Add("FamilyDiabetes_", typeof(string));
            dt.Columns.Add("FamilyCancer", typeof(bool));
            dt.Columns.Add("FamilyCancer_", typeof(string));
            dt.Columns.Add("FamilyTuberculosis", typeof(bool));
            dt.Columns.Add("FamilyTuberculosis_", typeof(string));
            dt.Columns.Add("FamilyHeartDisease", typeof(bool));
            dt.Columns.Add("FamilyHeartDisease_", typeof(string));
            dt.Columns.Add("FamilyKidneyDisease", typeof(bool));
            dt.Columns.Add("FamilyKidneyDisease_", typeof(string));
            dt.Columns.Add("FamilyHighBloodPressure", typeof(bool));
            dt.Columns.Add("FamilyHighBloodPressure_", typeof(string));
            dt.Columns.Add("FamilyHayFever", typeof(bool));
            dt.Columns.Add("FamilyHayFever_", typeof(string));
            dt.Columns.Add("FamilyAsthma", typeof(bool));
            dt.Columns.Add("FamilyAsthma_", typeof(string));
            dt.Columns.Add("FamilyEpilepsy", typeof(bool));
            dt.Columns.Add("FamilyEpilepsy_", typeof(string));
            dt.Columns.Add("FamilyGlaucoma", typeof(bool));
            dt.Columns.Add("FamilyGlaucoma_", typeof(string));
            dt.Columns.Add("FamilySyphilis", typeof(bool));
            dt.Columns.Add("FamilySyphilis_", typeof(string));
            dt.Columns.Add("FamilyNervousDisorders", typeof(bool));
            dt.Columns.Add("FamilyNervousDisorders_", typeof(string));
            dt.Columns.Add("FamilyOther", typeof(bool));
            dt.Columns.Add("FamilyOther_", typeof(string));               
            dt.Columns.Add("HaveYouEverBeenPregnant", typeof(bool));
            dt.Columns.Add("HaveYouEverHadComplications", typeof(bool));
            dt.Columns.Add("HaveYouEverHadPainful", typeof(bool));
            dt.Columns.Add("HaveYouEverHadExcessive", typeof(bool));
            dt.Columns.Add("HaveYouEverHadSpotting", typeof(bool));
            dt.Columns.Add("AreYouCurrently", typeof(bool));
            dt.Columns.Add("AreYouPhysician", typeof(bool));
            dt.Columns.Add("DoYouSmoke", typeof(bool));
            dt.Columns.Add("DoYouSmoke_PackPerDay", typeof(string)); 
            dt.Columns.Add("DoYouSmoke_Year", typeof(string)); 
            dt.Columns.Add("ListAllCurrentMedications", typeof(string));
            dt.Columns.Add("PerformingCertainMotions", typeof(bool));
            dt.Columns.Add("AssumingCertainPositions", typeof(bool));
            dt.Columns.Add("Hearing", typeof(bool));
            dt.Columns.Add("Seeing", typeof(bool));
            dt.Columns.Add("Speaking", typeof(bool));
            dt.Columns.Add("Reading", typeof(bool));
            dt.Columns.Add("Concentrating", typeof(bool));
            dt.Columns.Add("Comprehending", typeof(bool));
            dt.Columns.Add("BeingConfused", typeof(bool));
            dt.Columns.Add("BeingDisorientated", typeof(bool));
            dt.Columns.Add("Calculating", typeof(bool));
            dt.Columns.Add("WritingSentence", typeof(bool));
            dt.Columns.Add("Walking", typeof(bool));
            dt.Columns.Add("Planned", typeof(bool));
            dt.Columns.Add("Unplanned", typeof(bool));
            dt.Columns.Add("Normal", typeof(bool));
            dt.Columns.Add("Complications", typeof(bool));
            dt.Columns.Add("Complications_Explain", typeof(string));
            dt.Columns.Add("BirthWeight", typeof(string));
            dt.Columns.Add("Length", typeof(string));
            dt.Columns.Add("BreastFed", typeof(bool));
            dt.Columns.Add("BottleFedUntilAge", typeof(string));
            dt.Columns.Add("AgeWeaned", typeof(string));
            dt.Columns.Add("FirstYearMedical", typeof(string));
            dt.Columns.Add("AgeFirstWalked", typeof(string));
            dt.Columns.Add("AgeFirstTalked", typeof(string));
            dt.Columns.Add("AgeToiletTrained", typeof(string));
            dt.Columns.Add("DescriptionOfChild", typeof(string));
            dt.Columns.Add("ProblemWithBedWetting", typeof(bool));
            dt.Columns.Add("AndOrSoiling", typeof(bool));
            dt.Columns.Add("Immunizations", typeof(string));
            dt.Columns.Add("Documents", typeof(bool));
            dt.Columns.Add("AgeOfFirstMenstruation", typeof(string));
            dt.Columns.Add("DateOfLastBreastExam", typeof(DateTime));
            dt.Columns.Add("DateOfLastPelvic", typeof(DateTime));
            dt.Columns.Add("DateOfLastPeriod", typeof(DateTime));
            dt.Columns.Add("UsualDurationOfPeriods", typeof(string));
            dt.Columns.Add("UsualIntervalBetweenPeriods", typeof(string));
            dt.Columns.Add("InformationProvided", typeof(bool));

            if (intake != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            intake.Id,
                                            intake.Client_FK,
                                            intake.DateSignatureLegalGuardian,
                                            intake.DateSignaturePerson,
                                            intake.DateSignatureEmployee,
                                            intake.PrimaryCarePhysician,
                                            intake.AddressPhysician,
                                            intake.City,
                                            intake.State,
                                            intake.ZipCode,
                                            intake.Diphtheria,
                                            intake.Mumps,
                                            intake.Poliomyelitis,
                                            intake.RheumaticFever,
                                            intake.WhoopingCough,
                                            intake.Tuberculosis,
                                            intake.ScarletFever,
                                            intake.Hepatitis,
                                            intake.HighBloodPressure,
                                            intake.KidneyTrouble,
                                            intake.KidneyStones,
                                            intake.BloodInUrine,
                                            intake.BurningUrine,
                                            intake.PainfulUrination,
                                            intake.EyeTrouble,
                                            intake.HearingTrouble,
                                            intake.Fractures,
                                            intake.EarInfections,
                                            intake.FrequentNoseBleeds,
                                            intake.FrequentSoreThroat,
                                            intake.Hoarseness,
                                            intake.Allergies,
                                            intake.Allergies_Describe,
                                            intake.StomachPain,
                                            intake.BlackStools,
                                            intake.NightSweats,
                                            intake.FrequentVomiting,
                                            intake.SkinTrouble,
                                            intake.PainfulMuscles,
                                            intake.PainfulJoints,
                                            intake.BackPain,
                                            intake.SeriousInjury,
                                            intake.Surgery,
                                            intake.Arthritis,
                                            intake.Hemorrhoids,
                                            intake.WeightLoss,
                                            intake.FrequentHeadaches,
                                            intake.Fainting,
                                            intake.ConvulsionsOrFits,
                                            intake.LossOfMemory,
                                            intake.Nervousness,
                                            intake.ChronicCough,
                                            intake.CoughingOfBlood,
                                            intake.VenerealDisease,
                                            intake.FrequentColds,
                                            intake.HeartPalpitation,
                                            intake.ChestPain,
                                            intake.ShortnessOfBreath,
                                            intake.SwellingOfFeet,
                                            intake.SwollenAnkles,
                                            intake.ChronicIndigestion,
                                            intake.VomitingOfBlood,
                                            intake.Jaundice,
                                            intake.Constipation,
                                            intake.BloodyStools,
                                            intake.Cancer,
                                            intake.Diabetes,
                                            intake.HayFever,
                                            intake.Hernia,
                                            intake.HeadInjury,
                                            intake.Rheumatism,
                                            intake.Epilepsy,
                                            intake.VaricoseVeins,
                                            intake.Anemia,
                                            intake.InfectiousDisease,
                                            intake.FamilyDiabetes,
                                            intake.FamilyDiabetes_,
                                            intake.FamilyCancer,
                                            intake.FamilyCancer_,
                                            intake.FamilyTuberculosis,
                                            intake.FamilyTuberculosis_,
                                            intake.FamilyHeartDisease,
                                            intake.FamilyHeartDisease_,
                                            intake.FamilyKidneyDisease,
                                            intake.FamilyKidneyDisease_,
                                            intake.FamilyHighBloodPressure,
                                            intake.FamilyHighBloodPressure_,
                                            intake.FamilyHayFever,
                                            intake.FamilyHayFever_,
                                            intake.FamilyAsthma,
                                            intake.FamilyAsthma_,
                                            intake.FamilyEpilepsy,
                                            intake.FamilyEpilepsy_,
                                            intake.FamilyGlaucoma,
                                            intake.FamilyGlaucoma_,
                                            intake.FamilySyphilis,
                                            intake.FamilySyphilis_,
                                            intake.FamilyNervousDisorders,
                                            intake.FamilyNervousDisorders_,
                                            intake.FamilyOther,
                                            intake.FamilyOther_,
                                            intake.HaveYouEverBeenPregnant,
                                            intake.HaveYouEverHadComplications,
                                            intake.HaveYouEverHadPainful,
                                            intake.HaveYouEverHadExcessive,
                                            intake.HaveYouEverHadSpotting,
                                            intake.AreYouCurrently,
                                            intake.AreYouPhysician,
                                            intake.DoYouSmoke,
                                            intake.DoYouSmoke_PackPerDay,
                                            intake.DoYouSmoke_Year,
                                            intake.ListAllCurrentMedications,
                                            intake.PerformingCertainMotions,
                                            intake.AssumingCertainPositions,
                                            intake.Hearing,
                                            intake.Seeing,
                                            intake.Speaking,
                                            intake.Reading,
                                            intake.Concentrating,
                                            intake.Comprehending,
                                            intake.BeingConfused,
                                            intake.BeingDisorientated,
                                            intake.Calculating,
                                            intake.WritingSentence,
                                            intake.Walking,
                                            intake.Planned,
                                            intake.Unplanned,
                                            intake.Normal,
                                            intake.Complications,
                                            intake.Complications_Explain,
                                            intake.BirthWeight,
                                            intake.Length,
                                            intake.BreastFed,
                                            intake.BottleFedUntilAge,
                                            intake.AgeWeaned,
                                            intake.FirstYearMedical,
                                            intake.AgeFirstWalked,
                                            intake.AgeFirstTalked,
                                            intake.AgeToiletTrained,
                                            intake.DescriptionOfChild,
                                            intake.ProblemWithBedWetting,
                                            intake.AndOrSoiling,
                                            intake.Immunizations,
                                            intake.Documents,
                                            intake.AgeOfFirstMenstruation,
                                            intake.DateOfLastBreastExam,
                                            intake.DateOfLastPelvic,
                                            intake.DateOfLastPeriod,
                                            intake.UsualDurationOfPeriods,
                                            intake.UsualIntervalBetweenPeriods,
                                            intake.InformationProvided
            });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            string.Empty,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,                                             
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            false,
                                            false,
                                            string.Empty,
                                            false,
                                            string.Empty,
                                            new DateTime(),
                                            new DateTime(),
                                            new DateTime(),
                                            string.Empty,
                                            string.Empty,
                                            false
                                        });
            }

            return dt;
        }

        private DataTable GetFarsDS(FarsFormEntity fars)
        {
            DataTable dt = new DataTable
            {
                TableName = "FarsForm"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("ClientId", typeof(int));
            dt.Columns.Add("ContractorID", typeof(string));
            dt.Columns.Add("DcfEvaluation", typeof(string));
            dt.Columns.Add("EvaluationDate", typeof(DateTime));
            dt.Columns.Add("ProviderId", typeof(string));
            dt.Columns.Add("M_GafScore", typeof(string));
            dt.Columns.Add("RaterEducation", typeof(string));
            dt.Columns.Add("RaterFMHI", typeof(string));
            dt.Columns.Add("SubstanceAbusoHistory", typeof(int));
            dt.Columns.Add("DepressionScale", typeof(int));
            dt.Columns.Add("AnxietyScale", typeof(int));
            dt.Columns.Add("HyperAffectScale", typeof(int));
            dt.Columns.Add("ThoughtProcessScale", typeof(int));
            dt.Columns.Add("CognitiveScale", typeof(int));
            dt.Columns.Add("MedicalScale", typeof(int));
            dt.Columns.Add("TraumaticsScale", typeof(int));
            dt.Columns.Add("SubstanceScale", typeof(int));
            dt.Columns.Add("InterpersonalScale", typeof(int));
            dt.Columns.Add("FamilyRelationShipsScale", typeof(int));
            dt.Columns.Add("FamilyEnvironmentScale", typeof(int));
            dt.Columns.Add("SocialScale", typeof(int));
            dt.Columns.Add("WorkScale", typeof(int));
            dt.Columns.Add("ActivitiesScale", typeof(int));
            dt.Columns.Add("AbilityScale", typeof(int));
            dt.Columns.Add("DangerToSelfScale", typeof(int));
            dt.Columns.Add("DangerToOtherScale", typeof(int));
            dt.Columns.Add("SecurityScale", typeof(int));
            dt.Columns.Add("ContID1", typeof(string));
            dt.Columns.Add("ContID2", typeof(string));
            dt.Columns.Add("ContID3", typeof(string));
            dt.Columns.Add("ProviderLocal", typeof(string));
            dt.Columns.Add("MedicaidRecipientID", typeof(string));
            dt.Columns.Add("MedicaidProviderID", typeof(string));
            dt.Columns.Add("MCOID", typeof(string));
            dt.Columns.Add("Country", typeof(string));
            dt.Columns.Add("SignatureDate", typeof(DateTime));
            dt.Columns.Add("AdmissionedFor", typeof(string));
            dt.Columns.Add("ProgramEvaluation", typeof(string));            

            if (fars != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            fars.Id,
                                            fars.Client.Id,
                                            fars.ContractorID,
                                            fars.DcfEvaluation,
                                            fars.EvaluationDate,
                                            fars.ProviderId,
                                            fars.M_GafScore,
                                            fars.RaterEducation,
                                            fars.RaterFMHI,
                                            fars.SubstanceAbusoHistory,
                                            fars.DepressionScale,
                                            fars.AnxietyScale,
                                            fars.HyperAffectScale,
                                            fars.ThoughtProcessScale,
                                            fars.CognitiveScale,
                                            fars.MedicalScale,
                                            fars.TraumaticsScale,
                                            fars.SubstanceScale,
                                            fars.InterpersonalScale,
                                            fars.FamilyRelationShipsScale,
                                            fars.FamilyEnvironmentScale,
                                            fars.SocialScale,
                                            fars.WorkScale,
                                            fars.ActivitiesScale,
                                            fars.AbilityScale,
                                            fars.DangerToSelfScale,
                                            fars.DangerToOtherScale,
                                            fars.SecurityScale,
                                            fars.ContID1,
                                            fars.ContID2,
                                            fars.ContID3,
                                            fars.ProviderLocal,
                                            fars.MedicaidRecipientID,
                                            fars.MedicaidProviderID,
                                            fars.MCOID,
                                            fars.Country,
                                            fars.SignatureDate,
                                            fars.AdmissionedFor,
                                            fars.ProgramEvaluation,
                                        });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            string.Empty,
                                            string.Empty,
                                            new DateTime(),
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            0,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            new DateTime(),
                                            string.Empty,
                                            string.Empty
                                        });
            }

            return dt;
        }

        private DataTable GetDischargeDS(DischargeEntity discharge)
        {
            DataTable dt = new DataTable
            {
                TableName = "Discharge"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Client_FK", typeof(int));
            dt.Columns.Add("DateReport", typeof(DateTime));
            dt.Columns.Add("DateDischarge", typeof(DateTime));            
            dt.Columns.Add("Planned", typeof(bool));            
            dt.Columns.Add("ReasonDischarge", typeof(string));
            dt.Columns.Add("BriefHistory", typeof(string));
            dt.Columns.Add("CourseTreatment", typeof(string));
            dt.Columns.Add("ConditionalDischarge", typeof(string));
            dt.Columns.Add("ReferralFor1", typeof(string));
            dt.Columns.Add("ReferralAgency1", typeof(string));
            dt.Columns.Add("ReferralContactPersonal1", typeof(string));
            dt.Columns.Add("ReferralPhone1", typeof(string));
            dt.Columns.Add("ReferralHoursOperation1", typeof(string));
            dt.Columns.Add("ReferralFor2", typeof(string));
            dt.Columns.Add("ReferralAgency2", typeof(string));
            dt.Columns.Add("ReferralContactPersonal2", typeof(string));
            dt.Columns.Add("ReferralPhone2", typeof(string));
            dt.Columns.Add("ReferralHoursOperation2", typeof(string));
            dt.Columns.Add("FollowDischarge", typeof(string));
            dt.Columns.Add("TreatmentPlanObjCumpl", typeof(bool));
            dt.Columns.Add("AgencyDischargeClient", typeof(bool));
            dt.Columns.Add("ClientDischargeAgainst", typeof(bool));
            dt.Columns.Add("ClientDeceased", typeof(bool));
            dt.Columns.Add("ClientMoved", typeof(bool));
            dt.Columns.Add("PhysicallyUnstable", typeof(bool));
            dt.Columns.Add("ClientReferred", typeof(bool));            
            dt.Columns.Add("AdmissionedFor", typeof(string));
            dt.Columns.Add("Hospitalization", typeof(bool));
            dt.Columns.Add("Others", typeof(bool));
            dt.Columns.Add("DateSignatureEmployee", typeof(DateTime));
            dt.Columns.Add("DateSignaturePerson", typeof(DateTime));
            dt.Columns.Add("Others_Explain", typeof(string));
            dt.Columns.Add("DateSignatureSupervisor", typeof(DateTime));

            if (discharge != null)
            {
                dt.Rows.Add(new object[]
                                        {
                                            discharge.Id,
                                            discharge.Client_FK,
                                            discharge.DateReport,
                                            discharge.DateDischarge,
                                            discharge.Planned,
                                            discharge.ReasonDischarge,
                                            discharge.BriefHistory,
                                            discharge.CourseTreatment,
                                            discharge.ConditionalDischarge,
                                            discharge.ReferralFor1,
                                            discharge.ReferralAgency1,
                                            discharge.ReferralContactPersonal1,
                                            discharge.ReferralPhone1,
                                            discharge.ReferralHoursOperation1,
                                            discharge.ReferralFor2,
                                            discharge.ReferralAgency2,
                                            discharge.ReferralContactPersonal2,
                                            discharge.ReferralPhone2,
                                            discharge.ReferralHoursOperation2,
                                            discharge.FollowDischarge,
                                            discharge.TreatmentPlanObjCumpl,
                                            discharge.AgencyDischargeClient,
                                            discharge.ClientDischargeAgainst,
                                            discharge.ClientDeceased,
                                            discharge.ClientMoved,
                                            discharge.PhysicallyUnstable,
                                            discharge.ClientReferred,
                                            discharge.AdmissionedFor,
                                            discharge.Hospitalization,
                                            discharge.Others,
                                            discharge.DateSignatureEmployee,
                                            discharge.DateSignaturePerson,
                                            discharge.Others_Explain,
                                            discharge.DateSignatureSupervisor,
            });
            }
            else
            {
                dt.Rows.Add(new object[]
                                        {
                                            0,
                                            0,
                                            new DateTime(),
                                            new DateTime(),
                                            false,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            false,
                                            string.Empty,
                                            false,
                                            false,
                                            new DateTime(),
                                            new DateTime(),
                                            string.Empty,
                                            new DateTime()                                  
                                        });
            }

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

        private DataTable GetNotePDS(NotePEntity note)
        {
            DataTable dt = new DataTable
            {
                TableName = "NoteP"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Title", typeof(string));
            dt.Columns.Add("PlanNote", typeof(string));
            dt.Columns.Add("Status", typeof(int));
            dt.Columns.Add("Workday_Client_FK", typeof(int));
            dt.Columns.Add("DateOfApprove", typeof(DateTime));
            
            dt.Columns.Add("Attentive", typeof(bool));
            dt.Columns.Add("Depressed", typeof(bool));
            dt.Columns.Add("Inattentive", typeof(bool));
            dt.Columns.Add("Angry", typeof(bool));
            dt.Columns.Add("Sad", typeof(bool));
            dt.Columns.Add("FlatAffect", typeof(bool));
            dt.Columns.Add("Anxious", typeof(bool));
            dt.Columns.Add("PositiveEffect", typeof(bool));
            dt.Columns.Add("Oriented3x", typeof(bool));
            dt.Columns.Add("Oriented2x", typeof(bool));
            dt.Columns.Add("Oriented1x", typeof(bool));
            dt.Columns.Add("Impulsive", typeof(bool));
            dt.Columns.Add("Labile", typeof(bool));
            dt.Columns.Add("Withdrawn", typeof(bool));
            dt.Columns.Add("RelatesWell", typeof(bool));
            dt.Columns.Add("DecreasedEyeContact", typeof(bool));
            dt.Columns.Add("AppropiateEyeContact", typeof(bool));

            dt.Columns.Add("Minimal", typeof(bool));
            dt.Columns.Add("Slow", typeof(bool));
            dt.Columns.Add("Steady", typeof(bool));
            dt.Columns.Add("GoodExcelent", typeof(bool));
            dt.Columns.Add("IncreasedDifficultiesNoted", typeof(bool));
            dt.Columns.Add("Complicated", typeof(bool));
            dt.Columns.Add("DevelopingInsight", typeof(bool));
            dt.Columns.Add("LittleInsight", typeof(bool));
            dt.Columns.Add("Aware", typeof(bool));
            dt.Columns.Add("AbleToGenerateAlternatives", typeof(bool));
            dt.Columns.Add("Initiates", typeof(bool));
            dt.Columns.Add("ProblemSolved", typeof(bool));
            dt.Columns.Add("DemostratesEmpathy", typeof(bool));
            dt.Columns.Add("UsesSessions", typeof(bool));
            dt.Columns.Add("Variable", typeof(bool));

            dt.Columns.Add("Setting", typeof(string));
            dt.Columns.Add("MTPId", typeof(int));
            dt.Columns.Add("Schema", typeof(int));

            dt.Rows.Add(new object[]
                                        {
                                            note.Id,
                                            note.Title,
                                            note.PlanNote,
                                            note.Status,
                                            note.Workday_Client_FK,
                                            note.DateOfApprove,

                                            note.Attentive,
                                            note.Depressed,
                                            note.Inattentive,
                                            note.Angry,
                                            note.Sad,
                                            note.FlatAffect,
                                            note.Anxious,
                                            note.PositiveEffect,
                                            note.Oriented3x,
                                            note.Oriented2x,
                                            note.Oriented1x,
                                            note.Impulsive,
                                            note.Labile,
                                            note.Withdrawn,
                                            note.RelatesWell,
                                            note.DecreasedEyeContact,
                                            note.AppropiateEyeContact,

                                            note.Minimal,
                                            note.Slow,
                                            note.Steady,
                                            note.GoodExcelent,
                                            note.IncreasedDifficultiesNoted,
                                            note.Complicated,
                                            note.DevelopingInsight,
                                            note.LittleInsight,
                                            note.Aware,
                                            note.AbleToGenerateAlternatives,
                                            note.Initiates,
                                            note.ProblemSolved,
                                            note.DemostratesEmpathy,
                                            note.UsesSessions,
                                            note.Variable,

                                            note.Setting,
                                            note.MTPId,
                                            note.Schema
            });

            return dt;
        }

        private DataTable GetIndividualNoteDS(IndividualNoteEntity note)
        {
            DataTable dt = new DataTable
            {
                TableName = "IndividualNote"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("PlanNote", typeof(string));
            dt.Columns.Add("SubjectiveData", typeof(string));
            dt.Columns.Add("ObjectiveData", typeof(string));
            dt.Columns.Add("Assessment", typeof(string));
            dt.Columns.Add("Status", typeof(int));
            dt.Columns.Add("Workday_Client_FK", typeof(int));
            dt.Columns.Add("DateOfApprove", typeof(DateTime));
            
            dt.Columns.Add("Groomed", typeof(bool));
            dt.Columns.Add("Unkempt", typeof(bool));
            dt.Columns.Add("Disheveled", typeof(bool));
            dt.Columns.Add("Meticulous", typeof(bool));
            dt.Columns.Add("Overbuild", typeof(bool));
            dt.Columns.Add("Other", typeof(bool));
            dt.Columns.Add("Clear", typeof(bool));
            dt.Columns.Add("Pressured", typeof(bool));
            dt.Columns.Add("Slurred", typeof(bool));
            dt.Columns.Add("Slow", typeof(bool));
            dt.Columns.Add("Impaired", typeof(bool));
            dt.Columns.Add("Poverty", typeof(bool));
            dt.Columns.Add("Euthymic", typeof(bool));
            dt.Columns.Add("Depressed", typeof(bool));
            dt.Columns.Add("Anxious", typeof(bool));
            dt.Columns.Add("Fearful", typeof(bool));
            dt.Columns.Add("Irritable", typeof(bool));            
            dt.Columns.Add("Labile", typeof(bool));
            dt.Columns.Add("WNL", typeof(bool));
            dt.Columns.Add("Guarded", typeof(bool));
            dt.Columns.Add("Withdrawn", typeof(bool));
            dt.Columns.Add("Hostile", typeof(bool));
            dt.Columns.Add("Restless", typeof(bool));
            dt.Columns.Add("Impulsive", typeof(bool));
            dt.Columns.Add("WNL_Cognition", typeof(bool));
            dt.Columns.Add("Blocked", typeof(bool));
            dt.Columns.Add("Obsessive", typeof(bool));
            dt.Columns.Add("Paranoid", typeof(int));
            dt.Columns.Add("Scattered", typeof(bool));
            dt.Columns.Add("Psychotic", typeof(bool));            
            dt.Columns.Add("Exceptional", typeof(bool));
            dt.Columns.Add("Steady", typeof(bool));
            dt.Columns.Add("Slow_Progress", typeof(bool));
            dt.Columns.Add("Regressing", typeof(bool));
            dt.Columns.Add("Stable", typeof(bool));
            dt.Columns.Add("Maintain", typeof(bool));
            dt.Columns.Add("CBT", typeof(bool));
            dt.Columns.Add("Psychodynamic", typeof(bool));
            dt.Columns.Add("BehaviorModification", typeof(bool));
            dt.Columns.Add("Other_Intervention", typeof(bool));

            dt.Columns.Add("SupervisorId", typeof(int));
            dt.Columns.Add("ObjectiveId", typeof(int));
            dt.Columns.Add("MTPId", typeof(int));

            dt.Rows.Add(new object[]
                                        {
                                            note.Id,
                                            note.PlanNote,
                                            note.SubjectiveData,
                                            note.ObjectiveData,
                                            note.Assessment,
                                            note.Status,
                                            note.Workday_Client_FK,
                                            note.DateOfApprove,

                                            note.Groomed,
                                            note.Unkempt,
                                            note.Disheveled,
                                            note.Meticulous,
                                            note.Overbuild,
                                            note.Other,
                                            note.Clear,
                                            note.Pressured,
                                            note.Slurred,
                                            note.Slow,
                                            note.Impaired,
                                            note.Poverty,
                                            note.Euthymic,
                                            note.Depressed,
                                            note.Anxious,
                                            note.Fearful,
                                            note.Irritable,
                                            note.Labile,
                                            note.WNL,
                                            note.Guarded,
                                            note.Withdrawn,
                                            note.Hostile,
                                            note.Restless,
                                            note.Impulsive,
                                            note.WNL_Cognition,
                                            note.Blocked,
                                            note.Obsessive,
                                            note.Paranoid,
                                            note.Scattered,
                                            note.Psychotic,
                                            note.Exceptional,
                                            note.Steady,
                                            note.Slow_Progress,
                                            note.Regressing,
                                            note.Stable,
                                            note.Maintain,
                                            note.CBT,
                                            note.Psychodynamic,
                                            note.BehaviorModification,
                                            note.Other_Intervention,  
                                            note.Supervisor.Id,
                                            note.Objective.Id,
                                            note.MTPId
            });

            return dt;
        }

        private DataTable GetGroupNoteDS(GroupNoteEntity note)
        {
            DataTable dt = new DataTable
            {
                TableName = "GroupNote"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("PlanNote", typeof(string));
            dt.Columns.Add("Status", typeof(int));
            dt.Columns.Add("Workday_Client_FK", typeof(int));
            dt.Columns.Add("DateOfApprove", typeof(DateTime));

            dt.Columns.Add("Groomed", typeof(bool));
            dt.Columns.Add("Unkempt", typeof(bool));
            dt.Columns.Add("Disheveled", typeof(bool));
            dt.Columns.Add("Meticulous", typeof(bool));
            dt.Columns.Add("Overbuild", typeof(bool));
            dt.Columns.Add("Other", typeof(bool));
            dt.Columns.Add("Clear", typeof(bool));
            dt.Columns.Add("Pressured", typeof(bool));
            dt.Columns.Add("Slurred", typeof(bool));
            dt.Columns.Add("Slow", typeof(bool));
            dt.Columns.Add("Impaired", typeof(bool));
            dt.Columns.Add("Poverty", typeof(bool));
            dt.Columns.Add("Euthymic", typeof(bool));
            dt.Columns.Add("Depressed", typeof(bool));
            dt.Columns.Add("Anxious", typeof(bool));
            dt.Columns.Add("Fearful", typeof(bool));
            dt.Columns.Add("Irritable", typeof(bool));
            dt.Columns.Add("Labile", typeof(bool));
            dt.Columns.Add("WNL", typeof(bool));
            dt.Columns.Add("Guarded", typeof(bool));
            dt.Columns.Add("Withdrawn", typeof(bool));
            dt.Columns.Add("Hostile", typeof(bool));
            dt.Columns.Add("Restless", typeof(bool));
            dt.Columns.Add("Impulsive", typeof(bool));
            dt.Columns.Add("WNL_Cognition", typeof(bool));
            dt.Columns.Add("Blocked", typeof(bool));
            dt.Columns.Add("Obsessive", typeof(bool));
            dt.Columns.Add("Paranoid", typeof(int));
            dt.Columns.Add("Scattered", typeof(bool));
            dt.Columns.Add("Psychotic", typeof(bool));
            dt.Columns.Add("Exceptional", typeof(bool));
            dt.Columns.Add("Steady", typeof(bool));
            dt.Columns.Add("Slow_Progress", typeof(bool));
            dt.Columns.Add("Regressing", typeof(bool));
            dt.Columns.Add("Stable", typeof(bool));
            dt.Columns.Add("Maintain", typeof(bool));
            dt.Columns.Add("CBT", typeof(bool));
            dt.Columns.Add("Psychodynamic", typeof(bool));
            dt.Columns.Add("BehaviorModification", typeof(bool));
            dt.Columns.Add("Other_Intervention", typeof(bool));
            
            dt.Columns.Add("SupervisorId", typeof(int));            
            dt.Columns.Add("MTPId", typeof(int));

            dt.Rows.Add(new object[]
                                        {
                                            note.Id,
                                            note.PlanNote,                                            
                                            note.Status,
                                            note.Workday_Client_FK,
                                            note.DateOfApprove,

                                            note.Groomed,
                                            note.Unkempt,
                                            note.Disheveled,
                                            note.Meticulous,
                                            note.Overbuild,
                                            note.Other,
                                            note.Clear,
                                            note.Pressured,
                                            note.Slurred,
                                            note.Slow,
                                            note.Impaired,
                                            note.Poverty,
                                            note.Euthymic,
                                            note.Depressed,
                                            note.Anxious,
                                            note.Fearful,
                                            note.Irritable,
                                            note.Labile,
                                            note.WNL,
                                            note.Guarded,
                                            note.Withdrawn,
                                            note.Hostile,
                                            note.Restless,
                                            note.Impulsive,
                                            note.WNL_Cognition,
                                            note.Blocked,
                                            note.Obsessive,
                                            note.Paranoid,
                                            note.Scattered,
                                            note.Psychotic,
                                            note.Exceptional,
                                            note.Steady,
                                            note.Slow_Progress,
                                            note.Regressing,
                                            note.Stable,
                                            note.Maintain,
                                            note.CBT,
                                            note.Psychodynamic,
                                            note.BehaviorModification,
                                            note.Other_Intervention,

                                            note.Supervisor.Id,                                            
                                            note.MTPId
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
                                            (noteActivity.Objetive == null) ? 0 : noteActivity.Objetive.Id                                            
            });

            return dt;
        }

        private DataTable GetNotePActivityDS(NoteP_Activity noteActivity)
        {
            DataTable dt = new DataTable
            {
                TableName = "NotePActivity"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("NotePId", typeof(int));
            dt.Columns.Add("ActivityId", typeof(int));

            dt.Columns.Add("Cooperative", typeof(bool));
            dt.Columns.Add("Assertive", typeof(bool));
            dt.Columns.Add("Passive", typeof(bool));
            dt.Columns.Add("Variable", typeof(bool));
            dt.Columns.Add("Uninterested", typeof(bool));
            dt.Columns.Add("EngagedActive", typeof(bool));
            dt.Columns.Add("Distractible", typeof(bool));
            dt.Columns.Add("Confused", typeof(bool));
            dt.Columns.Add("Aggresive", typeof(bool));
            dt.Columns.Add("Resistant", typeof(bool));
            dt.Columns.Add("Other", typeof(bool));

            dt.Columns.Add("ObjetiveId", typeof(int));

            dt.Rows.Add(new object[]
                                        {
                                            noteActivity.Id,
                                            noteActivity.NoteP.Id,
                                            noteActivity.Activity.Id,

                                            noteActivity.Cooperative,
                                            noteActivity.Assertive,
                                            noteActivity.Passive,
                                            noteActivity.Variable,
                                            noteActivity.Uninterested,
                                            noteActivity.EngagedActive,
                                            noteActivity.Distractible,
                                            noteActivity.Confused,
                                            noteActivity.Aggresive,
                                            noteActivity.Resistant,
                                            noteActivity.Other,

                                            (noteActivity.Objetive == null) ? 0 : noteActivity.Objetive.Id
                                        });

            return dt;
        }

        private DataTable GetWorkdayActivityFacilitatorDS(Workday_Activity_Facilitator workdayActivityFacilitator)
        {
            DataTable dt = new DataTable
            {
                TableName = "WorkdayActivityFacilitator"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("WorkdayId", typeof(int));
            dt.Columns.Add("ActivityId", typeof(int));
            dt.Columns.Add("FacilitatorId", typeof(int));
            dt.Columns.Add("Schema", typeof(int));

            dt.Columns.Add("copingSkills", typeof(bool));
            dt.Columns.Add("stressManagement", typeof(bool));
            dt.Columns.Add("healthyLiving", typeof(bool));
            dt.Columns.Add("relaxationTraining", typeof(bool));
            dt.Columns.Add("diseaseManagement", typeof(bool));
            dt.Columns.Add("communityResources", typeof(bool));
            dt.Columns.Add("activityDailyLiving", typeof(bool));
            dt.Columns.Add("socialSkills", typeof(bool));
            dt.Columns.Add("lifeSkills", typeof(bool));

            dt.Rows.Add(new object[]
                                        {
                                            workdayActivityFacilitator.Id,
                                            workdayActivityFacilitator.Workday.Id,
                                            workdayActivityFacilitator.Activity.Id,
                                            (workdayActivityFacilitator.Facilitator == null) ? 0 : workdayActivityFacilitator.Facilitator.Id,
                                            workdayActivityFacilitator.Schema,

                                            workdayActivityFacilitator.copingSkills,
                                            workdayActivityFacilitator.stressManagement,
                                            workdayActivityFacilitator.healthyLiving,
                                            workdayActivityFacilitator.relaxationTraining,
                                            workdayActivityFacilitator.diseaseManagement,
                                            workdayActivityFacilitator.communityResources,
                                            workdayActivityFacilitator.activityDailyLiving,
                                            workdayActivityFacilitator.socialSkills,
                                            workdayActivityFacilitator.lifeSkills
                                        });

            return dt;
        }

        private DataTable GetGroupNoteActivityDS(GroupNote_Activity noteActivity)
        {
            DataTable dt = new DataTable
            {
                TableName = "GroupNoteActivity"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("GroupNoteId", typeof(int));
            dt.Columns.Add("ActivityId", typeof(int));
            dt.Columns.Add("AnswerClient", typeof(string));
            dt.Columns.Add("AnswerFacilitator", typeof(string));
            dt.Columns.Add("ObjetiveId", typeof(int));

            dt.Rows.Add(new object[]
                                        {
                                            noteActivity.Id,
                                            noteActivity.GroupNote.Id,
                                            noteActivity.Activity.Id,
                                            noteActivity.AnswerClient,
                                            noteActivity.AnswerFacilitator,
                                            (noteActivity.Objetive == null) ? 0 : noteActivity.Objetive.Id
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

        private DataTable GetMedicationsListDS(List<MedicationEntity> medicationList)
        {
            DataTable dt = new DataTable
            {
                TableName = "Medication"
            };

            // Create columns
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("ClientId", typeof(int));
            dt.Columns.Add("Dosage", typeof(string));
            dt.Columns.Add("Frequency", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Prescriber", typeof(string));

            foreach (MedicationEntity item in medicationList)
            {

                dt.Rows.Add(new object[]
                                        {
                                            item.Id,
                                            0,
                                            item.Dosage,
                                            item.Frequency,
                                            item.Name,
                                            item.Prescriber
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
