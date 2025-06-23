using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class CalendarController : Controller
    {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;

        public IConfiguration Configuration { get; }

        public CalendarController(DataContext context, ICombosHelper combosHelper, IConfiguration configuration)
        {
            _context = context;
            _combosHelper = combosHelper;
            Configuration = configuration;
        }

        [Authorize(Roles = "Manager, Frontdesk, CaseManager, TCMSupervisor, Facilitator")]
        public IActionResult Index()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                                .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (User.IsInRole("CaseManager"))
            {
                CalendarCMH model = new CalendarCMH
                {
                    IdClient = 0,
                    Clients = _combosHelper.GetComboClientsByTCM(user_logged.UserName, user_logged.Clinic.Id, false)
                };

                return View(model);
            }
            else
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    CalendarCMH model = new CalendarCMH
                    {
                        IdClient = 0,
                        Clients = _combosHelper.GetComboClientsByCaseManagerByTCMSupervisor(user_logged.UserName, 1)
                    };

                    return View(model);
                }
                else
                {
                    if (User.IsInRole("Facilitator"))
                    {
                        FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                        CalendarCMH model = new CalendarCMH
                        {
                            IdClient = 0,
                            Clients = _combosHelper.GetComboClientsByFacilitator(facilitator, user_logged.Clinic.Id)
                        };

                        return View(model);
                    }
                    else
                    {
                        CalendarCMH model = new CalendarCMH
                        {
                            IdClient = 0,
                            Clients = _combosHelper.GetComboTCMClientsByClinic_ClientId(user_logged.Clinic.Id)
                        };

                        return View(model);
                    }
                }
            }
            
        }

        [Authorize(Roles = "Manager, Facilitator, Frontdesk")]
        public IActionResult IndexFacilitator()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                                .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            CalendarCMH model = null;
            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk"))
            {
                model = new CalendarCMH
                {
                    IdFacilitator = 0,
                    Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false)
                };
            }
            if (User.IsInRole("Facilitator"))
            {
                FacilitatorEntity facilitator = _context.Facilitators
                                                        .FirstOrDefault(f => f.LinkedUser == User.Identity.Name);
                model = new CalendarCMH
                {
                    IdFacilitator = facilitator.Id,
                    Facilitators = new List<SelectListItem> { 
                        new SelectListItem {
                            Value = facilitator.Id.ToString(),
                            Text = facilitator.Name
                        }
                    }
                };
            }

            return View(model);
        }

        [Authorize(Roles = "Frontdesk")]
        public IActionResult Schedule(int facilitatorId = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                                .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            CalendarCMH model = new CalendarCMH
            {
                IdFacilitator = facilitatorId,
                Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false, true)
            };

            return View(model);
        }

        [Authorize(Roles = "Frontdesk")]
        public IActionResult AddSchedule(int id)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                                .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            CalendarCMH model = new CalendarCMH
            {
                IdFacilitator = 0,
                Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false)
            };

            return View(model);
        }

        [Authorize(Roles = "Manager, Frontdesk, CaseManager, TCMSupervisor, Facilitator")]
        public async Task<IActionResult> Events(string start, string end, int idClient)
        {
            if (idClient != 0)
            {
                DateTime initDate = Convert.ToDateTime(start);
                DateTime finalDate = Convert.ToDateTime(end);

                Task<List<object>> notesTask = NotesByClient(idClient, initDate, finalDate);
                Task<List<object>> mtpsTask = MTPsByClient(idClient, initDate, finalDate);
                Task<List<object>> biosTask = BIOsByClient(idClient, initDate, finalDate);
                Task<List<object>> mtpReviewTask = MTPReviewsByClient(idClient, initDate, finalDate);
                Task<List<object>> farsTask = FarsByClient(idClient, initDate, finalDate);
                Task<List<object>> tcmNotesTask = TCMNotesByClient(idClient, initDate, finalDate);

                await Task.WhenAll(notesTask, mtpsTask, biosTask, mtpReviewTask, farsTask, tcmNotesTask);
                
                var notes = await notesTask;
                var mtps = await mtpsTask;
                var bios = await biosTask;
                var reviews = await mtpReviewTask;
                var fars = await farsTask;
                var tcmNotes = await tcmNotesTask;

                List<object> events = new List<object>();
                events.AddRange(notes);
                events.AddRange(mtps);
                events.AddRange(bios);
                events.AddRange(reviews);
                events.AddRange(fars);
                events.AddRange(tcmNotes);

                return new JsonResult(events);
            }
            else
            { 
                var events = new List<Workday_Client>();
                return new JsonResult(events);
            }
        }

        [Authorize(Roles = "Manager, Facilitator, Frontdesk")]
        public async Task<IActionResult> EventsFacilitator(string start, string end, int idFacilitator)
        {
            if (idFacilitator != 0)
            {
                DateTime initDate = Convert.ToDateTime(start);
                DateTime finalDate = Convert.ToDateTime(end);

                Task<List<object>> notesTask = NotesByFacilitator(idFacilitator, initDate, finalDate);
                Task<List<object>> mtpsTask = MTPsByFacilitator(idFacilitator, initDate, finalDate);
                Task<List<object>> biosTask = BIOsByFacilitator(idFacilitator, initDate, finalDate);
                Task<List<object>> mtpReviewTask = MTPReviewsByFacilitator(idFacilitator, initDate, finalDate);
                Task<List<object>> farsTask = FarsByFacilitator(idFacilitator, initDate, finalDate);

                await Task.WhenAll(notesTask, mtpsTask, biosTask, mtpReviewTask, farsTask);

                var notes = await notesTask;
                var mtps = await mtpsTask;
                var bios = await biosTask;
                var reviews = await mtpReviewTask;
                var fars = await farsTask;

                List<object> events = new List<object>();
                events.AddRange(notes);
                events.AddRange(mtps);
                events.AddRange(bios);
                events.AddRange(reviews);
                events.AddRange(fars);

                return new JsonResult(events);
            }
            else
            {
                var events = new List<Workday_Client>();
                return new JsonResult(events);
            }
        }

        [Authorize(Roles = "Frontdesk")]
        public async Task<IActionResult> EventsSchedule(string start, string end, int idFacilitator)
        {            
            DateTime initDate = Convert.ToDateTime(start);
            DateTime finalDate = Convert.ToDateTime(end);

            Task<List<object>> notesTask = AppointmentsByFacilitator(idFacilitator, initDate, finalDate);
                
            await Task.WhenAll(notesTask);

            var notes = await notesTask;
                
            List<object> events = new List<object>();
            events.AddRange(notes);
                
            return new JsonResult(events);            
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult IndexDocumentsAssistant()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                                .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            CalendarCMH_DocAssistant model = null;
            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk"))
            {
                model = new CalendarCMH_DocAssistant
                {
                    IdDocumentAssistant = 0,
                    DocumentAssistants = _combosHelper.GetComboDocumentsAssistantByClinic(user_logged.Clinic.Id, false)
                };
            }
            if (User.IsInRole("Documents_Assistant"))
            {
                //int a = AjustarHorarios();
                DocumentsAssistantEntity documentsAssistant = _context.DocumentsAssistant
                                                                      .FirstOrDefault(f => f.LinkedUser == User.Identity.Name);
                model = new CalendarCMH_DocAssistant
                {
                    IdDocumentAssistant = documentsAssistant.Id,
                    DocumentAssistants = new List<SelectListItem> {
                        new SelectListItem {
                            Value = documentsAssistant.Id.ToString(),
                            Text = documentsAssistant.Name
                        }
                    }
                };
            }

            return View(model);
        }

        [Authorize(Roles = "Manager, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> EventsDocumentsAssistant(string start, string end, int idDocumentsAssistant)
        {
            if (idDocumentsAssistant != 0)
            {
                DateTime initDate = Convert.ToDateTime(start);
                DateTime finalDate = Convert.ToDateTime(end);

               // Task<List<object>> notesTask = NotesByDocumentsAssistant(idDocumentsAssistant, initDate, finalDate);
                Task<List<object>> mtpsTask = MTPsByDocumentsAssistant(idDocumentsAssistant, initDate, finalDate);
                Task<List<object>> biosTask = BIOsByDocumentsAssistant(idDocumentsAssistant, initDate, finalDate);
                Task<List<object>> briefTask = BriefsByDocumentsAssistant(idDocumentsAssistant, initDate, finalDate);
                Task<List<object>> mtpReviewTask = MTPReviewsByDocumentsAssistant(idDocumentsAssistant, initDate, finalDate);
                Task<List<object>> farsTask = FarsByDocumentsAssistant(idDocumentsAssistant, initDate, finalDate);
                Task<List<object>> MedicalHistoryTask = MedicalHistoryByDocumentsAssistant(idDocumentsAssistant, initDate, finalDate);

                await Task.WhenAll(mtpsTask, biosTask, briefTask, mtpReviewTask, farsTask, MedicalHistoryTask);

                var mtps = await mtpsTask;
                var bios = await biosTask;
                var brief = await briefTask;
                var reviews = await mtpReviewTask;
                var fars = await farsTask;
                var MedicalHistory = await MedicalHistoryTask;

                List<object> events = new List<object>();
                events.AddRange(mtps);
                events.AddRange(bios);
                events.AddRange(brief);
                events.AddRange(reviews);
                events.AddRange(fars);
                events.AddRange(MedicalHistory);

                return new JsonResult(events);
            }
            else
            {
                var events = new List<Workday_Client>();
                return new JsonResult(events);
            }
        }

        #region Utils
        private async Task<List<object>> NotesByClient(int idClient, DateTime initDate, DateTime finalDate)
        {            
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<Workday_Client> listWorkdayClient;

            using (DataContext db = new DataContext(options))
            {
                listWorkdayClient = await db.Workdays_Clients

                                            .Include(wc => wc.Schedule)
                                            .Include(wc => wc.Workday)
                                                  
                                            .Include(wc => wc.IndividualNote)
                                                .ThenInclude(i => i.SubSchedule)

                                            .Where(wc => (wc.Workday.Date >= initDate && wc.Workday.Date <= finalDate && wc.Present == true &&
                                                    wc.Client.Id == idClient))
                                            .ToListAsync();                
            }

            return listWorkdayClient.Select(wc => new
                                    {
                                        title = (wc.Workday.Service == ServiceType.PSR) ? "PSR Therapy" :
                                                    (wc.Workday.Service == ServiceType.Group) ? "Group Therapy" :
                                                        (wc.Workday.Service == ServiceType.Individual) ? "Individual Therapy" : string.Empty,
                                        start = (wc.Workday.Service == ServiceType.Individual) ?
                                                new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                                (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.InitialTime.Hour : 0, (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.InitialTime.Minute : 0, 0)
                                                                .ToString("yyyy-MM-ddTHH:mm:ssK")
                                                :
                                                new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                                (wc.Schedule) != null ? wc.Schedule.InitialTime.Hour : 0, (wc.Schedule) != null ? wc.Schedule.InitialTime.Minute : 0, 0)
                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                        end = (wc.Workday.Service == ServiceType.Individual) ?
                                                new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                                (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.EndTime.Hour : 0, (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.EndTime.Minute : 0, 0)
                                                                .ToString("yyyy-MM-ddTHH:mm:ssK")
                                                :
                                                new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                                (wc.Schedule) != null ? wc.Schedule.EndTime.Hour : 0, (wc.Schedule) != null ? wc.Schedule.EndTime.Minute : 0, 0)
                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                        backgroundColor = (wc.Workday.Service == ServiceType.PSR) ? "#fcf8e3" :
                                                            (wc.Workday.Service == ServiceType.Group) ? "#d9edf7" :
                                                                (wc.Workday.Service == ServiceType.Individual) ? "#dff0d8" : "#dff0d8",
                                        textColor = (wc.Workday.Service == ServiceType.PSR) ? "#9e7d67" :
                                                        (wc.Workday.Service == ServiceType.Group) ? "#487c93" :
                                                            (wc.Workday.Service == ServiceType.Individual) ? "#417c49" : "#417c49",
                                        borderColor = (wc.Workday.Service == ServiceType.PSR) ? "#9e7d67" :
                                                        (wc.Workday.Service == ServiceType.Group) ? "#487c93" :
                                                            (wc.Workday.Service == ServiceType.Individual) ? "#417c49" : "#417c49"
                                    })
                                    .ToList<object>();            
        }

        private async Task<List<object>> MTPsByClient(int idClient, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<MTPEntity> mtpEntity;

            using (DataContext db = new DataContext(options))
            {
                mtpEntity = await db.MTPs                                                 

                                    .Where(m => (m.AdmissionDateMTP >= initDate && m.AdmissionDateMTP <= finalDate && m.Client.Id == idClient))
                                    .ToListAsync();
            }

            return mtpEntity.Select(m => new
                            {
                                title = "MTP Document",
                                start = new DateTime(m.AdmissionDateMTP.Year, m.AdmissionDateMTP.Month, m.AdmissionDateMTP.Date.Day,
                                                    m.StartTime.Hour, m.StartTime.Minute, 0)
                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                end = new DateTime(m.AdmissionDateMTP.Year, m.AdmissionDateMTP.Month, m.AdmissionDateMTP.Date.Day,
                                                    m.EndTime.Hour, m.EndTime.Minute, 0)
                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                backgroundColor = "#dff0d8",
                                textColor = "#417c49",
                                borderColor = "#417c49"
                            })
                            .ToList<object>();
        }

        private async Task<List<object>> BIOsByClient(int idClient, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<BioEntity> bioEntityList;

            using (DataContext db = new DataContext(options))
            {
                bioEntityList = await db.Bio

                                    .Where(m => (m.DateBio >= initDate && m.DateBio <= finalDate && m.Client.Id == idClient))
                                    .ToListAsync();
            }

            return bioEntityList.Select(m => new
                                {
                                    title = "BIO Document",
                                    start = new DateTime(m.DateBio.Year, m.DateBio.Month, m.DateBio.Date.Day,
                                                                        m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                        .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(m.DateBio.Year, m.DateBio.Month, m.DateBio.Date.Day,
                                                                        m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                        .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    backgroundColor = "#dff0d8",
                                    textColor = "#417c49",
                                    borderColor = "#417c49"
                                })
                                .ToList<object>();
        }

        private async Task<List<object>> MTPReviewsByClient(int idClient, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<MTPReviewEntity> mtpReviewEntity;

            using (DataContext db = new DataContext(options))
            {
                mtpReviewEntity = await db.MTPReviews

                                          .Where(m => (m.DataOfService >= initDate && m.DataOfService <= finalDate && m.Mtp.Client.Id == idClient))
                                          .ToListAsync();
            }

            return mtpReviewEntity.Select(m => new
                                  {
                                    title = "MTPR Document",
                                    start = new DateTime(m.DataOfService.Year, m.DataOfService.Month, m.DataOfService.Date.Day,
                                                                        m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                        .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(m.DataOfService.Year, m.DataOfService.Month, m.DataOfService.Date.Day,
                                                                        m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                        .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    backgroundColor = "#dff0d8",
                                    textColor = "#417c49",
                                    borderColor = "#417c49"
                                  })
                                  .ToList<object>();
        }

        private async Task<List<object>> FarsByClient(int idClient, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<FarsFormEntity> farsEntityList;

            using (DataContext db = new DataContext(options))
            {
                farsEntityList = await db.FarsForm

                                         .Where(m => (m.EvaluationDate >= initDate && m.EvaluationDate <= finalDate && m.Client.Id == idClient))
                                         .ToListAsync();
            }

            return farsEntityList.Select(m => new
                                 {
                                    title = "FARS Document",
                                    start = new DateTime(m.EvaluationDate.Year, m.EvaluationDate.Month, m.EvaluationDate.Date.Day,
                                                                                            m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(m.EvaluationDate.Year, m.EvaluationDate.Month, m.EvaluationDate.Date.Day,
                                                                                            m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    backgroundColor = "#dff0d8",
                                    textColor = "#417c49",
                                    borderColor = "#417c49"
                                 })
                                 .ToList<object>();
        }

        private async Task<List<object>> TCMNotesByClient(int idClient, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<TCMNoteActivityEntity> tcmNoteActivity;

            using (DataContext db = new DataContext(options))
            {
                tcmNoteActivity = await db.TCMNoteActivity

                                          .Include(n => n.TCMNote)
                                          .ThenInclude(n => n.TCMClient)
                                          .ThenInclude(n => n.Client)

                                          .Where(wc => (wc.StartTime >= initDate 
                                                     && wc.EndTime <= finalDate 
                                                     && wc.Billable == true 
                                                     && wc.TCMNote.TCMClient.Client.Id == idClient))
                                          .ToListAsync();
            }

            return tcmNoteActivity.Select(n => new
            {
                title = "TCM Service",
                start = new DateTime(n.TCMNote.DateOfService.Year, n.TCMNote.DateOfService.Month, n.TCMNote.DateOfService.Day,
                                                                                            n.StartTime.Hour, n.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(n.TCMNote.DateOfService.Year, n.TCMNote.DateOfService.Month, n.TCMNote.DateOfService.Day,
                                                                                            n.EndTime.Hour, n.EndTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                backgroundColor = "#dff0d8",
                textColor = "#417c49",
                borderColor = "#417c49"
            })
                                    .ToList<object>();
        }

        //Facilitator

        private async Task<List<object>> NotesByFacilitator(int idFacilitator, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<Workday_Client> listWorkdayClient;

            using (DataContext db = new DataContext(options))
            {
                listWorkdayClient = await db.Workdays_Clients

                                            .Include(wc => wc.Schedule)
                                            .Include(wc => wc.Workday)

                                            .Include(wc => wc.IndividualNote)
                                                .ThenInclude(i => i.SubSchedule)

                                            .Include(wc => wc.Client)

                                            .Where(wc => (wc.Workday.Date >= initDate && wc.Workday.Date <= finalDate && wc.Present == true &&
                                                    wc.Facilitator.Id == idFacilitator))
                                            
                                            .ToListAsync();
            }

            return listWorkdayClient
                    .Select(wc => new
                    {
                        title = (wc.Workday.Service == ServiceType.PSR) ? "PSR Therapy" :
                                    (wc.Workday.Service == ServiceType.Group) ? "Group Therapy" :
                                        (wc.Workday.Service == ServiceType.Individual && wc.Client == null) ? "Individual Therapy - No client" :
                                            (wc.Workday.Service == ServiceType.Individual && wc.Client != null) ? $"Individual Therapy - {wc.Client.Name}" : 
                                                string.Empty,
                        start = (wc.Workday.Service == ServiceType.Individual) ?
                                    new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                    (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.InitialTime.Hour : 0, (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.InitialTime.Minute : 0, 0)
                                                    .ToString("yyyy-MM-ddTHH:mm:ssK")
                                    :
                                    new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                    (wc.Schedule) != null ? wc.Schedule.InitialTime.Hour : 0, (wc.Schedule) != null ? wc.Schedule.InitialTime.Minute : 0, 0)
                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                        end = (wc.Workday.Service == ServiceType.Individual) ?
                                  new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                  (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.EndTime.Hour : 0, (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.EndTime.Minute : 0, 0)
                                                  .ToString("yyyy-MM-ddTHH:mm:ssK")
                                  :
                                  new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                              (wc.Schedule) != null ? wc.Schedule.EndTime.Hour : 0, (wc.Schedule) != null ? wc.Schedule.EndTime.Minute : 0, 0)
                                              .ToString("yyyy-MM-ddTHH:mm:ssK"),
                        backgroundColor = (wc.Workday.Service == ServiceType.PSR) ? "#fcf8e3" :
                                                                    (wc.Workday.Service == ServiceType.Group) ? "#d9edf7" :
                                                                        (wc.Workday.Service == ServiceType.Individual) ? "#dff0d8" : "#dff0d8",
                        textColor = (wc.Workday.Service == ServiceType.PSR) ? "#9e7d67" :
                                                                (wc.Workday.Service == ServiceType.Group) ? "#487c93" :
                                                                    (wc.Workday.Service == ServiceType.Individual) ? "#417c49" : "#417c49",
                        borderColor = (wc.Workday.Service == ServiceType.PSR) ? "#9e7d67" :
                                                                (wc.Workday.Service == ServiceType.Group) ? "#487c93" :
                                                                    (wc.Workday.Service == ServiceType.Individual) ? "#417c49" : "#417c49"
                    })
                    .Distinct()
                    .ToList<object>();
        }

        private async Task<List<object>> AppointmentsByFacilitator(int idFacilitator, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<CiteEntity> listAppointments;

            using (DataContext db = new DataContext(options))
            {
                listAppointments = await db.Cites

                                           .Include(c => c.SubSchedule)
                                           .Include(c => c.Worday_CLient)

                                           .Include(c => c.Client)

                                           .Include(c => c.Facilitator)

                                           .Where(c => (c.DateCite >= initDate && c.DateCite <= finalDate))
                                           .ToListAsync();
                                     

                if (idFacilitator != 0)
                {
                    listAppointments = listAppointments.Where(c => c.Facilitator.Id == idFacilitator).ToList();
                }                                            
            }

            return listAppointments
                        .Select(c => new
                        {
                            id = c.Id,
                            title = (c.Client == null) ? $"{c.Facilitator.Name} - No client" :
                                    (c.Client != null) ? $"{c.Facilitator.Name} - {c.Client.Name}" :
                                                    string.Empty,
                            start = new DateTime(c.DateCite.Year, c.DateCite.Month, c.DateCite.Day,
                                                    c.SubSchedule.InitialTime.Hour, c.SubSchedule.InitialTime.Minute, 0)
                                                        .ToString("yyyy-MM-ddTHH:mm:ssK"),                                    
                            end = new DateTime(c.DateCite.Year, c.DateCite.Month, c.DateCite.Day,
                                                  c.SubSchedule.EndTime.Hour, c.SubSchedule.EndTime.Minute, 0)
                                                        .ToString("yyyy-MM-ddTHH:mm:ssK"),                                  
                            backgroundColor = (c.Status == CiteStatus.C) ? "#dff0d8" :
                                                (c.Status == CiteStatus.S) ? "#76b5c5" :
                                                    (c.Status == CiteStatus.R) ? "#d0a190" :
                                                        (c.Status == CiteStatus.NS) ? "#eab676" :
                                                            (c.Status == CiteStatus.AR) ? "#fbffaa" :
                                                                (c.Status == CiteStatus.A) ? "#0f8f05" :
                                                                    (c.Status == CiteStatus.X) ? "#a54237" : string.Empty,
                            textColor = (c.Status == CiteStatus.C) ? "#417c49" :
                                           (c.Status == CiteStatus.S) ? "#063970" :
                                              (c.Status == CiteStatus.R) ? "#6a3b2a" :
                                                (c.Status == CiteStatus.NS) ? "#873e23" :
                                                    (c.Status == CiteStatus.AR) ? "#aea724" :
                                                        (c.Status == CiteStatus.A) ? "#010e00" :
                                                            (c.Status == CiteStatus.X) ? "#390802" : string.Empty,
                            borderColor = (c.Status == CiteStatus.C) ? "#417c49" :
                                           (c.Status == CiteStatus.S) ? "#063970" :
                                              (c.Status == CiteStatus.R) ? "#6a3b2a" :
                                                (c.Status == CiteStatus.NS) ? "#873e23" :
                                                    (c.Status == CiteStatus.AR) ? "#aea724" :
                                                        (c.Status == CiteStatus.A) ? "#010e00" :
                                                            (c.Status == CiteStatus.X) ? "#390802" : string.Empty
                        })
                        .Distinct()
                        .ToList<object>();
        }

        private async Task<List<object>> MTPsByFacilitator(int idFacilitator, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<MTPEntity> mtpEntity;
            FacilitatorEntity facilitator;

            using (DataContext db = new DataContext(options))
            {
                facilitator = await db.Facilitators
                                      .FirstOrDefaultAsync(f => f.Id == idFacilitator);

                mtpEntity = await db.MTPs
                                    .Include(m => m.Client)
                                    .Where(m => (m.AdmissionDateMTP >= initDate && m.AdmissionDateMTP <= finalDate && m.CreatedBy == facilitator.LinkedUser))
                                    .ToListAsync();
            }

            return mtpEntity.Select(m => new
                            {
                                title = $"MTP Document - {m.Client.Name}",
                                start = new DateTime(m.AdmissionDateMTP.Year, m.AdmissionDateMTP.Month, m.AdmissionDateMTP.Date.Day,
                                                                    m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                end = new DateTime(m.AdmissionDateMTP.Year, m.AdmissionDateMTP.Month, m.AdmissionDateMTP.Date.Day,
                                                                    m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                backgroundColor = "#dff0d8",
                                textColor = "#417c49",
                                borderColor = "#417c49"
                            })
                            .ToList<object>();
        }

        private async Task<List<object>> BIOsByFacilitator(int idFacilitator, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<BioEntity> bioEntityList;
            FacilitatorEntity facilitator;

            using (DataContext db = new DataContext(options))
            {
                facilitator = await db.Facilitators
                                      .FirstOrDefaultAsync(f => f.Id == idFacilitator);

                bioEntityList = await db.Bio
                                        .Include(m => m.Client)
                                        .Where(m => (m.DateBio >= initDate && m.DateBio <= finalDate && m.CreatedBy == facilitator.LinkedUser))
                                        .ToListAsync();
            }

            return bioEntityList.Select(m => new
                                {
                                    title = $"BIO Document - {m.Client.Name}",
                                    start = new DateTime(m.DateBio.Year, m.DateBio.Month, m.DateBio.Date.Day,
                                                                                            m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(m.DateBio.Year, m.DateBio.Month, m.DateBio.Date.Day,
                                                                                            m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    backgroundColor = "#dff0d8",
                                    textColor = "#417c49",
                                    borderColor = "#417c49"
                                })
                                .ToList<object>();
        }

        private async Task<List<object>> MTPReviewsByFacilitator(int idFacilitator, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<MTPReviewEntity> mtpReviewEntity;
            FacilitatorEntity facilitator;

            using (DataContext db = new DataContext(options))
            {
                facilitator = await db.Facilitators
                                      .FirstOrDefaultAsync(f => f.Id == idFacilitator);

                mtpReviewEntity = await db.MTPReviews
                                          .Include(m => m.Mtp)
                                            .ThenInclude(mt => mt.Client)
                                          .Where(m => (m.DataOfService >= initDate && m.DataOfService <= finalDate && m.CreatedBy == facilitator.LinkedUser))
                                          .ToListAsync();
            }

            return mtpReviewEntity.Select(m => new
                                  {
                                      title = $"MTPR Document - {m.Mtp.Client.Name}",
                                      start = new DateTime(m.DataOfService.Year, m.DataOfService.Month, m.DataOfService.Date.Day,
                                                                                            m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                      end = new DateTime(m.DataOfService.Year, m.DataOfService.Month, m.DataOfService.Date.Day,
                                                                                            m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                      backgroundColor = "#dff0d8",
                                      textColor = "#417c49",
                                      borderColor = "#417c49"
                                  })
                                  .ToList<object>();
        }

        private async Task<List<object>> FarsByFacilitator(int idFacilitator, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<FarsFormEntity> farsEntityList;
            FacilitatorEntity facilitator;

            using (DataContext db = new DataContext(options))
            {
                facilitator = await db.Facilitators
                                      .FirstOrDefaultAsync(f => f.Id == idFacilitator);

                farsEntityList = await db.FarsForm
                                         .Include(f => f.Client)
                                         .Where(m => (m.EvaluationDate >= initDate && m.EvaluationDate <= finalDate && m.CreatedBy == facilitator.LinkedUser))
                                         .ToListAsync();
            }

            return farsEntityList.Select(m => new
                                 {
                                    title = $"FARS Document  - {m.Client.Name}",
                                    start = new DateTime(m.EvaluationDate.Year, m.EvaluationDate.Month, m.EvaluationDate.Date.Day,
                                                                                                                m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(m.EvaluationDate.Year, m.EvaluationDate.Month, m.EvaluationDate.Date.Day,
                                                                                                                m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    backgroundColor = "#dff0d8",
                                    textColor = "#417c49",
                                    borderColor = "#417c49"
                                 })
                                 .ToList<object>();
        }

        //Documents Assistant
        private async Task<List<object>> MTPsByDocumentsAssistant(int idDocumentsAssistant, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<MTPEntity> mtpEntity;
            DocumentsAssistantEntity documentsAssistant;

            using (DataContext db = new DataContext(options))
            {
                documentsAssistant = await db.DocumentsAssistant
                                             .FirstOrDefaultAsync(f => f.Id == idDocumentsAssistant);

                mtpEntity = await db.MTPs
                                    .Include(m => m.Client)
                                    .Include(m => m.Messages)
                                    .Where(m => (m.StartTime >= initDate && m.StartTime <= finalDate && m.CreatedBy == documentsAssistant.LinkedUser))
                                    .ToListAsync();
            }

            return mtpEntity.Select(m => new
            {
                title = $"MTP Document - {m.Client.Name}",
                start = new DateTime(m.StartTime.Year, m.StartTime.Month, m.StartTime.Date.Day,
                                                                    m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(m.EndTime.Year, m.EndTime.Month, m.EndTime.Date.Day,
                                                                    m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                url = Url.Action("Edit", "MTPs", new { id = m.Id, origi = 3 }),
                backgroundColor = (m.Status == MTPStatus.Edition) ? "#fcf8e3" :
                                                                           (m.Status == MTPStatus.Pending) ? "#d9edf7" :
                                                                               (m.Status == MTPStatus.Approved) ? "#dff0d8" : "#dff0d8",
                textColor = (m.Status == MTPStatus.Edition) ? "#9e7d67" :
                                                                    (m.Status == MTPStatus.Pending && m.Messages.Count() > 0) ? "#Be2528" :
                                                                           (m.Status == MTPStatus.Pending) ? "#487c93" :
                                                                               (m.Status == MTPStatus.Approved) ? "#417c49" : "#417c49",
                borderColor = (m.Status == MTPStatus.Edition) ? "#9e7d67" :
                                                                           (m.Status == MTPStatus.Pending) ? "#487c93" :
                                                                               (m.Status == MTPStatus.Approved) ? "#417c49" : "#417c49"
            })
                            .ToList<object>();
        }
        private async Task<List<object>> BIOsByDocumentsAssistant(int idDocumentsAssistant, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<BioEntity> bioEntityList;
            DocumentsAssistantEntity documentsAssistant;

            using (DataContext db = new DataContext(options))
            {
                documentsAssistant = await db.DocumentsAssistant
                                             .FirstOrDefaultAsync(f => f.Id == idDocumentsAssistant);

                bioEntityList = await db.Bio
                                        .Include(m => m.Client)
                                        .Include(m => m.Messages)
                                        .Where(m => (m.StartTime >= initDate && m.StartTime <= finalDate && m.CreatedBy == documentsAssistant.LinkedUser))
                                        .ToListAsync();
            }

            return bioEntityList.Select(m => new
            {
                title = $"BIO Document - {m.Client.Name}",
                start = new DateTime(m.StartTime.Year, m.StartTime.Month, m.StartTime.Date.Day,
                                                                                            m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(m.EndTime.Year, m.EndTime.Month, m.EndTime.Date.Day,
                                                                                            m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                url = Url.Action("Edit", "Bios", new { id = m.Client_FK, origi = 3 }),
                backgroundColor = (m.Status == BioStatus.Edition) ? "#fcf8e3" :
                                                                           (m.Status == BioStatus.Pending) ? "#d9edf7" :
                                                                               (m.Status == BioStatus.Approved) ? "#dff0d8" : "#dff0d8",
                textColor = (m.Status == BioStatus.Edition) ? "#9e7d67" :
                                                                    (m.Status == BioStatus.Pending && m.Messages.Count() > 0) ? "#Be2528" :
                                                                           (m.Status == BioStatus.Pending) ? "#487c93" :
                                                                               (m.Status == BioStatus.Approved) ? "#417c49" : "#417c49",
                borderColor = (m.Status == BioStatus.Edition) ? "#9e7d67" :
                                                                           (m.Status == BioStatus.Pending) ? "#487c93" :
                                                                               (m.Status == BioStatus.Approved) ? "#417c49" : "#417c49"
            })
                                .ToList<object>();
        }
        private async Task<List<object>> MTPReviewsByDocumentsAssistant(int idDocumentsAssistant, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<MTPReviewEntity> mtpReviewEntity;
            DocumentsAssistantEntity documentsAssistant;

            using (DataContext db = new DataContext(options))
            {
                documentsAssistant = await db.DocumentsAssistant
                                             .FirstOrDefaultAsync(f => f.Id == idDocumentsAssistant);

                mtpReviewEntity = await db.MTPReviews
                                          .Include(m => m.Mtp)
                                            .ThenInclude(mt => mt.Client)
                                          .Include(m => m.Messages)
                                          .Where(m => (m.StartTime >= initDate && m.StartTime <= finalDate && m.CreatedBy == documentsAssistant.LinkedUser))
                                          .ToListAsync();
            }

            return mtpReviewEntity.Select(m => new
            {
                title = $"MTPR Document - {m.Mtp.Client.Name}",
                start = new DateTime(m.StartTime.Year, m.StartTime.Month, m.StartTime.Date.Day,
                                                                                            m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(m.EndTime.Year, m.EndTime.Month, m.EndTime.Date.Day,
                                                                                            m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                url = Url.Action("EditMTPReview", "MTPs", new { id = m.Id, origin = 9 }),
                backgroundColor = (m.Status == AdendumStatus.Edition) ? "#fcf8e3" :
                                                                           (m.Status == AdendumStatus.Pending) ? "#d9edf7" :
                                                                               (m.Status == AdendumStatus.Approved) ? "#dff0d8" : "#dff0d8",
                textColor = (m.Status == AdendumStatus.Edition) ? "#9e7d67" :
                                                                    (m.Status == AdendumStatus.Pending && m.Messages.Count() > 0) ? "#Be2528" :
                                                                           (m.Status == AdendumStatus.Pending) ? "#487c93" :
                                                                               (m.Status == AdendumStatus.Approved) ? "#417c49" : "#417c49",
                borderColor = (m.Status == AdendumStatus.Edition) ? "#9e7d67" :
                                                                           (m.Status == AdendumStatus.Pending) ? "#487c93" :
                                                                               (m.Status == AdendumStatus.Approved) ? "#417c49" : "#417c49"
            })
                                  .ToList<object>();
        }
        private async Task<List<object>> FarsByDocumentsAssistant(int idDocumentsAssistant, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<FarsFormEntity> farsEntityList;
            DocumentsAssistantEntity documentsAssistant;

            using (DataContext db = new DataContext(options))
            {
                documentsAssistant = await db.DocumentsAssistant
                                             .FirstOrDefaultAsync(f => f.Id == idDocumentsAssistant);

                farsEntityList = await db.FarsForm
                                         .Include(f => f.Client)
                                         .Include(f => f.Messages)
                                         .Where(m => (m.StartTime >= initDate && m.StartTime <= finalDate && m.CreatedBy == documentsAssistant.LinkedUser))
                                         .ToListAsync();
            }

            return farsEntityList.Select(m => new
            {
                title = $"FARS Document  - {m.Client.Name}",
                start = new DateTime(m.StartTime.Year, m.StartTime.Month, m.StartTime.Date.Day,
                                                                                                                m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(m.EndTime.Year, m.EndTime.Month, m.EndTime.Date.Day,
                                                                                                                m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                url = Url.Action("Edit", "FarsForms", new { id = m.Id, origin = 5 }),
                backgroundColor = (m.Status == FarsStatus.Edition) ? "#fcf8e3" :
                                                                           (m.Status == FarsStatus.Pending) ? "#d9edf7" :
                                                                               (m.Status == FarsStatus.Approved) ? "#dff0d8" : "#dff0d8",
                textColor = (m.Status == FarsStatus.Edition) ? "#9e7d67" :
                                                                    (m.Status == FarsStatus.Pending && m.Messages.Count() > 0) ? "#Be2528" :
                                                                           (m.Status == FarsStatus.Pending) ? "#487c93" :
                                                                               (m.Status == FarsStatus.Approved) ? "#417c49" : "#417c49",
                borderColor = (m.Status == FarsStatus.Edition) ? "#9e7d67" :
                                                                           (m.Status == FarsStatus.Pending) ? "#487c93" :
                                                                               (m.Status == FarsStatus.Approved) ? "#417c49" : "#417c49"
            })
                                 .ToList<object>();
        }
        private async Task<List<object>> MedicalHistoryByDocumentsAssistant(int idDocumentsAssistant, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<IntakeMedicalHistoryEntity> MedicalHistoryEntityList;
            DocumentsAssistantEntity documentsAssistant;

            using (DataContext db = new DataContext(options))
            {
                documentsAssistant = await db.DocumentsAssistant
                                             .FirstOrDefaultAsync(f => f.Id == idDocumentsAssistant);

                MedicalHistoryEntityList = await db.IntakeMedicalHistory
                                                   .Include(f => f.Client)
                                                   .Where(m => (m.DateSignatureEmployee >= initDate && m.DateSignatureEmployee <= finalDate && m.CreatedBy == documentsAssistant.LinkedUser))
                                                   .ToListAsync();
            }

            return MedicalHistoryEntityList.Select(m => new
            {
                title = $"Medical History  - {m.Client.Name}",
                start = new DateTime(m.DateSignatureEmployee.Year, m.DateSignatureEmployee.Month, m.DateSignatureEmployee.Date.Day,
                                                                                                                m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(m.DateSignatureEmployee.Year, m.DateSignatureEmployee.Month, m.DateSignatureEmployee.Date.Day,
                                                                                                                m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                url = Url.Action("CreateMedicalhistory", "Intakes", new { id = m.Id, origin = 2 }),
                backgroundColor = "#dff0d8",
                textColor = "#417c49",
                borderColor = "#417c49"
            })
                                 .ToList<object>();
        }
        private async Task<List<object>> BriefsByDocumentsAssistant(int idDocumentsAssistant, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<BriefEntity> briefEntityList;
            DocumentsAssistantEntity documentsAssistant;

            using (DataContext db = new DataContext(options))
            {
                documentsAssistant = await db.DocumentsAssistant
                                             .FirstOrDefaultAsync(f => f.Id == idDocumentsAssistant);

                briefEntityList = await db.Brief
                                          .Include(m => m.Client)
                                          .Include(m => m.Messages)
                                          .Where(m => (m.StartTime >= initDate && m.StartTime <= finalDate && m.CreatedBy == documentsAssistant.LinkedUser))
                                          .ToListAsync();
            }

            return briefEntityList.Select(m => new
            {
                title = $"Bief Document - {m.Client.Name}",
                start = new DateTime(m.StartTime.Year, m.StartTime.Month, m.StartTime.Date.Day,
                                                                                            m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(m.EndTime.Year, m.EndTime.Month, m.EndTime.Date.Day,
                                                                                            m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                url = Url.Action("Edit", "Briefs", new { id = m.Id, origi = 3 }),
                backgroundColor = (m.Status == BioStatus.Edition) ? "#fcf8e3" :
                                                                           (m.Status == BioStatus.Pending) ? "#d9edf7" :
                                                                               (m.Status == BioStatus.Approved) ? "#dff0d8" : "#dff0d8",
                textColor = (m.Status == BioStatus.Edition) ? "#9e7d67" :
                                                                    (m.Status == BioStatus.Pending && m.Messages.Count() > 0) ? "#Be2528" :
                                                                           (m.Status == BioStatus.Pending) ? "#487c93" :
                                                                               (m.Status == BioStatus.Approved) ? "#417c49" : "#417c49",
                borderColor = (m.Status == BioStatus.Edition) ? "#9e7d67" :
                                                                           (m.Status == BioStatus.Pending) ? "#487c93" :
                                                                               (m.Status == BioStatus.Approved) ? "#417c49" : "#417c49"
            })
                                .ToList<object>();
        }
        #endregion

        private int AjustarHorarios()
        {
            List<DocumentsAssistantEntity> listDocumentAssistant = _context.DocumentsAssistant.ToList();
            int cant = 0;
            foreach (var employed in listDocumentAssistant)
            {
                List<MTPEntity> mtps = _context.MTPs.Where(n => n.CreatedBy == employed.LinkedUser).ToList();

                foreach (var mtp in mtps)
                {
                    DateTime start = new DateTime(mtp.AdmissionDateMTP.Year, mtp.AdmissionDateMTP.Month, mtp.AdmissionDateMTP.Day, mtp.StartTime.Hour, mtp.StartTime.Minute, mtp.StartTime.Second);
                    mtp.StartTime = start;
                    DateTime end = new DateTime(mtp.AdmissionDateMTP.Year, mtp.AdmissionDateMTP.Month, mtp.AdmissionDateMTP.Day, mtp.EndTime.Hour, mtp.EndTime.Minute, mtp.EndTime.Second);
                    mtp.EndTime = end;
                    cant++;
                    _context.Update(mtp);
                }

                List<BioEntity> Bios = _context.Bio.Where(n => n.CreatedBy == employed.LinkedUser).ToList();

                foreach (var bio in Bios)
                {
                    DateTime start = new DateTime(bio.DateBio.Year, bio.DateBio.Month, bio.DateBio.Day, bio.StartTime.Hour, bio.StartTime.Minute, bio.StartTime.Second);
                    bio.StartTime = start;
                    DateTime end = new DateTime(bio.DateBio.Year, bio.DateBio.Month, bio.DateBio.Day, bio.EndTime.Hour, bio.EndTime.Minute, bio.EndTime.Second);
                    bio.EndTime = end;
                    cant++;
                    _context.Update(bio);
                }

                List<FarsFormEntity> FARSs = _context.FarsForm.Where(n => n.CreatedBy == employed.LinkedUser).ToList();

                foreach (var fars in FARSs)
                {
                    DateTime start = new DateTime(fars.EvaluationDate.Year, fars.EvaluationDate.Month, fars.EvaluationDate.Day, fars.StartTime.Hour, fars.StartTime.Minute, fars.StartTime.Second);
                    fars.StartTime = start;
                    DateTime end = new DateTime(fars.EvaluationDate.Year, fars.EvaluationDate.Month, fars.EvaluationDate.Day, fars.EndTime.Hour, fars.EndTime.Minute, fars.EndTime.Second);
                    fars.EndTime = end;
                    cant++;
                    _context.Update(fars);
                }

                List<MTPReviewEntity> MTPRs = _context.MTPReviews.Where(n => n.CreatedBy == employed.LinkedUser).ToList();

                foreach (var mtpr in MTPRs)
                {
                    DateTime start = new DateTime(mtpr.DataOfService.Year, mtpr.DataOfService.Month, mtpr.DataOfService.Day, mtpr.StartTime.Hour, mtpr.StartTime.Minute, mtpr.StartTime.Second);
                    mtpr.StartTime = start;
                    DateTime end = new DateTime(mtpr.DataOfService.Year, mtpr.DataOfService.Month, mtpr.DataOfService.Day, mtpr.EndTime.Hour, mtpr.EndTime.Minute, mtpr.EndTime.Second);
                    mtpr.EndTime = end;
                    cant++;
                    _context.Update(mtpr);
                }

                List<IntakeMedicalHistoryEntity> medicalHistoryList = _context.IntakeMedicalHistory.Where(n => n.CreatedBy == employed.LinkedUser).ToList();

                foreach (var MH in medicalHistoryList)
                {
                    DateTime start = new DateTime(MH.DateSignatureEmployee.Year, MH.DateSignatureEmployee.Month, MH.DateSignatureEmployee.Day, MH.StartTime.Hour, MH.StartTime.Minute, MH.StartTime.Second);
                    MH.StartTime = start;
                    DateTime end = new DateTime(MH.DateSignatureEmployee.Year, MH.DateSignatureEmployee.Month, MH.DateSignatureEmployee.Day, MH.EndTime.Hour, MH.EndTime.Minute, MH.EndTime.Second);
                    MH.EndTime = end;
                    cant++;
                    _context.Update(MH);
                }
            }
             _context.SaveChanges();
            return cant;
        }
    }
}
