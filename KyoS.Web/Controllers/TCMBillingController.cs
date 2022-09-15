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

        public TCMBillingController(DataContext context, ICombosHelper combosHelper)
        {
            _context = context;
            _combosHelper = combosHelper;            
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Index()
        {
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

        [Authorize(Roles = "CaseManager")]
        public IActionResult Events(string start, string end, int idClient)
        {
            HttpContext.Session.SetString("initDate", start);
            HttpContext.Session.SetString("finalDate", end);
            HttpContext.Session.SetInt32("idClient", idClient);

            DateTime initDate = Convert.ToDateTime(start);
            DateTime finalDate = Convert.ToDateTime(end);            

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idClient == 0)
            {
                var events = _context.TCMNoteActivity
                                     .Where(t => (t.TCMNote.CaseManager.LinkedUser == user_logged.UserName
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
                                     .Where(t => (t.TCMNote.CaseManager.LinkedUser == user_logged.UserName
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
                                 .Where(t => (t.CaseManager.LinkedUser == user_logged.UserName
                                           && t.DateOfService >= initDate && t.DateOfService <= finalDate))
                                 .ToList()
                                 .Count();

                return Json(count);
            }
            else
            {
                int count = _context.TCMNote
                                 .Where(t => (t.CaseManager.LinkedUser == user_logged.UserName
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

                                                           .Where(t => (t.CaseManager.LinkedUser == user_logged.UserName
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

                                                           .Where(t => (t.CaseManager.LinkedUser == user_logged.UserName
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

        [Authorize(Roles = "Manager")]
        public IActionResult BillingForWeek(string dateInterval = "", int idCaseManager = 0, int idClient = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            ViewBag.Notes = "0";
            ViewBag.Units = "0";
            ViewBag.Money = "0";
            List<TCMNoteEntity> list = new List<TCMNoteEntity>();
            if (dateInterval != string.Empty)
            {
                string[] date = dateInterval.Split(" - ");
                IQueryable<TCMNoteEntity> query = _context.TCMNote
                                                          .Include(t => t.CaseManager)

                                                          .Include(t => t.TCMClient)

                                                          .ThenInclude(c => c.Client)
                                                          .ThenInclude(cl => cl.Clients_Diagnostics)
                                                          .ThenInclude(cd => cd.Diagnostic)

                                                          .Include(t => t.TCMNoteActivity)

                                                          .Where(t => (t.DateOfService >= Convert.ToDateTime(date[0]) && t.DateOfService <= Convert.ToDateTime(date[1])));

                if (idCaseManager != 0)
                    query = query.Where(t => t.CaseManager.Id == idCaseManager);

                if (idClient != 0)
                    query = query.Where(t => t.TCMClient.Id == idClient);

                list = query.ToList();

                int minutes;
                int totalUnits = 0;
                int value;
                int mod;
                foreach (TCMNoteEntity item in list)
                {
                    minutes = item.TCMNoteActivity.Sum(t => t.Minutes);
                    value = minutes / 15;
                    mod = minutes % 15;
                    totalUnits = (mod > 7) ? totalUnits + value + 1 : totalUnits + value;
                }
                ViewBag.Notes = list.Count().ToString();
                ViewBag.Units = totalUnits.ToString();
                ViewBag.Money = (totalUnits * 12).ToString();
            }

            TCMBillingReportViewModel model = new TCMBillingReportViewModel
            {
                DateIterval = dateInterval,
                IdCaseManager = idCaseManager,
                CaseManagers = _combosHelper.GetComboCaseMannagersByClinicFilter(user_logged.Clinic.Id),
                IdClient = idClient,
                Clients = _combosHelper.GetComboClientsForTCMCaseOpenFilter(user_logged.Clinic.Id),
                TCMNotes = dateInterval != string.Empty ? list : new List<TCMNoteEntity>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public IActionResult BillingForWeek(TCMBillingReportViewModel model)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(BillingForWeek), new { dateInterval = model.DateIterval, idCaseManager = model.IdCaseManager, idClient = model.IdClient});
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
                                                           .Include(t => t.CaseManager)
                                                           .Where(t => (t.CaseManager.LinkedUser == user_logged.UserName
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
                    valor = notes.ElementAt(0).CaseManager.Money / 60;
                    money = totalMinutes * valor;
                    return Json(decimal.Round(money, 2));
                }
                else
                {
                    return Json(money);
                }

            }
            else
            {
                List<TCMNoteEntity> notes = _context.TCMNote

                                                           .Include(t => t.TCMNoteActivity)
                                                           .Include(t => t.CaseManager)
                                                           .Where(t => (t.CaseManager.LinkedUser == user_logged.UserName
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
                    valor = notes.ElementAt(0).CaseManager.Money / 60;
                    money = totalMinutes * valor;
                    return Json(decimal.Round(money, 2));
                }
                else
                {
                    return Json(money);
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
                                                           .Include(t => t.CaseManager)
                                                           .Where(t => (t.CaseManager.LinkedUser == user_logged.UserName
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
                    money = (totalMinutes / 60 * notes.ElementAt(0).CaseManager.Money);
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
                                                           .Include(t => t.CaseManager)
                                                           .Where(t => (t.CaseManager.LinkedUser == user_logged.UserName
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
                    money = (totalMinutes / 60 * notes.ElementAt(0).CaseManager.Money);
                }
                else
                {
                    money = 0;
                }
                return Json(totalMinutes);
            }
        }
    }
}