using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ThemesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        public ThemesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Themes.OrderBy(t => t.Day).ToListAsync());
        }

        public IActionResult Create(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.Creado = "Y";
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

            ThemeViewModel model = new ThemeViewModel
            {
                Days = _combosHelper.GetComboDays()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThemeViewModel themeViewModel)
        {
            if (ModelState.IsValid)
            {
                DayOfWeekType day = (themeViewModel.DayId == 1) ? DayOfWeekType.Monday : (themeViewModel.DayId == 2) ? DayOfWeekType.Tuesday :
                                    (themeViewModel.DayId == 3) ? DayOfWeekType.Wednesday : (themeViewModel.DayId == 4) ? DayOfWeekType.Thursday :
                                    (themeViewModel.DayId == 5) ? DayOfWeekType.Friday : DayOfWeekType.Monday;
                ThemeEntity theme = await _context.Themes.FirstOrDefaultAsync((t => (t.Name == themeViewModel.Name && t.Day == day)));
                if (theme == null)
                {
                    ThemeEntity themeEntity = _converterHelper.ToThemeEntity(themeViewModel, true);

                    _context.Add(themeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the theme: ${themeEntity.Day.ToString()} - {themeEntity.Name}");
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
            return View(themeViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ThemeEntity themeEntity = await _context.Themes.FirstOrDefaultAsync(t => t.Id == id);
            if (themeEntity == null)
            {
                return NotFound();
            }

            _context.Themes.Remove(themeEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ThemeEntity themeEntity = await _context.Themes.FindAsync(id);
            if (themeEntity == null)
            {
                return NotFound();
            }

            ThemeEntity themeViewModel = _converterHelper.ToThemeViewModel(themeEntity);
            return View(themeViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ThemeViewModel themeViewModel)
        {
            if (id != themeViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                ThemeEntity themeEntity = _converterHelper.ToThemeEntity(themeViewModel, false);
                _context.Update(themeEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the theme: {themeEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(themeViewModel);
        }
    }
}