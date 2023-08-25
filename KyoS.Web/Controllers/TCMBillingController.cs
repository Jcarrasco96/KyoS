using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class TCMBillingController : Controller
    {
        private readonly DataContext _context;        
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IExportExcellHelper _exportExcelHelper;
        private readonly IFileHelper _fileHelper;

        public TCMBillingController(DataContext context, ICombosHelper combosHelper, IRenderHelper renderHelper, IExportExcellHelper exportExcelHelper, IFileHelper fileHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _renderHelper = renderHelper;
            _exportExcelHelper = exportExcelHelper;
            _fileHelper = fileHelper;
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Index(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.DateBlocked = "B";
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            TCMBillingViewModel model = new TCMBillingViewModel
            {
                IdClient = 0,
                Clients = _combosHelper.GetComboTCMClientsByCaseManager(user_logged.UserName)
            };

            return View(model);
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult AddProgressNote(string date)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            AddProgressNoteViewModel model = new AddProgressNoteViewModel();

            if (User.IsInRole("CaseManager"))
            {
                model = new AddProgressNoteViewModel
                {
                    Date = (date != null) ? Convert.ToDateTime(date) : new DateTime(),
                    IdClient = 0,
                    Clients = _combosHelper.GetComboTCMClientsByCaseManager(user_logged.UserName)
                };
            }            

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> AddProgressNote(AddProgressNoteViewModel model, IFormCollection form)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                if (form["Billable"] == "Value1")
                {
                    return RedirectToAction("Create", "TCMNotes", new { dateTime = model.Date, IdTCMClient = model.IdClient, origin = 1 });                          
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult Events(string start, string end, int idClient, int idCaseManager = 0)
        {
            HttpContext.Session.SetString("initDate", start);
            HttpContext.Session.SetString("finalDate", end);
            HttpContext.Session.SetInt32("idClient", idClient);
            HttpContext.Session.SetInt32("idCaseManager", idCaseManager);

            DateTime initDate = Convert.ToDateTime(start);
            DateTime finalDate = Convert.ToDateTime(end);            

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idCaseManager == 0)
            {
                if (idClient == 0)
                {
                    var events = _context.TCMNoteActivity
                                         .Where(t => (t.TCMNote.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                                  && t.TCMNote.DateOfService >= initDate && t.TCMNote.DateOfService <= finalDate))
                                         .Select(t => new
                                         {
                                             //id = t.TCMNote.Id,
                                             title = t.ServiceName.ToString(),
                                             start = t.StartTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                             end = t.EndTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                             url = Url.Action("Edit", "TCMNotes", new { id = t.TCMNote.Id, origin = 2 }),
                                             backgroundColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#fcf8e3" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#d9edf7" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#dff0d8" : "#dff0d8",
                                             textColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#9e7d67" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#487c93" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#417c49" : "#417c49",
                                             borderColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#9e7d67" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#487c93" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#417c49" : "#417c49"
                                         })
                                         .ToList();

                    return new JsonResult(events);
                }
                else
                {
                    var events = _context.TCMNoteActivity
                                         .Where(t => (t.TCMNote.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                                  && t.TCMNote.TCMClient.Id == idClient
                                                  && t.TCMNote.DateOfService >= initDate && t.TCMNote.DateOfService <= finalDate))
                                         .Select(t => new
                                         {
                                             //id = t.TCMNote.Id,
                                             title = t.ServiceName.ToString(),
                                             start = t.StartTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                             end = t.EndTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                             url = Url.Action("Edit", "TCMNotes", new { id = t.TCMNote.Id, origin = 2 }),
                                             backgroundColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#fcf8e3" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#d9edf7" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#dff0d8" : "#dff0d8",
                                             textColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#9e7d67" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#487c93" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#417c49" : "#417c49",
                                             borderColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#9e7d67" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#487c93" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#417c49" : "#417c49"
                                         })
                                         .ToList();

                    return new JsonResult(events);
                }
            }
            else
            {
                var events = _context.TCMNoteActivity
                                     .Where(t => (t.TCMNote.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                               && t.TCMNote.TCMClient.Id == idClient
                                               && t.TCMNote.DateOfService >= initDate && t.TCMNote.DateOfService <= finalDate))
                                         .Select(t => new
                                         {
                                             //id = t.TCMNote.Id,
                                             title = t.ServiceName.ToString(),
                                             start = t.StartTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                             end = t.EndTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                             url = Url.Action("Edit", "TCMNotes", new { id = t.TCMNote.Id, origin = 2 }),
                                             backgroundColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#fcf8e3" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#d9edf7" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#dff0d8" : "#dff0d8",
                                             textColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#9e7d67" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#487c93" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#417c49" : "#417c49",
                                             borderColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#9e7d67" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#487c93" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#417c49" : "#417c49"
                                         })
                                         .ToList();

                return new JsonResult(events);
            }
           
        }

        [Authorize(Roles = "CaseManager")]
        public JsonResult GetTotalNotes()
        {
            DateTime initDate = Convert.ToDateTime(HttpContext.Session.GetString("initDate"));
            DateTime finalDate = Convert.ToDateTime(HttpContext.Session.GetString("finalDate"));
            int idClient = HttpContext.Session.GetInt32("idClient") == null ? 0 : (int)HttpContext.Session.GetInt32("idClient");

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idClient == 0)
            {
                int count = _context.TCMNote
                                    .Where(t => (t.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                           && t.DateOfService >= initDate && t.DateOfService <= finalDate))
                                    .ToList()
                                    .Count();

                return Json(count);
            }
            else
            {
                int count = _context.TCMNote
                                    .Where(t => (t.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                           && t.TCMClient.Id == idClient
                                           && t.DateOfService >= initDate && t.DateOfService <= finalDate))
                                    .ToList()
                                    .Count();

                return Json(count);
            }            
        }

        [Authorize(Roles = "CaseManager")]
        public JsonResult GetTotalUnits()
        {
            DateTime initDate = Convert.ToDateTime(HttpContext.Session.GetString("initDate"));
            DateTime finalDate = Convert.ToDateTime(HttpContext.Session.GetString("finalDate"));
            int idClient = HttpContext.Session.GetInt32("idClient") == null ? 0 : (int)HttpContext.Session.GetInt32("idClient");

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idClient == 0)
            {
                IEnumerable<TCMNoteEntity> notes = _context.TCMNote

                                                           .Include(t => t.TCMNoteActivity)

                                                           .Where(t => (t.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                                                     && t.DateOfService >= initDate && t.DateOfService <= finalDate));

                int minutes;
                int totalUnits = 0;
                int value;
                int mod;
                foreach (TCMNoteEntity item in notes)
                {
                    minutes = item.TCMNoteActivity.Sum(t => t.Minutes);
                    value = minutes / 15;
                    mod = minutes % 15;
                    totalUnits = (mod > 7) ? totalUnits + value + 1 : totalUnits + value;
                }

                return Json(totalUnits);
            }
            else
            {
                IEnumerable<TCMNoteEntity> notes = _context.TCMNote

                                                           .Include(t => t.TCMNoteActivity)

                                                           .Where(t => (t.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                                                     && t.TCMClient.Id == idClient
                                                                     && t.DateOfService >= initDate && t.DateOfService <= finalDate));

                int minutes;
                int totalUnits = 0;
                int value;
                int mod;
                foreach (TCMNoteEntity item in notes)
                {
                    minutes = item.TCMNoteActivity.Sum(t => t.Minutes);
                    value = minutes / 15;
                    mod = minutes % 15;
                    totalUnits = (mod > 7) ? totalUnits + value + 1 : totalUnits + value;
                }

                return Json(totalUnits);
            }
        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor")]
        public IActionResult BillingForWeek(string dateInterval = "", int idCaseManager = 0, int idClient = 0, int billed = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            ViewBag.Clients = "0";
            ViewBag.Notes = "0";
            ViewBag.Services = "0";
            ViewBag.Units = "0";
            ViewBag.Money = "0";
            ViewData["billed"] = billed;
            List<TCMNoteEntity> list = new List<TCMNoteEntity>();

            if (dateInterval == string.Empty)
            {
                dateInterval = DateTime.Today.AddDays(-30).ToShortDateString() + " - " + DateTime.Today.ToShortDateString();
            }
            if (dateInterval != string.Empty)
            {
                string[] date = dateInterval.Split(" - ");
                if (billed == 0)
                {
                    if (User.IsInRole("Manager"))
                    {
                        IQueryable<TCMNoteEntity> query = _context.TCMNote
                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_Diagnostics)
                                                                  .ThenInclude(cd => cd.Diagnostic)

                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                                  .ThenInclude(cd => cd.HealthInsurance)

                                                                  .Include(t => t.TCMClient.Casemanager)
                                                                  .Include(t => t.TCMNoteActivity)

                                                                  .Where(t => (t.DateOfService >= Convert.ToDateTime(date[0]) && t.DateOfService <= Convert.ToDateTime(date[1])));

                        if (idCaseManager != 0)
                            query = query.Where(t => t.TCMClient.Casemanager.Id == idCaseManager);

                        if (idClient != 0)
                            query = query.Where(t => t.TCMClient.Id == idClient);

                        try
                        {
                            list = query.ToList();
                        }
                        catch (Exception)
                        {
                            return RedirectToAction(nameof(BillingForWeek));
                        }
                    }
                    if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor")) 
                    {
                        IQueryable<TCMNoteEntity> query = _context.TCMNote
                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_Diagnostics)
                                                                  .ThenInclude(cd => cd.Diagnostic)

                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                                  .ThenInclude(cd => cd.HealthInsurance)

                                                                  .Include(t => t.TCMClient.Casemanager)
                                                                  .Include(t => t.TCMNoteActivity)

                                                                  .Where(t => (t.ApprovedDate >= Convert.ToDateTime(date[0]) && t.ApprovedDate <= Convert.ToDateTime(date[1])));

                        if (idCaseManager != 0)
                            query = query.Where(t => t.TCMClient.Casemanager.Id == idCaseManager);

                        if (idClient != 0)
                            query = query.Where(t => t.TCMClient.Id == idClient);

                        try
                        {
                            list = query.ToList();
                        }
                        catch (Exception)
                        {
                            return RedirectToAction(nameof(BillingForWeek));
                        }
                    }
                }
                if (billed == 1)
                {
                    if (User.IsInRole("Manager"))
                    {
                        IQueryable<TCMNoteEntity> query = _context.TCMNote
                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_Diagnostics)
                                                                  .ThenInclude(cd => cd.Diagnostic)

                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                                  .ThenInclude(cd => cd.HealthInsurance)

                                                                  .Include(t => t.TCMClient.Casemanager)
                                                                  .Include(t => t.TCMNoteActivity)

                                                                  .Where(t => (t.DateOfService >= Convert.ToDateTime(date[0])
                                                                            && t.DateOfService <= Convert.ToDateTime(date[1])
                                                                            && t.BilledDate == null));

                        if (idCaseManager != 0)
                            query = query.Where(t => t.TCMClient.Casemanager.Id == idCaseManager);

                        if (idClient != 0)
                            query = query.Where(t => t.TCMClient.Id == idClient);

                        try
                        {
                            list = query.ToList();
                        }
                        catch (Exception)
                        {
                            return RedirectToAction(nameof(BillingForWeek));
                        }
                    }
                    if(User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
                    {
                        IQueryable<TCMNoteEntity> query = _context.TCMNote
                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_Diagnostics)
                                                                  .ThenInclude(cd => cd.Diagnostic)

                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                                  .ThenInclude(cd => cd.HealthInsurance)

                                                                  .Include(t => t.TCMClient.Casemanager)
                                                                  .Include(t => t.TCMNoteActivity)

                                                                  .Where(t => (t.ApprovedDate >= Convert.ToDateTime(date[0])
                                                                            && t.ApprovedDate <= Convert.ToDateTime(date[1])
                                                                            && t.BilledDate == null));

                        if (idCaseManager != 0)
                            query = query.Where(t => t.TCMClient.Casemanager.Id == idCaseManager);

                        if (idClient != 0)
                            query = query.Where(t => t.TCMClient.Id == idClient);

                        try
                        {
                            list = query.ToList();
                        }
                        catch (Exception)
                        {
                            return RedirectToAction(nameof(BillingForWeek));
                        }
                    }
                }
                if (billed == 2)
                {
                    if (User.IsInRole("Manager"))
                    {
                        IQueryable<TCMNoteEntity> query = _context.TCMNote
                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_Diagnostics)
                                                                  .ThenInclude(cd => cd.Diagnostic)

                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                                  .ThenInclude(cd => cd.HealthInsurance)

                                                                  .Include(t => t.TCMClient.Casemanager)
                                                                  .Include(t => t.TCMNoteActivity)

                                                                  .Where(t => (t.DateOfService >= Convert.ToDateTime(date[0])
                                                                            && t.DateOfService <= Convert.ToDateTime(date[1])
                                                                            && t.BilledDate != null));

                        if (idCaseManager != 0)
                            query = query.Where(t => t.TCMClient.Casemanager.Id == idCaseManager);

                        if (idClient != 0)
                            query = query.Where(t => t.TCMClient.Id == idClient);

                        try
                        {
                            list = query.ToList();
                        }
                        catch (Exception)
                        {
                            return RedirectToAction(nameof(BillingForWeek));
                        }
                    }
                    if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
                    {
                        IQueryable<TCMNoteEntity> query = _context.TCMNote
                                                                      .Include(t => t.TCMClient)
                                                                      .ThenInclude(c => c.Client)
                                                                      .ThenInclude(cl => cl.Clients_Diagnostics)
                                                                      .ThenInclude(cd => cd.Diagnostic)

                                                                      .Include(t => t.TCMClient)
                                                                      .ThenInclude(c => c.Client)
                                                                      .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                                      .ThenInclude(cd => cd.HealthInsurance)

                                                                      .Include(t => t.TCMClient.Casemanager)
                                                                      .Include(t => t.TCMNoteActivity)

                                                                      .Where(t => (t.ApprovedDate >= Convert.ToDateTime(date[0])
                                                                                && t.ApprovedDate <= Convert.ToDateTime(date[1])
                                                                                && t.BilledDate != null));

                        if (idCaseManager != 0)
                            query = query.Where(t => t.TCMClient.Casemanager.Id == idCaseManager);

                        if (idClient != 0)
                            query = query.Where(t => t.TCMClient.Id == idClient);

                        try
                        {
                            list = query.ToList();
                        }
                        catch (Exception)
                        {
                            return RedirectToAction(nameof(BillingForWeek));
                        }
                    }
                }
                if (billed == 3)
                {
                    if (User.IsInRole("Manager"))
                    {
                        IQueryable<TCMNoteEntity> query = _context.TCMNote
                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_Diagnostics)
                                                                  .ThenInclude(cd => cd.Diagnostic)

                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                                  .ThenInclude(cd => cd.HealthInsurance)

                                                                  .Include(t => t.TCMClient.Casemanager)
                                                                  .Include(t => t.TCMNoteActivity)

                                                                  .Where(t => (t.DateOfService >= Convert.ToDateTime(date[0])
                                                                            && t.DateOfService <= Convert.ToDateTime(date[1])
                                                                            && t.BilledDate != null
                                                                            && t.PaymentDate == null));

                        if (idCaseManager != 0)
                            query = query.Where(t => t.TCMClient.Casemanager.Id == idCaseManager);

                        if (idClient != 0)
                            query = query.Where(t => t.TCMClient.Id == idClient);

                        try
                        {
                            list = query.ToList();
                        }
                        catch (Exception)
                        {
                            return RedirectToAction(nameof(BillingForWeek));
                        }
                    }
                    if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
                    {
                        IQueryable<TCMNoteEntity> query = _context.TCMNote
                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_Diagnostics)
                                                                  .ThenInclude(cd => cd.Diagnostic)

                                                                  .Include(t => t.TCMClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                                  .ThenInclude(cd => cd.HealthInsurance)

                                                                  .Include(t => t.TCMClient.Casemanager)
                                                                  .Include(t => t.TCMNoteActivity)

                                                                  .Where(t => (t.ApprovedDate >= Convert.ToDateTime(date[0])
                                                                            && t.ApprovedDate <= Convert.ToDateTime(date[1])
                                                                            && t.BilledDate != null
                                                                            && t.PaymentDate == null));

                        if (idCaseManager != 0)
                            query = query.Where(t => t.TCMClient.Casemanager.Id == idCaseManager);

                        if (idClient != 0)
                            query = query.Where(t => t.TCMClient.Id == idClient);

                        try
                        {
                            list = query.ToList();
                        }
                        catch (Exception)
                        {
                            return RedirectToAction(nameof(BillingForWeek));
                        }
                    }
                }
                int minutes;
                int totalUnits = 0;
                int value;
                int mod;
                foreach (TCMNoteEntity item in list)
                {
                    foreach (var activity in item.TCMNoteActivity)
                    {
                        minutes = activity.Minutes;
                        value = minutes / 15;
                        mod = minutes % 15;
                        totalUnits = (mod > 7) ? totalUnits + value + 1 : totalUnits + value;
                    }

                    
                }
                
                ViewBag.Clients = list.GroupBy(n => n.TCMClient).Count().ToString();
                ViewBag.Notes = list.Count().ToString();
                ViewBag.Services = list.Sum(n => n.TCMNoteActivity.Count()).ToString();
                ViewBag.Units = totalUnits.ToString();
                ViewBag.Money = (totalUnits * 12).ToString();
            }
            
            TCMBillingReportViewModel model = new TCMBillingReportViewModel
            {
                DateIterval = dateInterval,
                IdCaseManager = idCaseManager,
                CaseManagers = _combosHelper.GetComboCaseMannagersByClinicFilter(user_logged.Clinic.Id),
                IdClient = idClient,
                Clients = _combosHelper.GetComboTCMClientsByClinic(user_logged.Clinic.Id),
                TCMNotes = list
            };
            if (User.IsInRole("CaseManager"))
            {
                model.Clients = _combosHelper.GetComboTCMClientsByCaseManager(user_logged.UserName);
            }
            if (User.IsInRole("TCMSupervisor"))
            {
                model.CaseManagers = _combosHelper.GetComboCaseManagersByTCMSupervisor(user_logged.UserName);
                model.Clients = _combosHelper.GetComboTCMClientsByCaseManagerByTCMSupervisor(user_logged.UserName);
                
            }

            return View(model);                   
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,CaseManager, TCMSupervisor")]
        public IActionResult BillingForWeek(TCMBillingReportViewModel model, int billed = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(BillingForWeek), new { dateInterval = model.DateIterval, idCaseManager = model.IdCaseManager, idClient = model.IdClient, billed = billed});
            }     
            
            return View(model);            
        }

        [Authorize(Roles = "CaseManager")]
        public JsonResult GetTotalMoney()
        {
            DateTime initDate = Convert.ToDateTime(HttpContext.Session.GetString("initDate"));
            DateTime finalDate = Convert.ToDateTime(HttpContext.Session.GetString("finalDate"));
            int idClient = HttpContext.Session.GetInt32("idClient") == null ? 0 : (int)HttpContext.Session.GetInt32("idClient");

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idClient == 0)
            {
                List <TCMNoteEntity> notes = _context.TCMNote
                                                     .Include(t => t.TCMNoteActivity)
                                                     .Include(t => t.TCMClient)
                                                     .ThenInclude(t => t.Casemanager)
                                                     .Where(t => (t.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                                                     && t.DateOfService >= initDate && t.DateOfService <= finalDate))
                                                     .ToList();


                decimal totalMinutes = 0;
                decimal valor = new decimal(0.00);
                decimal money = new decimal(0.00);
                foreach (TCMNoteEntity item in notes)
                {
                    totalMinutes = totalMinutes + item.TCMNoteActivity.Sum(t => t.Minutes);
                }
                if (notes.Count() > 0)
                {
                    valor = notes.ElementAt(0).TCMClient.Casemanager.Money / 60;
                    money = totalMinutes * valor;
                    return Json(decimal.Round(money, 2));
                }
                else
                {
                    return Json("0.00");
                }

            }
            else
            {
                List<TCMNoteEntity> notes = _context.TCMNote
                                                    .Include(t => t.TCMNoteActivity)
                                                    .Include(t => t.TCMClient)
                                                    .ThenInclude(t => t.Casemanager)
                                                    .Where(t => (t.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                                              && t.TCMClient.Id == idClient
                                                              && t.DateOfService >= initDate && t.DateOfService <= finalDate))
                                                    .ToList();

                decimal totalMinutes = 0;
                decimal valor = new decimal();
                decimal money = new decimal();
                foreach (TCMNoteEntity item in notes)
                {
                    totalMinutes = totalMinutes + item.TCMNoteActivity.Sum(t => t.Minutes);
                }
                if (notes.Count() > 0)
                {
                    valor = notes.ElementAt(0).TCMClient.Casemanager.Money / 60;
                    money = totalMinutes * valor;
                    return Json(decimal.Round(money, 2));
                }
                else
                {
                    return Json("0.00");
                }

            }
        }

        [Authorize(Roles = "CaseManager")]
        public JsonResult GetTotalMinutes()
        {
            DateTime initDate = Convert.ToDateTime(HttpContext.Session.GetString("initDate"));
            DateTime finalDate = Convert.ToDateTime(HttpContext.Session.GetString("finalDate"));
            int idClient = HttpContext.Session.GetInt32("idClient") == null ? 0 : (int)HttpContext.Session.GetInt32("idClient");

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idClient == 0)
            {
                IEnumerable<TCMNoteEntity> notes = _context.TCMNote
                                                           .Include(t => t.TCMNoteActivity)
                                                           .Include(t => t.TCMClient)
                                                           .ThenInclude(t => t.Casemanager)
                                                           .Where(t => (t.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                                                     && t.DateOfService >= initDate && t.DateOfService <= finalDate));

                int minutes = 0;
                int totalMinutes = 0;
                decimal money = 0;
                foreach (TCMNoteEntity item in notes)
                {
                    minutes = item.TCMNoteActivity.Sum(t => t.Minutes);
                    totalMinutes = totalMinutes + minutes;
                }
                if (notes.Count() > 0)
                {
                    money = (totalMinutes / 60 * notes.ElementAt(0).TCMClient.Casemanager.Money);
                }
                else
                {
                    money = 0;
                }
                return Json(totalMinutes);
            }
            else
            {
                IEnumerable<TCMNoteEntity> notes = _context.TCMNote
                                                           .Include(t => t.TCMNoteActivity)
                                                           .Include(t => t.TCMClient)
                                                           .ThenInclude(t => t.Casemanager)
                                                           .Where(t => (t.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                                                     && t.TCMClient.Id == idClient
                                                                     && t.DateOfService >= initDate && t.DateOfService <= finalDate));

                int minutes = 0;
                int totalMinutes = 0;
                decimal money = 0;
                foreach (TCMNoteEntity item in notes)
                {
                    minutes = item.TCMNoteActivity.Sum(t => t.Minutes);
                    totalMinutes = totalMinutes + minutes;
                }
                if (notes.Count() > 0)
                {
                    money = (totalMinutes / 60 * notes.ElementAt(0).TCMClient.Casemanager.Money);
                }
                else
                {
                    money = 0;
                }
                return Json(totalMinutes);
            }
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult TCMSupervisor()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMBillingViewModel model = new TCMBillingViewModel
            {
                IdClient = 0,
                IdCaseManager = 0,
                CaseManagers = _combosHelper.GetComboCaseManagersByTCMSupervisor(user_logged.UserName)
            };

            return View(model);
        }
        
        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult EventsTCM(string start, string end, int idCaseManager = 0)
        {
            HttpContext.Session.SetString("initDate", start);
            HttpContext.Session.SetString("finalDate", end);
            HttpContext.Session.SetInt32("idCaseManager", idCaseManager);

            DateTime initDate = Convert.ToDateTime(start);
            DateTime finalDate = Convert.ToDateTime(end);

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idCaseManager == 0)
            {
                var events = _context.TCMNoteActivity
                                     .Where(t => (t.TCMNote.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                               && t.TCMNote.DateOfService >= initDate && t.TCMNote.DateOfService <= finalDate))
                                         .Select(t => new
                                         {
                                             //id = t.TCMNote.Id,
                                             title = t.ServiceName.ToString(),
                                             start = t.StartTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                             end = t.EndTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                             url = Url.Action("Edit", "TCMNotes", new { id = t.TCMNote.Id, origin = 2 }),
                                             backgroundColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#fcf8e3" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#d9edf7" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#dff0d8" : "#dff0d8",
                                             textColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#9e7d67" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#487c93" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#417c49" : "#417c49",
                                             borderColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#9e7d67" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#487c93" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#417c49" : "#417c49"
                                         })
                                         .ToList();

                return new JsonResult(events);
            }
            else
            {
                var events = _context.TCMNoteActivity
                                     .Where(t => (t.TCMNote.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                               && t.TCMNote.TCMClient.Casemanager.Id == idCaseManager
                                               && t.TCMNote.DateOfService >= initDate && t.TCMNote.DateOfService <= finalDate))
                                         .Select(t => new
                                         {
                                             //id = t.TCMNote.Id,
                                             title = t.ServiceName.ToString(),
                                             start = t.StartTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                             end = t.EndTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                             url = Url.Action("Edit", "TCMNotes", new { id = t.TCMNote.Id, origin = 2 }),
                                             backgroundColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#fcf8e3" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#d9edf7" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#dff0d8" : "#dff0d8",
                                             textColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#9e7d67" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#487c93" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#417c49" : "#417c49",
                                             borderColor = (t.TCMNote.Status == Common.Enums.NoteStatus.Edition) ? "#9e7d67" :
                                                                       (t.TCMNote.Status == Common.Enums.NoteStatus.Pending) ? "#487c93" :
                                                                           (t.TCMNote.Status == Common.Enums.NoteStatus.Approved) ? "#417c49" : "#417c49"
                                         })
                                         .ToList();

                return new JsonResult(events);
            }

        }

        [Authorize(Roles = "TCMSupervisor")]
        public JsonResult GetTotalMoneySupervisor()
        {
            DateTime initDate = Convert.ToDateTime(HttpContext.Session.GetString("initDate"));
            DateTime finalDate = Convert.ToDateTime(HttpContext.Session.GetString("finalDate"));
            int idCaseManager = HttpContext.Session.GetInt32("idCaseManager") == null ? 0 : (int)HttpContext.Session.GetInt32("idCaseManager");

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idCaseManager == 0)
            {
                List<TCMNoteEntity> notes = _context.TCMNote
                                                     .Include(t => t.TCMNoteActivity)
                                                     .Include(t => t.TCMClient)
                                                     .ThenInclude(t => t.Casemanager)
                                                     .Where(t => (t.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                               && t.DateOfService >= initDate && t.DateOfService <= finalDate))
                                                     .ToList();


                decimal totalMinutes = 0;
                decimal valor = new decimal(0.00);
                decimal money = new decimal(0.00);
                foreach (TCMNoteEntity item in notes)
                {
                    totalMinutes = totalMinutes + item.TCMNoteActivity.Sum(t => t.Minutes);
                }
                if (notes.Count() > 0)
                {
                    valor = notes.ElementAt(0).TCMClient.Casemanager.Money / 60;
                    money = totalMinutes * valor;
                    return Json(decimal.Round(money, 2));
                }
                else
                {
                    return Json("0.00");
                }

            }
            else
            {
                List<TCMNoteEntity> notes = _context.TCMNote
                                                    .Include(t => t.TCMNoteActivity)
                                                    .Include(t => t.TCMClient)
                                                    .ThenInclude(t => t.Casemanager)
                                                    .Where(t => (t.TCMClient.Casemanager.Id == idCaseManager
                                                              && t.DateOfService >= initDate && t.DateOfService <= finalDate))
                                                    .ToList();

                decimal totalMinutes = 0;
                decimal valor = new decimal();
                decimal money = new decimal();
                foreach (TCMNoteEntity item in notes)
                {
                    totalMinutes = totalMinutes + item.TCMNoteActivity.Sum(t => t.Minutes);
                }
                if (notes.Count() > 0)
                {
                    valor = notes.ElementAt(0).TCMClient.Casemanager.Money / 60;
                    money = totalMinutes * valor;
                    return Json(decimal.Round(money, 2));
                }
                else
                {
                    return Json("0.00");
                }

            }
        }

        [Authorize(Roles = "TCMSupervisor")]
        public JsonResult GetTotalMinutesSupervisor()
        {
            DateTime initDate = Convert.ToDateTime(HttpContext.Session.GetString("initDate"));
            DateTime finalDate = Convert.ToDateTime(HttpContext.Session.GetString("finalDate"));
            int idCaseManager = HttpContext.Session.GetInt32("idCaseManager") == null ? 0 : (int)HttpContext.Session.GetInt32("idCaseManager");

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idCaseManager == 0)
            {
                IEnumerable<TCMNoteEntity> notes = _context.TCMNote
                                                           .Include(t => t.TCMNoteActivity)
                                                           .Include(t => t.TCMClient)
                                                           .ThenInclude(t => t.Casemanager)
                                                           .ThenInclude(t => t.TCMSupervisor)
                                                           .Where(t => (t.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                                     && t.DateOfService >= initDate && t.DateOfService <= finalDate));

                int minutes = 0;
                int totalMinutes = 0;
                decimal money = 0;
                foreach (TCMNoteEntity item in notes)
                {
                    minutes = item.TCMNoteActivity.Sum(t => t.Minutes);
                    totalMinutes = totalMinutes + minutes;
                    money += minutes / 60 * item.TCMClient.Casemanager.Money;
                }
              
                return Json(totalMinutes);
            }
            else
            {
                IEnumerable<TCMNoteEntity> notes = _context.TCMNote
                                                           .Include(t => t.TCMNoteActivity)
                                                           .Include(t => t.TCMClient)
                                                           .ThenInclude(t => t.Casemanager)
                                                           .Where(t => (t.TCMClient.Casemanager.Id == idCaseManager
                                                                     && t.DateOfService >= initDate && t.DateOfService <= finalDate));

                int minutes = 0;
                int totalMinutes = 0;
                decimal money = 0;
                foreach (TCMNoteEntity item in notes)
                {
                    minutes = item.TCMNoteActivity.Sum(t => t.Minutes);
                    totalMinutes = totalMinutes + minutes;
                    money += minutes / 60 * item.TCMClient.Casemanager.Money;
                }
               
                return Json(totalMinutes);
            }
        }

        [Authorize(Roles = "TCMSupervisor")]
        public JsonResult GetTotalNotesSupervisor()
        {
            DateTime initDate = Convert.ToDateTime(HttpContext.Session.GetString("initDate"));
            DateTime finalDate = Convert.ToDateTime(HttpContext.Session.GetString("finalDate"));
            int idCaseManager = HttpContext.Session.GetInt32("idCaseManager") == null ? 0 : (int)HttpContext.Session.GetInt32("idCaseManager");

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idCaseManager == 0)
            {
                int count = _context.TCMNote
                                    .Where(t => (t.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                           && t.DateOfService >= initDate && t.DateOfService <= finalDate))
                                    .ToList()
                                    .Count();

                return Json(count);
            }
            else
            {
                int count = _context.TCMNote
                                    .Where(t => (t.TCMClient.Casemanager.Id == idCaseManager
                                           && t.DateOfService >= initDate && t.DateOfService <= finalDate))
                                    .ToList()
                                    .Count();

                return Json(count);
            }
        }

        [Authorize(Roles = "TCMSupervisor")]
        public JsonResult GetTotalUnitsSupervisor()
        {
            DateTime initDate = Convert.ToDateTime(HttpContext.Session.GetString("initDate"));
            DateTime finalDate = Convert.ToDateTime(HttpContext.Session.GetString("finalDate"));
            int idCaseManager = HttpContext.Session.GetInt32("idCaseManager") == null ? 0 : (int)HttpContext.Session.GetInt32("idCaseManager");

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idCaseManager == 0)
            {
                IEnumerable<TCMNoteEntity> notes = _context.TCMNote

                                                           .Include(t => t.TCMNoteActivity)

                                                           .Where(t => (t.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                                     && t.DateOfService >= initDate && t.DateOfService <= finalDate));

                int minutes;
                int totalUnits = 0;
                int value;
                int mod;
                foreach (TCMNoteEntity item in notes)
                {
                    minutes = item.TCMNoteActivity.Sum(t => t.Minutes);
                    value = minutes / 15;
                    mod = minutes % 15;
                    totalUnits = (mod > 7) ? totalUnits + value + 1 : totalUnits + value;
                }

                return Json(totalUnits);
            }
            else
            {
                IEnumerable<TCMNoteEntity> notes = _context.TCMNote

                                                           .Include(t => t.TCMNoteActivity)

                                                           .Where(t => (t.TCMClient.Casemanager.Id == idCaseManager
                                                                     && t.DateOfService >= initDate && t.DateOfService <= finalDate));

                int minutes;
                int totalUnits = 0;
                int value;
                int mod;
                foreach (TCMNoteEntity item in notes)
                {
                    minutes = item.TCMNoteActivity.Sum(t => t.Minutes);
                    value = minutes / 15;
                    mod = minutes % 15;
                    totalUnits = (mod > 7) ? totalUnits + value + 1 : totalUnits + value;
                }

                return Json(totalUnits);
            }
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateBill(int billed = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            ViewBag.Clients = "0";
            ViewBag.Notes = "0";
            ViewBag.Services = "0";
            ViewBag.Units = "0";
            ViewBag.Money = "0";
            ViewData["billed"] = billed;
            List<TCMClientEntity> list = new List<TCMClientEntity>();

            if (billed == 1)
            {
                list = await _context.TCMClient
                                     .Include(c => c.Client)
                                     .ThenInclude(cl => cl.Clients_Diagnostics)
                                     .ThenInclude(cd => cd.Diagnostic)

                                     .Include(c => c.Client)
                                     .ThenInclude(cl => cl.Clients_HealthInsurances)
                                     .ThenInclude(cl => cl.HealthInsurance)

                                     .Include(t => t.Casemanager)
                                     .Include(c => c.TCMNote)
                                     .ThenInclude(t => t.TCMNoteActivity)

                                     .Where(t => t.TCMNote.Where(n => n.BilledDate == null).Count() > 0)
                                     .ToListAsync();

            }
            if (billed == 2)
            {
                list = await _context.TCMClient
                                    .Include(c => c.Client)
                                    .ThenInclude(cl => cl.Clients_Diagnostics)
                                    .ThenInclude(cd => cd.Diagnostic)

                                    .Include(c => c.Client)
                                    .ThenInclude(cl => cl.Clients_HealthInsurances)
                                    .ThenInclude(cl => cl.HealthInsurance)

                                    .Include(t => t.Casemanager)
                                    .Include(c => c.TCMNote)
                                    .ThenInclude(t => t.TCMNoteActivity)

                                    .Where(t => t.TCMNote.Where(n => n.PaymentDate == null
                                                                  && n.BilledDate != null).Count() > 0)
                                    .ToListAsync();

            }
            if (billed == 0)
            {
                list = await _context.TCMClient
                                     .Include(c => c.Client)
                                     .ThenInclude(cl => cl.Clients_Diagnostics)
                                     .ThenInclude(cd => cd.Diagnostic)

                                     .Include(c => c.Client)
                                     .ThenInclude(cl => cl.Clients_HealthInsurances)
                                     .ThenInclude(cl => cl.HealthInsurance)

                                     .Include(t => t.Casemanager)
                                     .Include(c => c.TCMNote)
                                     .ThenInclude(t => t.TCMNoteActivity)

                                     .ToListAsync();

            }
            int minutes;
            int totalUnits = 0;
            int value;
            int mod;
            int notes = 0;
            int services = 0;
            foreach (TCMClientEntity item in list)
            {
                if (billed == 1)
                {
                    foreach (var note in item.TCMNote.Where(n => n.BilledDate == null))
                    {
                        foreach (var activity in note.TCMNoteActivity)
                        {
                            minutes = activity.Minutes;
                            value = minutes / 15;
                            mod = minutes % 15;
                            totalUnits = (mod > 7) ? totalUnits + value + 1 : totalUnits + value;
                            services ++;
                        }
                        notes++;
                    }
                }
                if (billed == 2)
                {
                    foreach (var note in item.TCMNote.Where(n => n.BilledDate != null && n.PaymentDate == null))
                    {
                        foreach (var activity in note.TCMNoteActivity)
                        {
                            minutes = activity.Minutes;
                            value = minutes / 15;
                            mod = minutes % 15;
                            totalUnits = (mod > 7) ? totalUnits + value + 1 : totalUnits + value;
                            services++;
                        }
                        notes++;
                    }
                }
            }
            
            ViewBag.Clients = list.Count().ToString();
            ViewBag.Notes = notes;
            ViewBag.Services = services;
            ViewBag.Units = totalUnits.ToString();
            ViewBag.Money = (totalUnits * 12).ToString();

            return View(list);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> BillTCMNoteToday(int id = 0)
        {
            if (id != 0)
            {
                TCMNoteEntity note = await _context.TCMNote
                                                   .Where(wc => wc.Id == id)
                                                   .FirstOrDefaultAsync();

                note.BilledDate = DateTime.Now;
                _context.Update(note);
                await _context.SaveChangesAsync();

                return RedirectToAction("UpdateBill", "TCMBilling", new { billed = 1 });
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> NotTMCNoteBill(int id = 0)
        {
            if (id != 0)
            {
                TCMNoteEntity note = await _context.TCMNote
                                                   .Where(wc => wc.Id == id)
                                                   .FirstOrDefaultAsync();

                note.BilledDate = null;
                _context.Update(note);
                await _context.SaveChangesAsync();

                return RedirectToAction("UpdateBill", "TCMBilling", new { billed = 2});
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> BillTCMClientToday(int id = 0)
        {
            if (id != 0)
            {
                List<TCMNoteEntity> notes = await _context.TCMNote
                                                          .Where(n => n.TCMClient.Id == id
                                                                   && n.BilledDate == null)
                                                          .ToListAsync();

                List<TCMNoteEntity> salida = new List<TCMNoteEntity>();

                foreach (var item in notes)
                {
                    item.BilledDate = DateTime.Today;
                    salida.Add(item);
                }

                _context.UpdateRange(salida);
                await _context.SaveChangesAsync();

                return RedirectToAction("UpdateBill", "TCMBilling", new { billed = 1 });
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> NotTCMClientToday(int id = 0)
        {
            if (id != 0)
            {
                List<TCMNoteEntity> notes = await _context.TCMNote
                                                          .Where(n => n.TCMClient.Id == id
                                                                   && n.BilledDate != null
                                                                   && n.PaymentDate == null)
                                                          .ToListAsync();

                List<TCMNoteEntity> salida = new List<TCMNoteEntity>();

                foreach (var item in notes)
                {
                    item.BilledDate = null;
                    salida.Add(item);
                }

                _context.UpdateRange(salida);
                await _context.SaveChangesAsync();

                return RedirectToAction("UpdateBill", "TCMBilling", new { billed = 2 });
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> PayTCMNoteToday(int id = 0)
        {
            if (id != 0)
            {
                TCMNoteEntity note = await _context.TCMNote
                                                   .Where(wc => wc.Id == id)
                                                   .FirstOrDefaultAsync();

                note.PaymentDate = DateTime.Now;
                _context.Update(note);
                await _context.SaveChangesAsync();

                return RedirectToAction("UpdateBill", "TCMBilling", new { billed = 2 });
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> PayTCMClientToday(int id = 0)
        {
            if (id != 0)
            {
                List<TCMNoteEntity> notes = await _context.TCMNote
                                                          .Where(n => n.TCMClient.Id == id
                                                                   && n.BilledDate != null
                                                                   && n.PaymentDate == null)
                                                          .ToListAsync();

                List<TCMNoteEntity> salida = new List<TCMNoteEntity>();

                foreach (var item in notes)
                {
                    item.PaymentDate = DateTime.Today;
                    salida.Add(item);
                }

                _context.UpdateRange(salida);
                await _context.SaveChangesAsync();

                return RedirectToAction("UpdateBill", "TCMBilling", new { billed = 2 });
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public IActionResult BillTCMNote(int id, int week = 0, int abilled = 0)
        {
            BillViewModel model = new BillViewModel { Id = id, BilledDate = DateTime.Now };
            ViewData["Billed"] = abilled;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> BillTCMNote(BillViewModel model, int week = 0, int abilled = 0)
        {

            if (abilled == 1)
            {
                TCMNoteEntity note =  _context.TCMNote
                                              .Where(wc => wc.Id == model.Id)
                                              .FirstOrDefault();

                note.BilledDate = model.BilledDate;
                _context.Update(note);
                _context.SaveChanges();

                List<TCMClientEntity> list = _context.TCMClient
                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_Diagnostics)
                                                     .ThenInclude(cd => cd.Diagnostic)

                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                     .ThenInclude(cl => cl.HealthInsurance)

                                                     .Include(t => t.Casemanager)
                                                     .Include(c => c.TCMNote)
                                                     .ThenInclude(t => t.TCMNoteActivity)

                                                     .Where(t => t.TCMNote.Where(n => n.BilledDate == null).Count() > 0)
                                                     .ToList();
                ViewData["billed"] = abilled;
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewNotes", list) });
            }
            if (abilled == 2)
            {
                TCMNoteEntity note = _context.TCMNote
                                             .Where(wc => wc.Id == model.Id)
                                             .FirstOrDefault();

                note.BilledDate = model.BilledDate;
                _context.Update(note);
                _context.SaveChanges();

                List<TCMClientEntity> list = _context.TCMClient
                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_Diagnostics)
                                                     .ThenInclude(cd => cd.Diagnostic)

                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                     .ThenInclude(cl => cl.HealthInsurance)

                                                     .Include(t => t.Casemanager)
                                                     .Include(c => c.TCMNote)
                                                     .ThenInclude(t => t.TCMNoteActivity)

                                                     .Where(t => t.TCMNote.Where(n => n.BilledDate != null && n.PaymentDate == null).Count() > 0)
                                                     .ToList();

                ViewData["billed"] = abilled;
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewNotes", list) });
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "BillTCMNote", model) });
        }

        [Authorize(Roles = "Manager")]
        public IActionResult BillTCMClient(int id, int abilled = 0)
        {
            BillViewModel model = new BillViewModel { Id = id, BilledDate = DateTime.Now };
            ViewData["Billed"] = abilled;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> BillTCMClient(BillViewModel model, int abilled = 0)
        {

            if (abilled == 1)
            {
                List<TCMNoteEntity> notes = _context.TCMNote
                                                    .Where(n => n.TCMClient.Id == model.Id
                                                             && n.BilledDate == null)
                                                    .ToList();
                
                List<TCMNoteEntity> salida = new List<TCMNoteEntity>();

                foreach (TCMNoteEntity item in notes)
                {
                    item.BilledDate = model.BilledDate;
                    salida.Add(item);
                }

                _context.UpdateRange(salida);
                _context.SaveChanges();

                List<TCMClientEntity> list = _context.TCMClient
                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_Diagnostics)
                                                     .ThenInclude(cd => cd.Diagnostic)

                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                     .ThenInclude(cl => cl.HealthInsurance)

                                                     .Include(t => t.Casemanager)
                                                     .Include(c => c.TCMNote)
                                                     .ThenInclude(t => t.TCMNoteActivity)

                                                     .Where(t => t.TCMNote.Where(n => n.BilledDate == null).Count() > 0)
                                                     .ToList();
                ViewData["billed"] = abilled;
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewNotes", list) });
            }
            if (abilled == 2)
            {
                List<TCMNoteEntity> notes = _context.TCMNote
                                                    .Where(n => n.TCMClient.Id == model.Id
                                                             && n.BilledDate != null
                                                             && n.PaymentDate == null)
                                                    .ToList();

                List<TCMNoteEntity> salida = new List<TCMNoteEntity>();

                foreach (TCMNoteEntity item in notes)
                {
                    item.BilledDate = model.BilledDate;
                    salida.Add(item);
                }

                _context.UpdateRange(salida);
                _context.SaveChanges();

                List<TCMClientEntity> list = _context.TCMClient
                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_Diagnostics)
                                                     .ThenInclude(cd => cd.Diagnostic)

                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                     .ThenInclude(cl => cl.HealthInsurance)

                                                     .Include(t => t.Casemanager)
                                                     .Include(c => c.TCMNote)
                                                     .ThenInclude(t => t.TCMNoteActivity)

                                                     .Where(t => t.TCMNote.Where(n => n.BilledDate != null 
                                                                                   && n.PaymentDate == null).Count() > 0)
                                                     .ToList();

                ViewData["billed"] = abilled;
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewNotes", list) });
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "BillTCMNote", model) });
        }

        [Authorize(Roles = "Manager")]
        public IActionResult PayTCMNote(int id, int week = 0, int abilled = 0)
        {
            BillViewModel model = new BillViewModel { Id = id, BilledDate = DateTime.Now };
            ViewData["Billed"] = abilled;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> PayTCMNote(BillViewModel model, int week = 0, int abilled = 0)
        {
            if (abilled == 2)
            {
                TCMNoteEntity note = _context.TCMNote
                                             .Where(wc => wc.Id == model.Id)
                                             .FirstOrDefault();

                note.PaymentDate = model.BilledDate;
                _context.Update(note);
                _context.SaveChanges();

                List<TCMClientEntity> list = _context.TCMClient
                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_Diagnostics)
                                                     .ThenInclude(cd => cd.Diagnostic)

                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                     .ThenInclude(cl => cl.HealthInsurance)

                                                     .Include(t => t.Casemanager)
                                                     .Include(c => c.TCMNote)
                                                     .ThenInclude(t => t.TCMNoteActivity)

                                                     .Where(t => t.TCMNote.Where(n => n.BilledDate != null && n.PaymentDate == null).Count() > 0)
                                                     .ToList();

                ViewData["billed"] = abilled;
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewNotes", list) });
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "BillTCMNote", model) });
        }

        [Authorize(Roles = "Manager")]
        public IActionResult PayTCMClient(int id, int abilled = 0)
        {
            BillViewModel model = new BillViewModel { Id = id, BilledDate = DateTime.Now };
            ViewData["Billed"] = abilled;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> PayTCMClient(BillViewModel model, int abilled = 0)
        {
            if (abilled == 2)
            {
                List<TCMNoteEntity> notes = _context.TCMNote
                                                    .Where(n => n.TCMClient.Id == model.Id
                                                             && n.BilledDate != null
                                                             && n.PaymentDate == null)
                                                    .ToList();

                List<TCMNoteEntity> salida = new List<TCMNoteEntity>();

                foreach (TCMNoteEntity item in notes)
                {
                    item.PaymentDate = model.BilledDate;
                    salida.Add(item);
                }

                _context.UpdateRange(salida);
                _context.SaveChanges();

                List<TCMClientEntity> list = _context.TCMClient
                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_Diagnostics)
                                                     .ThenInclude(cd => cd.Diagnostic)

                                                     .Include(c => c.Client)
                                                     .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                     .ThenInclude(cl => cl.HealthInsurance)

                                                     .Include(t => t.Casemanager)
                                                     .Include(c => c.TCMNote)
                                                     .ThenInclude(t => t.TCMNoteActivity)

                                                     .Where(t => t.TCMNote.Where(n => n.BilledDate != null
                                                                                   && n.PaymentDate == null).Count() > 0)
                                                     .ToList();

                ViewData["billed"] = abilled;
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewNotes", list) });
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "BillTCMNote", model) });
        }

        [Authorize(Roles = "Manager")]
        public IActionResult EXCEL(string dateInterval = "", int all = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            List<TCMNoteEntity> tcmNotes = new List<TCMNoteEntity>();
            string[] week = dateInterval.Split(" - ");
            DateTime initial = DateTime.Parse(week[0]);
            DateTime end = DateTime.Parse(week[1]);

            if (week.Count() > 0)
            {
                string Periodo = "";
                string ReportName = "SuperBill Report " + week[0] + " To " + week[1] + ".xlsx";
                string data = "";
                if (all == 0)
                {
                    tcmNotes = _context.TCMNote
                                       .Include(f => f.TCMClient)
                                       .ThenInclude(f => f.Casemanager)
                                       
                                       .Include(w => w.TCMClient)
                                       .ThenInclude(w => w.Client)
                                       
                                       .ThenInclude(w => w.Clients_Diagnostics)
                                       .ThenInclude(w => w.Diagnostic)

                                       .Include(w => w.TCMClient)
                                       .ThenInclude(w => w.Client)
                                       .ThenInclude(w => w.Clients_HealthInsurances)
                                       .ThenInclude(w => w.HealthInsurance)

                                       .Include(w => w.TCMNoteActivity)

                                       .Where(n => n.TCMClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                                && n.BilledDate == null
                                                && n.DateOfService >= initial
                                                && n.DateOfService <= end)
                                       .OrderBy(n => n.TCMClient.Client.Name)
                                       .ThenBy(n => n.DateOfService)
                                       .ToList();

                    Periodo = week[0] + " - " + week[1];
                    data = "NOT BILLED";
                }
                else
                {
                    tcmNotes = _context.TCMNote
                                        .Include(f => f.TCMClient)
                                        .ThenInclude(f => f.Casemanager)

                                        .Include(w => w.TCMClient)
                                        .ThenInclude(w => w.Client)

                                        .ThenInclude(w => w.Clients_Diagnostics)
                                        .ThenInclude(w => w.Diagnostic)

                                        .Include(w => w.TCMClient)
                                        .ThenInclude(w => w.Client)
                                        .ThenInclude(w => w.Clients_HealthInsurances)
                                        .ThenInclude(w => w.HealthInsurance)

                                        .Include(w => w.TCMNoteActivity)

                                        .Where(n => n.TCMClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                                 && n.DateOfService >= initial
                                                 && n.DateOfService <= end)
                                        .OrderBy(n => n.TCMClient.Client.Name)
                                        .ThenBy(n => n.DateOfService)
                                        .ToList();

                    Periodo = initial.ToLongDateString() + " - " + end.ToLongDateString();
                    data = "ALL DATA";
                }


                byte[] content = _exportExcelHelper.ExportBillTCMHelper(tcmNotes, Periodo, user_logged.Clinic.Name, data);

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ReportName);
            }
            else
            {
                string Periodo = "";
                string ReportName = "SuperBill Unbilled Full Report.xlsx";
                string data = "";
                tcmNotes = _context.TCMNote
                                      .Include(f => f.TCMClient)
                                      .ThenInclude(f => f.Casemanager)

                                      .Include(w => w.TCMClient)
                                      .ThenInclude(w => w.Client)

                                      .ThenInclude(w => w.Clients_Diagnostics)
                                      .ThenInclude(w => w.Diagnostic)

                                      .Include(w => w.TCMClient)
                                      .ThenInclude(w => w.Client)
                                      .ThenInclude(w => w.Clients_HealthInsurances)
                                      .ThenInclude(w => w.HealthInsurance)

                                      .Include(w => w.TCMNoteActivity)

                                      .Where(n => n.TCMClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                               && n.BilledDate == null)
                                      .OrderBy(n => n.TCMClient.Client.Name)
                                      .ThenBy(n => n.DateOfService)
                                      .ToList();

                Periodo = "Unbilled Full Report";
                data = "NOT BILLED";


                byte[] content = _exportExcelHelper.ExportBillTCMHelper(tcmNotes, Periodo, user_logged.Clinic.Name, data);

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ReportName);
                //return RedirectToAction("NotAuthorized", "Account");
            }
        }

    }
}