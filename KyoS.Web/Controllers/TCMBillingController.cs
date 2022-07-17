using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

        public TCMBillingController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
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
                    return RedirectToAction("Create", "TCMNotes", new { dateTime = model.Date, IdTCMClient = model.IdClient });                          
                }
            }

            return RedirectToAction("Index");
        }            
    }
}