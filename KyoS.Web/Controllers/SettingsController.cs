using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;        

        public SettingsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;               
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Settings        
                
                                      .Include(s => s.Clinic)

                                      .OrderBy(s => s.Clinic.Name).ToListAsync());
        }

        public IActionResult Create()
        {
            SettingViewModel entity = new SettingViewModel()
            {
                IdClinic = 0,
                Clinics = _combosHelper.GetComboClinics()
            };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SettingViewModel model)
        {
            if (ModelState.IsValid)
            {
                SettingEntity setting = await _context.Settings
                                                      .FirstOrDefaultAsync(s => s.Clinic.Id == model.IdClinic);
                if (setting == null)
                {
                    UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    setting = await _converterHelper.ToSettingEntity(model, true, user_logged.Id);
                    _context.Add(setting);
                    await _context.SaveChangesAsync();             
                }

                List<SettingEntity> settingList = await _context.Settings
                                                                .Include(s => s.Clinic)
                                                                .ToListAsync();

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_SettingList", settingList) });
            }

            SettingViewModel entity = new SettingViewModel()
            {
                IdClinic = model.IdClinic,
                Clinics = _combosHelper.GetComboClinics()
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", entity) });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SettingEntity entity = await _context.Settings
                                                 .Include(c => c.Clinic)
                                                 .FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SettingViewModel model = _converterHelper.ToSettingViewModel(entity);

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
        public async Task<IActionResult> Edit(int id, SettingViewModel model)
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

                SettingEntity setting = await _converterHelper.ToSettingEntity(model, false, user_logged.Id);
                _context.Update(setting);
                await _context.SaveChangesAsync();

                List<SettingEntity> settingList = await _context.Settings
                                                                .Include(s => s.Clinic)
                                                                .ToListAsync();

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_SettingList", settingList) });
            }

            SettingViewModel entity = new SettingViewModel()
            {
                IdClinic = model.IdClinic,
                Clinics = _combosHelper.GetComboClinics()
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", entity) });
        }
    }
}