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
    [Authorize(Roles = "CaseManager")]
    public class TCMBillingController : Controller
    {
        private readonly DataContext _context;        
        private readonly ICombosHelper _combosHelper;        

        public TCMBillingController(DataContext context, ICombosHelper combosHelper)
        {
            _context = context;
            _combosHelper = combosHelper;            
        }

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
                                      title = t.DescriptionOfService,
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
                                         title = t.DescriptionOfService,
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

        public IActionResult BillingForWeek()
        {
            return View();
        }

    }
}