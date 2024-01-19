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

        [Authorize(Roles = "Manager, Frontdesk")]
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

            CalendarCMH model = new CalendarCMH
            {
                IdClient = 0,
                Clients = _combosHelper.GetComboClientsByClinic(user_logged.Clinic.Id, false)
            };

            return View(model);
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

        [Authorize(Roles = "Manager, Frontdesk")]
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
                Task<List<object>> mtpReviewTask = MTPReviewsByDocumentsAssistant(idDocumentsAssistant, initDate, finalDate);
                Task<List<object>> farsTask = FarsByDocumentsAssistant(idDocumentsAssistant, initDate, finalDate);
                Task<List<object>> MedicalHistoryTask = MedicalHistoryByDocumentsAssistant(idDocumentsAssistant, initDate, finalDate);

                await Task.WhenAll(mtpsTask, biosTask, mtpReviewTask, farsTask, MedicalHistoryTask);

                var mtps = await mtpsTask;
                var bios = await biosTask;
                var reviews = await mtpReviewTask;
                var fars = await farsTask;
                var MedicalHistory = await MedicalHistoryTask;

                List<object> events = new List<object>();
                events.AddRange(mtps);
                events.AddRange(bios);
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

                                    .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.Client.Id == idClient))
                                    .ToListAsync();
            }

            return mtpEntity.Select(m => new
                            {
                                title = "MTP Document",
                                start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                    m.StartTime.Hour, m.StartTime.Minute, 0)
                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
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

                                    .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.Client.Id == idClient))
                                    .ToListAsync();
            }

            return bioEntityList.Select(m => new
                                {
                                    title = "BIO Document",
                                    start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                        m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                        .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
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

                                          .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.Mtp.Client.Id == idClient))
                                          .ToListAsync();
            }

            return mtpReviewEntity.Select(m => new
                                  {
                                    title = "MTPR Document",
                                    start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                        m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                        .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
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

                                         .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.Client.Id == idClient))
                                         .ToListAsync();
            }

            return farsEntityList.Select(m => new
                                 {
                                    title = "FARS Document",
                                    start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                            m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                            m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    backgroundColor = "#dff0d8",
                                    textColor = "#417c49",
                                    borderColor = "#417c49"
                                 })
                                 .ToList<object>();
        }

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
                                    .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.CreatedBy == facilitator.LinkedUser))
                                    .ToListAsync();
            }

            return mtpEntity.Select(m => new
                            {
                                title = $"MTP Document - {m.Client.Name}",
                                start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                    m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
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
                                        .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.CreatedBy == facilitator.LinkedUser))
                                        .ToListAsync();
            }

            return bioEntityList.Select(m => new
                                {
                                    title = $"BIO Document - {m.Client.Name}",
                                    start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                            m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
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
                                          .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.CreatedBy == facilitator.LinkedUser))
                                          .ToListAsync();
            }

            return mtpReviewEntity.Select(m => new
                                  {
                                      title = $"MTPR Document - {m.Mtp.Client.Name}",
                                      start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                            m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                      end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
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
                                         .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.CreatedBy == facilitator.LinkedUser))
                                         .ToListAsync();
            }

            return farsEntityList.Select(m => new
                                 {
                                    title = $"FARS Document  - {m.Client.Name}",
                                    start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                                                m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
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
                                    .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.CreatedBy == documentsAssistant.LinkedUser))
                                    .ToListAsync();
            }

            return mtpEntity.Select(m => new
            {
                title = $"MTP Document - {m.Client.Name}",
                start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                    m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                    m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                backgroundColor = "#dff0d8",
                textColor = "#417c49",
                borderColor = "#417c49"
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
                                        .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.CreatedBy == documentsAssistant.LinkedUser))
                                        .ToListAsync();
            }

            return bioEntityList.Select(m => new
            {
                title = $"BIO Document - {m.Client.Name}",
                start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                            m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                            m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                backgroundColor = "#dff0d8",
                textColor = "#417c49",
                borderColor = "#417c49"
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
                                          .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.CreatedBy == documentsAssistant.LinkedUser))
                                          .ToListAsync();
            }

            return mtpReviewEntity.Select(m => new
            {
                title = $"MTPR Document - {m.Mtp.Client.Name}",
                start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                            m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                            m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                backgroundColor = "#dff0d8",
                textColor = "#417c49",
                borderColor = "#417c49"
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
                                         .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.CreatedBy == documentsAssistant.LinkedUser))
                                         .ToListAsync();
            }

            return farsEntityList.Select(m => new
            {
                title = $"FARS Document  - {m.Client.Name}",
                start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                                                m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                                                m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                backgroundColor = "#dff0d8",
                textColor = "#417c49",
                borderColor = "#417c49"
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
                                                   .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.CreatedBy == documentsAssistant.LinkedUser))
                                                   .ToListAsync();
            }

            return MedicalHistoryEntityList.Select(m => new
            {
                title = $"Medical History  - {m.Client.Name}",
                start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                                                m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                                                                m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                backgroundColor = "#dff0d8",
                textColor = "#417c49",
                borderColor = "#417c49"
            })
                                 .ToList<object>();
        }
        #endregion
    }
}
