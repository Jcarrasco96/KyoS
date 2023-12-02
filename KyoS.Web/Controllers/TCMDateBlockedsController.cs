using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
    public class TCMDateBlockedsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;

        public TCMDateBlockedsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
        }

        public async Task<IActionResult> Index()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                return View(await _context.TCMDateBlocked

                                      .Include(s => s.Clinic)

                                      .OrderBy(s => s.Clinic.Name).ToListAsync());
            }
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public IActionResult Create()
        {
            TCMDateBlockedViewModel dateBlocked = new TCMDateBlockedViewModel()
            {
                IdClinic = 0,
                Clinics = _combosHelper.GetComboClinics(),
                DateBlocked = DateTime.Today
            };
            return View(dateBlocked);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TCMDateBlockedViewModel model)
        {
            if (ModelState.IsValid)
            {
                TCMDateBlockedEntity dateBlocked = await _context.TCMDateBlocked
                                                                 .FirstOrDefaultAsync(s => s.DateBlocked.Day == model.DateBlocked.Day);
                if (dateBlocked == null)
                {
                    UserEntity user_logged = _context.Users
                                                     .Include(u => u.Clinic)
                                                     .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    dateBlocked = await _converterHelper.ToTCMDateBlockedEntity(model, true, user_logged.Id);
                    _context.Add(dateBlocked);
                    await _context.SaveChangesAsync();
                }

                List<TCMDateBlockedEntity> dateBlockedList = await _context.TCMDateBlocked
                                                                           .Include(s => s.Clinic)
                                                                           .ToListAsync();

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_DateBlockedList", dateBlockedList) });
            }

            TCMDateBlockedViewModel salida = new TCMDateBlockedViewModel()
            {
                IdClinic = model.IdClinic,
                Clinics = _combosHelper.GetComboClinics()
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", salida) });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMDateBlockedEntity entity = await _context.TCMDateBlocked
                                                        .Include(c => c.Clinic)
                                                        .FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMDateBlockedViewModel model = _converterHelper.ToTCMDateBlockedViewModel(entity);

            ClinicEntity clinic = await _context.Clinics
                                                .FirstOrDefaultAsync(c => c.Id == model.IdClinic);

            List<SelectListItem> list = new List<SelectListItem>();
            list.Insert(0, new SelectListItem
            {
                Text = clinic.Name,
                Value = $"{clinic.Id}"
            });
            model.Clinics = list;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TCMDateBlockedViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                TCMDateBlockedEntity entity = await _converterHelper.ToTCMDateBlockedEntity(model, false, user_logged.Id);
                _context.Update(entity);
                await _context.SaveChangesAsync();

                List<TCMDateBlockedEntity> tcmDateBlokedList = await _context.TCMDateBlocked
                                                                             .Include(s => s.Clinic)
                                                                             .ToListAsync();

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_DateBlockedList", tcmDateBlokedList) });
            }

            TCMDateBlockedViewModel salida = new TCMDateBlockedViewModel()
            {
                IdClinic = model.IdClinic,
                Clinics = _combosHelper.GetComboClinics()
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", salida) });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMDateBlockedEntity tcmDateBlockedEntity = await _context.TCMDateBlocked.FirstOrDefaultAsync(t => t.Id == id);
            if (tcmDateBlockedEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMDateBlocked.Remove(tcmDateBlockedEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
