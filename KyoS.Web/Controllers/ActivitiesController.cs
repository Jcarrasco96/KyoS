using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin, Facilitator")]
    public class ActivitiesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        public ActivitiesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
                return View(await _context.Activities.Include(a => a.Theme).ThenInclude(t => t.Clinic).OrderBy(a => a.Theme.Name).ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                    return View(await _context.Activities.Include(a => a.Theme).ThenInclude(t => t.Clinic).OrderBy(a => a.Theme.Name).ToListAsync());

                return View(await _context.Activities.Include(a => a.Theme).ThenInclude(t => t.Clinic)
                                                         .Where(a => a.Theme.Clinic.Id == user_logged.Clinic.Id).OrderBy(a => a.Theme.Name).ToListAsync());                
            }
        }

        public IActionResult Create(int id = 0, int idActivity = 0)
        {
            if (id == 1)
            {
                ViewBag.Creado = "Y";
                ViewBag.IdCreado = idActivity.ToString();
            }
            else
            {
                if (id == 2)
                {
                    ViewBag.Creado = "E";
                }
                else
                {
                    ViewBag.Creado = "N";
                }
            }

            ActivityViewModel model;

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {                   
                    model = new ActivityViewModel
                    {
                        Themes = _combosHelper.GetComboThemesByClinic(user_logged.Clinic.Id)                        
                    };
                    return View(model);
                }
            }

            model = new ActivityViewModel
            {
                Themes = _combosHelper.GetComboThemes()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivityViewModel activityViewModel)
        {
            if (ModelState.IsValid)
            {
                ThemeEntity themeEntity = await _context.Themes.FirstOrDefaultAsync(t => t.Id == activityViewModel.IdTheme);
                ActivityEntity activity = await _context.Activities.FirstOrDefaultAsync((t => (t.Name == activityViewModel.Name && t.Theme.Name == themeEntity.Name)));
                if (activity == null)
                {
                    ActivityEntity activityEntity = await _converterHelper.ToActivityEntity(activityViewModel, true);

                    _context.Add(activityEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        ActivityEntity activityEntityCreated = await _context.Activities.OrderBy(a => a.Id).LastAsync();
                        return RedirectToAction("Create", new { id = 1, idActivity = activityEntityCreated.Id });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the activity: {activityEntity.Theme.Name} - {activityEntity.Name}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Create", new { id = 2 });
                }
            }
            return View(activityViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ActivityEntity activityEntity = await _context.Activities.FirstOrDefaultAsync(t => t.Id == id);
            if (activityEntity == null)
            {
                return NotFound();
            }

            _context.Activities.Remove(activityEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ActivityEntity activityEntity = await _context.Activities.Include(a => a.Theme).FirstOrDefaultAsync(a => a.Id == id);
            if (activityEntity == null)
            {
                return NotFound();
            }

            ActivityViewModel activityViewModel = _converterHelper.ToActivityViewModel(activityEntity);

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    activityViewModel.Themes = _combosHelper.GetComboThemesByClinic(user_logged.Clinic.Id);
                }
            }

            return View(activityViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActivityViewModel activityViewModel)
        {
            if (id != activityViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                ActivityEntity activityEntity = await _converterHelper.ToActivityEntity(activityViewModel, false);
                _context.Update(activityEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the activity: {activityEntity.Theme.Name} - {activityEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(activityViewModel);
        }
    }
}