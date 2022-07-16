using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "CaseManager")]
    public class TCMBillingController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;

        public TCMBillingController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
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
            
            AddProgressNoteViewModel model = new AddProgressNoteViewModel
            {
                Date = (date != null) ? Convert.ToDateTime(date) : new DateTime(),
                IdClient = 0,
                Clients = _combosHelper.GetComboClientsByClinic(user_logged.Clinic.Id)
            };

            return View(model);
        }
    }
}