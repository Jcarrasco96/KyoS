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
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IDateHelper _dateHelper;

        public TCMBillingController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IDateHelper dateHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _dateHelper = dateHelper;
        }

        public IActionResult Index()
        {
            return View();
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
                    Clients = _combosHelper.GetComboTCMClientsByCasemanager(user_logged.UserName)
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

        public IActionResult Events(string start, string end)
        {
            DateTime initDate = Convert.ToDateTime(start);
            DateTime finalDate = Convert.ToDateTime(end);            

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            var events = _context.TCMNoteActivity
                                 .Where(t => (t.TCMNote.CaseManager.LinkedUser == user_logged.UserName
                                           && t.StartTime >= initDate && t.StartTime <= finalDate))
                                 .Select(t => new
                                 {
                                           //id = t.TCMNote.Id,
                                           title = t.DescriptionOfService,                                           
                                           start = t.StartTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                           end = t.EndTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                                           url = Url.Action("Edit", "TCMNotes", new {id = t.TCMNote.Id , origin = 2 }),
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

            //return Json(events);
            return new JsonResult(events);
        }
    }
}