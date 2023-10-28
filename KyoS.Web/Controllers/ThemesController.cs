using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Manager, Supervisor, Facilitator")]
    public class ThemesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;

        public ThemesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
        }
        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (user_logged.Clinic.Schema == SchemaType.Schema3)
            {
                return RedirectToAction(nameof(Index3), "Themes");
            }
                     
            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
            if (clinic != null)
                return View(await _context.Themes
                                          .Include(t => t.Clinic)
                                          .Where(t => (t.Clinic.Id == clinic.Id && t.Day != null))
                                          .OrderBy(f => f.Day)
                                          .ToListAsync());
            else
                return View(await _context.Themes
                                          .Include(t => t.Clinic)
                                          .OrderBy(t => t.Day)
                                          .ToListAsync());            
        }

        public async Task<IActionResult> Index3(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
            if (clinic != null)
                return View(await _context.Themes
                                          .Include(t => t.Clinic)
                                          .Where(t => (t.Clinic.Id == clinic.Id && t.Day == null))
                                          .OrderBy(f => f.Day).ToListAsync());
            else
                return View(await _context.Themes
                                          .Include(t => t.Clinic)
                                          .Where(t => t.Day == null)
                                          .OrderBy(t => t.Day).ToListAsync());
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

            ThemeViewModel model = new ThemeViewModel();

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });
                    model = new ThemeViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        Days = _combosHelper.GetComboDays()
                    };
                    return View(model);
                }
            }

            model = new ThemeViewModel
            {
                Days = _combosHelper.GetComboDays(),
                Clinics = _combosHelper.GetComboClinics(),
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
                ThemeEntity theme = await _context.Themes.FirstOrDefaultAsync((t => (t.Name == themeViewModel.Name 
                                                                                  && t.Day == day 
                                                                                  && t.Clinic.Id == themeViewModel.IdClinic)));
                if (theme == null)
                {
                    ThemeEntity themeEntity = await _converterHelper.ToThemeEntity(themeViewModel, true);

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
                            ModelState.AddModelError(string.Empty, $"Already exists the theme: {themeEntity.Day.ToString()} - {themeEntity.Name}");
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
            
            
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            List<SelectListItem> list = new List<SelectListItem>();
            list.Insert(0, new SelectListItem
            {
                Text = user_logged.Clinic.Name,
                Value = $"{user_logged.Clinic.Id}"
            });
            themeViewModel.Days = _combosHelper.GetComboDays();
            themeViewModel.Clinics = list;
            themeViewModel.IdClinic = user_logged.Clinic.Id;

            return View(themeViewModel);
        }

        public IActionResult Create3(int id = 0)
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

            Theme3ViewModel model = new Theme3ViewModel();

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    ClinicEntity clinic = _context.Clinics
                                                  .FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });
                    model = new Theme3ViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        //Days = _combosHelper.GetComboDays()
                        IdService = 1,
                        Services = _combosHelper.GetComboThemeType(),
                        Themes = _context.Themes
                                         .Include(n => n.Activities) 
                                         .Where(n => n.Clinic.Id == clinic.Id).ToList()
                    };
                    return View(model);
                }
            }

            model = new Theme3ViewModel
            {
                Clinics = _combosHelper.GetComboClinics()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create3(Theme3ViewModel themeViewModel)
        {
            if (ModelState.IsValid)
            {
                ThemeEntity theme = await _context.Themes.FirstOrDefaultAsync((t => (t.Name == themeViewModel.Name 
                                                                                  && t.Day == null 
                                                                                  && t.Clinic.Id == themeViewModel.IdClinic
                                                                                  && t.Service == ThemeUtils.GetThemeByIndex(themeViewModel.IdService))));
                if (theme == null)
                {
                    ThemeEntity themeEntity = await _converterHelper.ToTheme3Entity(themeViewModel, true);

                    _context.Add(themeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create3", new { id = 1 });
                    }
                    catch (Exception ex)
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
                else
                {
                    return RedirectToAction("Create3", new { id = 2 });
                }
            }
            return View(themeViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ThemeEntity themeEntity = await _context.Themes.FirstOrDefaultAsync(t => t.Id == id);
            if (themeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Themes.Remove(themeEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ThemeEntity themeEntity = await _context.Themes.Include(t => t.Clinic).FirstOrDefaultAsync(t => t.Id == id);
            if (themeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ThemeViewModel themeViewModel = _converterHelper.ToThemeViewModel(themeEntity);

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    themeViewModel.Clinics = list;
                }
            }

            return View(themeViewModel);
        }       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ThemeViewModel themeViewModel)
        {
            if (id != themeViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                ThemeEntity themeEntity = await _converterHelper.ToThemeEntity(themeViewModel, false);
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

        public async Task<IActionResult> Edit3(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ThemeEntity themeEntity = await _context.Themes.Include(t => t.Clinic).FirstOrDefaultAsync(t => t.Id == id);
            if (themeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Theme3ViewModel themeViewModel = _converterHelper.ToTheme3ViewModel(themeEntity);

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    themeViewModel.Clinics = list;
                }
            }

            return View(themeViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit3(int id, Theme3ViewModel themeViewModel)
        {
            if (id != themeViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                ThemeEntity themeEntity = await _converterHelper.ToTheme3Entity(themeViewModel, false);
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

        public async Task<IActionResult> Edit3Modal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ThemeEntity themeEntity = await _context.Themes
                                                    .Include(t => t.Clinic)
                                                    .FirstOrDefaultAsync(t => t.Id == id);
            if (themeEntity == null)
            {
                
            }
            else 
            {
                Theme3ViewModel themeViewModel = _converterHelper.ToTheme3ViewModel(themeEntity);

                if (!User.IsInRole("Admin"))
                {
                    UserEntity user_logged = _context.Users
                                                     .Include(u => u.Clinic)
                                                     .FirstOrDefault(u => u.UserName == User.Identity.Name);
                    if (user_logged.Clinic != null)
                    {
                        List<SelectListItem> list = new List<SelectListItem>();
                        list.Insert(0, new SelectListItem
                        {
                            Text = user_logged.Clinic.Name,
                            Value = $"{user_logged.Clinic.Id}"
                        });
                        themeViewModel.Clinics = list;
                    }
                }
                return View(themeViewModel);
            }

            return RedirectToAction("Home/Error404");

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit3Modal(Theme3ViewModel themeViewModel)
        {
            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                ThemeEntity themeEntity = await _converterHelper.ToTheme3Entity(themeViewModel, false);
                _context.Update(themeEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<ThemeEntity> list = await _context.Themes
                                                           .Include(n => n.Activities)
                                                           .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                                           .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTheme3", list) });
                    
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
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit3Modal", themeViewModel) });
            
        }

        public IActionResult CreateActivity3Modal(int id = 0, int idActivity = 0, int idTheme = 0)
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
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    model = new ActivityViewModel
                    {
                       IdTheme = idTheme
                    };
                    ViewData["theme"] = _context.Themes.FirstOrDefault(n => n.Id == idTheme).Name;
                    return View(model);
                }
            }

            model = new ActivityViewModel
            {
                IdTheme = idTheme
            };
            return View(model);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivity3Modal(ActivityViewModel activityViewModel)
        {
            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                ThemeEntity themeEntity = await _context.Themes.FirstOrDefaultAsync(t => t.Id == activityViewModel.IdTheme);
                ActivityEntity activity = await _context.Activities.FirstOrDefaultAsync((t => (t.Name == activityViewModel.Name && t.Theme.Name == themeEntity.Name)));
                if (activity == null)
                {
                    FacilitatorEntity facilitator_logged = await _context.Facilitators
                                                                         .Include(u => u.Clinic)
                                                                         .FirstOrDefaultAsync(u => u.LinkedUser == User.Identity.Name);
                    ActivityEntity activityEntity = await _converterHelper.ToActivityEntity(activityViewModel, true);
                    activityEntity.Facilitator = facilitator_logged;
                    activityEntity.DateCreated = DateTime.Now;
                    _context.Add(activityEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        ActivityEntity activityEntityCreated = await _context.Activities.OrderBy(a => a.Id).LastAsync();

                        List<ThemeEntity> list = await _context.Themes
                                                           .Include(n => n.Activities)
                                                           .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                                           .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTheme3", list) });

                        //return RedirectToAction("Create3", new { id = 1, idActivity = activityEntityCreated.Id });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the facilitator's intervention: {activityEntity.Theme.Name} - {activityEntity.Name}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    return RedirectToAction("CreateActivity3Modal", new { id = 2 });
                }
            }
            return View(activityViewModel);
        }

        public IActionResult DeleteActivity(int id = 0)
        {
            if (id > 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DeleteViewModel model = new DeleteViewModel
                {
                    Id_Element = id,
                    Desciption = "Do you want to delete this record?"

                };
                return View(model);
            }
            else
            {
                //Edit
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteActivity(DeleteViewModel activityModel)
        {
            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);
                ActivityEntity activity = await _context.Activities
                                                        .FirstAsync(n => n.Id == activityModel.Id_Element);
                try
                {
                    _context.Activities.Remove(activity);
                    await _context.SaveChangesAsync();

                    List<ThemeEntity> list = await _context.Themes
                                                           .Include(n => n.Activities)
                                                           .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                                           .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTheme3", list) });
                }
                catch (Exception)
                {
                    List<ThemeEntity> list = await _context.Themes
                                                            .Include(n => n.Activities)
                                                            .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                                            .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTheme3", list) });

                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create3") });
        }

        public async Task<IActionResult> EditActivity3Modal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ActivityEntity activityEntity = await _context.Activities.Include(a => a.Theme).FirstOrDefaultAsync(a => a.Id == id);
            if (activityEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ActivityViewModel activityViewModel = _converterHelper.ToActivityViewModel(activityEntity);

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    if (activityEntity.Theme.Service == ThemeType.PSR)
                    {
                        activityViewModel.Themes = _combosHelper.GetComboThemesByClinic3(user_logged.Clinic.Id, ThemeType.PSR);
                    }
                    if (activityEntity.Theme.Service == ThemeType.Group)
                    {
                        activityViewModel.Themes = _combosHelper.GetComboThemesByClinic3(user_logged.Clinic.Id, ThemeType.Group);
                    }
                }
            }
           
            return View(activityViewModel);
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditActivity3Modal(ActivityViewModel activityViewModel)
        {
          
            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

                ActivityEntity activityEntity = await _converterHelper.ToActivityEntity(activityViewModel, false);
                _context.Update(activityEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<ThemeEntity> list = await _context.Themes
                                                           .Include(n => n.Activities)
                                                           .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                                           .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTheme3", list) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the facilitator's intervention: {activityEntity.Theme.Name} - {activityEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(activityViewModel);
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public async Task<IActionResult> ReturnTo(int? id)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ActivityEntity activity = await _context.Activities.FirstOrDefaultAsync(s => s.Id == id);
            if (activity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                activity.Status = ActivityStatus.Pending;
                _context.Activities.Update(activity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Create3", "Themes");
            }

            return RedirectToAction("Create3", "Themes");
        }
    }
}