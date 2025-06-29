﻿using KyoS.Common.Enums;
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
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace KyoS.Web.Controllers
{
    public class ActivitiesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly ITranslateHelper _translateHelper;
        private readonly IRenderHelper _renderHelper;
        public ActivitiesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, ITranslateHelper translateHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _translateHelper = translateHelper;
            _renderHelper = renderHelper;
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> Index()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (user_logged.Clinic.Schema == SchemaType.Schema3)
            {
                return RedirectToAction(nameof(Index3), "Activities");
            }

            if (user_logged.Clinic == null)
            {
                return View(await _context.Activities
                                          .Include(a => a.Theme)
                                          .ThenInclude(t => t.Clinic)
                                          .OrderBy(a => a.Theme.Name).ToListAsync());
            }
            return View(await _context.Activities
                                        .Include(a => a.Theme)
                                        .ThenInclude(t => t.Clinic)
                                        .Where(a => a.Theme.Clinic.Id == user_logged.Clinic.Id)
                                        .OrderBy(a => a.Theme.Name).ToListAsync());

        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> Index3()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (!user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (user_logged.Clinic == null)
                return View(await _context.Activities
                                            .Include(a => a.Theme)
                                            .ThenInclude(t => t.Clinic)
                                            .Where(a => a.Theme.Day == null)
                                            .OrderBy(a => a.Theme.Name).ToListAsync());

            return View(await _context.Activities
                                        .Include(a => a.Theme)
                                        .ThenInclude(t => t.Clinic)
                                        .Where(a => (a.Theme.Clinic.Id == user_logged.Clinic.Id && a.Theme.Day == null))
                                        .OrderBy(a => a.Theme.Name).ToListAsync());

        }

        [Authorize(Roles = "Facilitator")]
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
                        Themes = _combosHelper.GetComboThemesByClinic(user_logged.Clinic.Id, ThemeType.PSR)
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

        [Authorize(Roles = "Facilitator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivityViewModel activityViewModel)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (ModelState.IsValid)
            {
                ThemeEntity themeEntity = await _context.Themes.FirstOrDefaultAsync(t => t.Id == activityViewModel.IdTheme);
                ActivityEntity activity = await _context.Activities.FirstOrDefaultAsync((t => (t.Name == activityViewModel.Name && t.Theme.Name == themeEntity.Name)));
                if (activity == null)
                {
                    FacilitatorEntity facilitator_logged = await _context.Facilitators.Include(u => u.Clinic)
                                                                                      .FirstOrDefaultAsync(u => u.LinkedUser == User.Identity.Name);
                    ActivityEntity activityEntity = await _converterHelper.ToActivityEntity(activityViewModel, true, user_logged.UserName);
                    activityEntity.Facilitator = facilitator_logged;
                    activityEntity.DateCreated = DateTime.Now;
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

        public IActionResult Create3(int id = 0, int idActivity = 0)
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
                        Themes = _combosHelper.GetComboThemesByClinic3(user_logged.Clinic.Id, ThemeType.PSR)
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

        [Authorize(Roles = "Facilitator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create3(ActivityViewModel activityViewModel)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (ModelState.IsValid)
            {
                ThemeEntity themeEntity = await _context.Themes.FirstOrDefaultAsync(t => t.Id == activityViewModel.IdTheme);
                ActivityEntity activity = await _context.Activities.FirstOrDefaultAsync((t => (t.Name == activityViewModel.Name && t.Theme.Name == themeEntity.Name)));
                if (activity == null)
                {
                    FacilitatorEntity facilitator_logged = await _context.Facilitators
                                                                         .Include(u => u.Clinic)
                                                                         .FirstOrDefaultAsync(u => u.LinkedUser == User.Identity.Name);
                    ActivityEntity activityEntity = await _converterHelper.ToActivityEntity(activityViewModel, true, user_logged.UserName);
                    activityEntity.Facilitator = facilitator_logged;
                    activityEntity.DateCreated = DateTime.Now;
                    _context.Add(activityEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        ActivityEntity activityEntityCreated = await _context.Activities.OrderBy(a => a.Id).LastAsync();
                        return RedirectToAction("Create3", new { id = 1, idActivity = activityEntityCreated.Id });
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
                    return RedirectToAction("Create3", new { id = 2 });
                }
            }
            return View(activityViewModel);
        }

        [Authorize(Roles = "Admin, Facilitator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ActivityEntity activityEntity = await _context.Activities.FirstOrDefaultAsync(t => t.Id == id);
            if (activityEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            _context.Activities.Remove(activityEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> Edit(int? id, int origin = 0)
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
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    activityViewModel.Themes = _combosHelper.GetComboThemesByClinic(user_logged.Clinic.Id, ThemeType.PSR);
                }
            }
            activityViewModel.Origin = origin;
            return View(activityViewModel);
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActivityViewModel activityViewModel)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (id != activityViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                ActivityEntity activityEntity = await _converterHelper.ToActivityEntity(activityViewModel, false, user_logged.UserName);
                _context.Update(activityEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    if (activityViewModel.Origin == 0)
                        return RedirectToAction(nameof(Index));
                    if (activityViewModel.Origin == 1)
                        return RedirectToAction(nameof(ActivitiesSupervision));
                    if (activityViewModel.Origin == 2)
                        return RedirectToAction(nameof(ActivitiesSupervision), new { pending = 1 });
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

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> Edit3(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ActivityEntity activityEntity = await _context.Activities
                                                          .Include(a => a.Theme)
                                                          .FirstOrDefaultAsync(a => a.Id == id);
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
            activityViewModel.Origin = origin;
            return View(activityViewModel);
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit3(int id, ActivityViewModel activityViewModel)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (id != activityViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                ActivityEntity activityEntity = await _converterHelper.ToActivityEntity(activityViewModel, false, user_logged.UserName);
                _context.Update(activityEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    if (activityViewModel.Origin == 0)
                        return RedirectToAction(nameof(Index3));
                    if (activityViewModel.Origin == 1)
                        return RedirectToAction(nameof(FISupervision));
                    if (activityViewModel.Origin == 2)
                        return RedirectToAction(nameof(FISupervision), new { pending = 1 });
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

        [Authorize(Roles = "Facilitator")]
        public async Task<JsonResult> Translate(string text)
        {
            return Json(text = await _translateHelper.TranslateText("es", "en", text));
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> ActivitiesPerWeek(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.NoDelete = "N";
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
                return View(null);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ViewData["delete"] = 0;
            return View(await _context.Weeks

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Activities_Facilitators)
                                      .ThenInclude(waf => waf.Activity)
                                      .ThenInclude(a => a.Theme)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Activities_Facilitators)
                                      .ThenInclude(waf => waf.Facilitator)
                                      .AsSplitQuery()
                                      .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                && w.Days.Where(d => (d.Service == ServiceType.PSR && d.Workdays_Clients.Where(wc => wc.Facilitator.LinkedUser == User.Identity.Name).Count() > 0)).Count() > 0))
                                      .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> ActivitiesPerGroupWeek()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
                return View(null);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View(await _context.Weeks

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Activities_Facilitators)
                                      .ThenInclude(waf => waf.Activity)
                                      .ThenInclude(a => a.Theme)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Activities_Facilitators)
                                      .ThenInclude(waf => waf.Facilitator)

                                      .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                && w.Days.Where(d => (d.Service == ServiceType.Group && d.Workdays_Clients.Where(wc => wc.Facilitator.LinkedUser == User.Identity.Name).Count() > 0)).Count() > 0))
                                      .ToListAsync());
        }

        //Schema 1 and schema 2
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> CreateActivitiesWeek(int id = 0, bool am = false, bool pm = false, string session = "AM")
        {
            List<ThemeEntity> topics;
            List<SelectListItem> list1 = new List<SelectListItem>();
            List<SelectListItem> list2 = new List<SelectListItem>();
            List<SelectListItem> list3 = new List<SelectListItem>();
            List<SelectListItem> list4 = new List<SelectListItem>();
            Workday_Activity_FacilitatorViewModel model;

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            WorkdayEntity workday = await _context.Workdays.Include(wd => wd.Week)
                                                           .ThenInclude(w => w.Clinic)

                                                           .Include(wd => wd.Workdays_Clients)
                                                           .ThenInclude(wc => wc.Note)

                                                           .FirstOrDefaultAsync(w => w.Id == id);

            //el workday ya tiene notas creadas por el facilitator logueado por tanto no es posible su edición
            if (WorkdayReadOnly(workday, session))
            {
                ViewBag.Error = "0";
                return View(null);
            }

            FacilitatorEntity facilitator_logged = await _context.Facilitators
                                                                 .FirstOrDefaultAsync(f => f.LinkedUser == User.Identity.Name);

            if ((workday == null) || (facilitator_logged == null))
            {
                return RedirectToAction("Home/Error404");
            }

            List<Workday_Activity_Facilitator> activities_list = await _context.Workdays_Activities_Facilitators
                                                                               .Include(waf => waf.Activity)
                                                                               .ThenInclude(a => a.Theme)

                                                                               .Where(waf => (waf.Workday.Id == workday.Id
                                                                                           && waf.Facilitator.Id == facilitator_logged.Id))
                                                                               .ToListAsync();

            //No hay creadas actividades del facilitador logueado en la fecha seleccionada
            if (activities_list.Count() == 0)
            {
                if (user_logged.Clinic.Schema == SchemaType.Schema3)
                {
                    return RedirectToAction(nameof(CreateActivitiesWeek3), "Activities", new { id = id, session = session });
                }

                if (user_logged.Clinic.Schema == SchemaType.Schema4)
                {
                    return RedirectToAction(nameof(CreateActivitiesWeek4), "Activities", new { id = id });
                }

                topics = await _context.Themes
                                       .Where(t => t.Clinic.Id == user_logged.Clinic.Id)
                                       .ToListAsync();
                topics = topics.Where(t => t.Day.ToString() == workday.Day).ToList();

                int index = 0;
                foreach (ThemeEntity value in topics)
                {
                    if (index == 0)
                    {
                        list1.Insert(index, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 1)
                    {
                        list2.Insert(0, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 2)
                    {
                        list3.Insert(0, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 3)
                    {
                        list4.Insert(0, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                }

                model = new Workday_Activity_FacilitatorViewModel
                {
                    IdWorkday = id,
                    Date = workday.Date.ToShortDateString(),
                    Day = workday.Date.DayOfWeek.ToString(),
                    AM = true,
                    PM = true,

                    IdTopic1 = (list1.Count != 0) ? topics[0].Id : 0,
                    Topics1 = (list1.Count != 0) ? list1 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    Activities1 = (list1.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[0].Id, facilitator_logged.Id, workday.Date) : null,

                    IdTopic2 = (list2.Count != 0) ? topics[1].Id : 0,
                    Topics2 = (list2.Count != 0) ? list2 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    Activities2 = (list2.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[1].Id, facilitator_logged.Id, workday.Date) : null,

                    IdTopic3 = (list3.Count != 0) ? topics[2].Id : 0,
                    Topics3 = (list3.Count != 0) ? list3 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    Activities3 = (list3.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[2].Id, facilitator_logged.Id, workday.Date) : null,

                    IdTopic4 = (list4.Count != 0) ? topics[3].Id : 0,
                    Topics4 = (list4.Count != 0) ? list4 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    Activities4 = (list4.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[3].Id, facilitator_logged.Id, workday.Date) : null,
                };
            }
            else
            {
                if (activities_list[0].Schema == SchemaType.Schema3)
                {
                    return RedirectToAction(nameof(CreateActivitiesWeek3), "Activities", new { id = id, am = am, pm = pm });
                }

                if (activities_list[0].Schema == SchemaType.Schema4)
                {
                    return RedirectToAction(nameof(CreateActivitiesWeek4), "Activities", new { id = id });
                }

                topics = await _context.Themes.Where(t => t.Clinic.Id == user_logged.Clinic.Id)
                                              .ToListAsync();
                topics = topics.Where(t => t.Day.ToString() == workday.Day).ToList();

                int index = 0;
                foreach (ThemeEntity value in topics)
                {
                    if (index == 0)
                    {
                        list1.Insert(index, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 1)
                    {
                        list2.Insert(0, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 2)
                    {
                        list3.Insert(0, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 3)
                    {
                        list4.Insert(0, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                }

                model = new Workday_Activity_FacilitatorViewModel
                {
                    IdWorkday = id,
                    Date = workday.Date.ToShortDateString(),
                    Day = workday.Date.DayOfWeek.ToString(),
                    AM = activities_list.ElementAtOrDefault(0).PM,
                    PM = activities_list.ElementAtOrDefault(0).AM,
                    TitleNote = activities_list.ElementAtOrDefault(0).TitleNote,

                    IdTopic1 = (activities_list.Count > 0) ? activities_list[0].Activity.Theme.Id : 0,
                    Topics1 = (list1.Count != 0) ? list1 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    IdActivity1 = (activities_list.Count > 0) ? activities_list[0].Activity.Id : 0,
                    Activities1 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 0) ? activities_list[0].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),

                    IdTopic2 = (activities_list.Count > 1) ? activities_list[1].Activity.Theme.Id : 0,
                    Topics2 = (list2.Count != 0) ? list2 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    IdActivity2 = (activities_list.Count > 1) ? activities_list[1].Activity.Id : 0,
                    Activities2 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 1) ? activities_list[1].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),

                    IdTopic3 = (activities_list.Count > 2) ? activities_list[2].Activity.Theme.Id : 0,
                    Topics3 = (list3.Count != 0) ? list3 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    IdActivity3 = (activities_list.Count > 2) ? activities_list[2].Activity.Id : 0,
                    Activities3 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 2) ? activities_list[2].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),

                    IdTopic4 = (activities_list.Count > 3) ? activities_list[3].Activity.Theme.Id : 0,
                    Topics4 = (list4.Count != 0) ? list4 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    IdActivity4 = (activities_list.Count > 3) ? activities_list[3].Activity.Id : 0,
                    Activities4 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 3) ? activities_list[3].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),
                };
            }

            return View(model);
        }

        [Authorize(Roles = "Facilitator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivitiesWeek(Workday_Activity_FacilitatorViewModel model)
        {
            if (ModelState.IsValid)
            {
                FacilitatorEntity facilitator_logged = await _context.Facilitators
                                                                     .Include(f => f.Clinic)
                                                                     .FirstOrDefaultAsync(f => f.LinkedUser == User.Identity.Name);

                WorkdayEntity workday = await _context.Workdays
                                                      .FirstOrDefaultAsync(w => w.Id == model.IdWorkday);

                if ((facilitator_logged == null) || (workday == null))
                {
                    return RedirectToAction("Home/Error404");
                }

                List<Workday_Activity_Facilitator> activities_list = await _context.Workdays_Activities_Facilitators
                                                                                   .Where(waf => (waf.Workday.Id == model.IdWorkday
                                                                                               && waf.Facilitator.Id == facilitator_logged.Id))
                                                                        .ToListAsync();
                //elimino las actividades que tiene asociada ese facilitator en ese dia
                _context.RemoveRange(activities_list);

                Workday_Activity_Facilitator activity;
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity1),
                    Schema = facilitator_logged.Clinic.Schema,
                    AM = model.AM,
                    PM = model.PM,
                    TitleNote = model.TitleNote
                };
                _context.Add(activity);
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity2),
                    Schema = facilitator_logged.Clinic.Schema,
                    AM = model.AM,
                    PM = model.PM,
                    TitleNote = model.TitleNote
                };
                _context.Add(activity);
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity3),
                    Schema = facilitator_logged.Clinic.Schema,
                    AM = model.AM,
                    PM = model.PM,
                    TitleNote = model.TitleNote
                };
                _context.Add(activity);
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity4),
                    Schema = facilitator_logged.Clinic.Schema,
                    AM = model.AM,
                    PM = model.PM,
                    TitleNote = model.TitleNote
                };
                _context.Add(activity);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ActivitiesPerWeek));
                }
                catch (Exception ex)
                {

                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the element");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(model);
        }

        //Schema 3
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> CreateActivitiesWeek3(int id = 0, int error = 0, bool am = false, bool pm = false, string session = "AM")
        {
            IEnumerable<SelectListItem> list1;
            IEnumerable<SelectListItem> list2;
            IEnumerable<SelectListItem> list3;
            IEnumerable<SelectListItem> list4;

            Workday_Activity_Facilitator3ViewModel model;

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            WorkdayEntity workday = await _context.Workdays

                                                  .Include(wd => wd.Week)
                                                  .ThenInclude(w => w.Clinic)

                                                  .Include(wd => wd.Workdays_Clients)
                                                  .ThenInclude(wc => wc.NoteP)

                                                  .FirstOrDefaultAsync(w => w.Id == id);

            //el workday ya tiene notas creadas por el facilitator logueado por tanto no es posible su edición
            if (WorkdayReadOnly(workday, session))
            {
                ViewBag.Error = "0";
                return View(null);
            }

            FacilitatorEntity facilitator_logged = await _context.Facilitators
                                                                 .FirstOrDefaultAsync(f => f.LinkedUser == User.Identity.Name);

            if ((workday == null) || (facilitator_logged == null))
            {
                return RedirectToAction("Home/Error404");
            }

            List<Workday_Activity_Facilitator> activities_list = await _context.Workdays_Activities_Facilitators

                                                                               .Include(waf => waf.Activity)
                                                                               .ThenInclude(a => a.Theme)

                                                                               .Where(waf => (waf.Workday.Id == workday.Id
                                                                                           && waf.Facilitator.Id == facilitator_logged.Id))
                                                                               .ToListAsync();

            list1 = _combosHelper.GetComboThemesByClinic3(user_logged.Clinic.Id, ThemeType.PSR);
            list2 = _combosHelper.GetComboThemesByClinic3(user_logged.Clinic.Id, ThemeType.PSR);
            list3 = _combosHelper.GetComboThemesByClinic3(user_logged.Clinic.Id, ThemeType.PSR);
            list4 = _combosHelper.GetComboThemesByClinic3(user_logged.Clinic.Id, ThemeType.PSR);

            ViewData["edit"] = 1;
            //No hay creadas actividades del facilitador logueado en la fecha seleccionada
            if (activities_list.Count() == 0)
            {
                ThemeEntity noTheme = await _context.Themes
                                                    .FirstOrDefaultAsync(t => t.Name == "No theme");

                List<SelectListItem> activitiesSLI = new List<SelectListItem>();
                if (noTheme != null)
                {
                    List<ActivityEntity> activities = await _context.Activities
                                                                .Where(a => (a.Theme.Id == noTheme.Id && a.Status == ActivityStatus.Approved))
                                                                .ToListAsync();
                    if (activities.Count() > 0)
                    {
                        activitiesSLI.Insert(0, new SelectListItem
                        {
                            Text = activities.First().Name.ToString(),
                            Value = $"{activities.First().Id}"
                        });
                    }                    
                }
                
                model = new Workday_Activity_Facilitator3ViewModel
                {
                    IdWorkday = id,
                    Date = workday.Date.ToShortDateString(),
                    Day = workday.Date.DayOfWeek.ToString(),

                    IdTopic1 = 0,
                    Topics1 = list1,
                    Activities1 = null,

                    IdTopic2 = 0,
                    Topics2 = list2,
                    Activities2 = null,

                    IdTopic3 = 0,
                    Topics3 = list3,
                    Activities3 = null,

                    IdTopic4 = (noTheme == null) ? 0 : noTheme.Id,
                    Topics4 = list4,
                    Activities4 = (noTheme == null) ? null : activitiesSLI,
                    IdActivity4 = 0,

                    AM = true,
                    PM = true

                };
            }
            else
            {
                if (activities_list.Count() == 4 && (activities_list.ElementAtOrDefault(0).AM == false || activities_list.ElementAtOrDefault(0).PM == false))
                {
                    model = new Workday_Activity_Facilitator3ViewModel
                    {
                        IdWorkday = id,
                        Date = workday.Date.ToShortDateString(),
                        Day = workday.Date.DayOfWeek.ToString(),

                        IdTopic1 = 0,
                        Topics1 = list1,
                        Activities1 = null,

                        IdTopic2 = 0,
                        Topics2 = list2,
                        Activities2 = null,

                        IdTopic3 = 0,
                        Topics3 = list3,
                        Activities3 = null,

                        IdTopic4 = 0,
                        Topics4 = list4,
                        Activities4 = null,

                        AM = activities_list.ElementAtOrDefault(0).PM,
                        PM = activities_list.ElementAtOrDefault(0).AM,
                        TitleNote = activities_list.ElementAtOrDefault(0).TitleNote
                    };
                    ViewData["edit"] = 0;
                }
                else
                {
                    if (activities_list.Count() == 8)
                    {
                        activities_list = await _context.Workdays_Activities_Facilitators

                                                    .Include(waf => waf.Activity)
                                                    .ThenInclude(a => a.Theme)

                                                    .Where(waf => (waf.Workday.Id == workday.Id
                                                        && waf.Facilitator.Id == facilitator_logged.Id
                                                        && waf.AM == am
                                                        && waf.PM == pm))
                                                    .ToListAsync();
                    }
                    if (activities_list.Count() == 4)
                    {
                        ViewData["edit"] = 0;
                    }
                    
                    model = new Workday_Activity_Facilitator3ViewModel
                    {
                        IdWorkday = id,
                        Date = workday.Date.ToShortDateString(),
                        Day = workday.Date.DayOfWeek.ToString(),

                        IdTopic1 = (activities_list.Count > 0) ? activities_list[0].Activity.Theme.Id : 0,
                        Topics1 = list1,
                        IdActivity1 = (activities_list.Count > 0) ? activities_list[0].Activity.Id : 0,
                        Activities1 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 0) ? activities_list[0].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),
                        activityDailyLiving1 = activities_list[0].activityDailyLiving == null ? false : Convert.ToBoolean(activities_list[0].activityDailyLiving),
                        communityResources1 = activities_list[0].communityResources == null ? false : Convert.ToBoolean(activities_list[0].communityResources),
                        copingSkills1 = activities_list[0].copingSkills == null ? false : Convert.ToBoolean(activities_list[0].copingSkills),
                        diseaseManagement1 = activities_list[0].diseaseManagement == null ? false : Convert.ToBoolean(activities_list[0].diseaseManagement),
                        healthyLiving1 = activities_list[0].healthyLiving == null ? false : Convert.ToBoolean(activities_list[0].healthyLiving),
                        lifeSkills1 = activities_list[0].lifeSkills == null ? false : Convert.ToBoolean(activities_list[0].lifeSkills),
                        relaxationTraining1 = activities_list[0].relaxationTraining == null ? false : Convert.ToBoolean(activities_list[0].relaxationTraining),
                        socialSkills1 = activities_list[0].socialSkills == null ? false : Convert.ToBoolean(activities_list[0].socialSkills),
                        stressManagement1 = activities_list[0].stressManagement == null ? false : Convert.ToBoolean(activities_list[0].stressManagement),

                        IdTopic2 = (activities_list.Count > 1) ? activities_list[1].Activity.Theme.Id : 0,
                        Topics2 = list2,
                        IdActivity2 = (activities_list.Count > 1) ? activities_list[1].Activity.Id : 0,
                        Activities2 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 1) ? activities_list[1].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),
                        activityDailyLiving2 = activities_list[1].activityDailyLiving == null ? false : Convert.ToBoolean(activities_list[1].activityDailyLiving),
                        communityResources2 = activities_list[1].communityResources == null ? false : Convert.ToBoolean(activities_list[1].communityResources),
                        copingSkills2 = activities_list[1].copingSkills == null ? false : Convert.ToBoolean(activities_list[1].copingSkills),
                        diseaseManagement2 = activities_list[1].diseaseManagement == null ? false : Convert.ToBoolean(activities_list[1].diseaseManagement),
                        healthyLiving2 = activities_list[1].healthyLiving == null ? false : Convert.ToBoolean(activities_list[1].healthyLiving),
                        lifeSkills2 = activities_list[1].lifeSkills == null ? false : Convert.ToBoolean(activities_list[1].lifeSkills),
                        relaxationTraining2 = activities_list[1].relaxationTraining == null ? false : Convert.ToBoolean(activities_list[1].relaxationTraining),
                        socialSkills2 = activities_list[1].socialSkills == null ? false : Convert.ToBoolean(activities_list[1].socialSkills),
                        stressManagement2 = activities_list[1].stressManagement == null ? false : Convert.ToBoolean(activities_list[1].stressManagement),

                        IdTopic3 = (activities_list.Count > 2) ? activities_list[2].Activity.Theme.Id : 0,
                        Topics3 = list3,
                        IdActivity3 = (activities_list.Count > 2) ? activities_list[2].Activity.Id : 0,
                        Activities3 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 2) ? activities_list[2].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),
                        activityDailyLiving3 = activities_list[2].activityDailyLiving == null ? false : Convert.ToBoolean(activities_list[2].activityDailyLiving),
                        communityResources3 = activities_list[2].communityResources == null ? false : Convert.ToBoolean(activities_list[2].communityResources),
                        copingSkills3 = activities_list[2].copingSkills == null ? false : Convert.ToBoolean(activities_list[2].copingSkills),
                        diseaseManagement3 = activities_list[2].diseaseManagement == null ? false : Convert.ToBoolean(activities_list[2].diseaseManagement),
                        healthyLiving3 = activities_list[2].healthyLiving == null ? false : Convert.ToBoolean(activities_list[2].healthyLiving),
                        lifeSkills3 = activities_list[2].lifeSkills == null ? false : Convert.ToBoolean(activities_list[2].lifeSkills),
                        relaxationTraining3 = activities_list[2].relaxationTraining == null ? false : Convert.ToBoolean(activities_list[2].relaxationTraining),
                        socialSkills3 = activities_list[2].socialSkills == null ? false : Convert.ToBoolean(activities_list[2].socialSkills),
                        stressManagement3 = activities_list[2].stressManagement == null ? false : Convert.ToBoolean(activities_list[2].stressManagement),

                        IdTopic4 = (activities_list.Count > 3) ? activities_list[3].Activity.Theme.Id : 0,
                        Topics4 = list4,
                        IdActivity4 = (activities_list.Count > 3) ? activities_list[3].Activity.Id : 0,
                        Activities4 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 3) ? activities_list[3].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),
                        activityDailyLiving4 = activities_list[3].activityDailyLiving == null ? false : Convert.ToBoolean(activities_list[3].activityDailyLiving),
                        communityResources4 = activities_list[3].communityResources == null ? false : Convert.ToBoolean(activities_list[3].communityResources),
                        copingSkills4 = activities_list[3].copingSkills == null ? false : Convert.ToBoolean(activities_list[3].copingSkills),
                        diseaseManagement4 = activities_list[3].diseaseManagement == null ? false : Convert.ToBoolean(activities_list[3].diseaseManagement),
                        healthyLiving4 = activities_list[3].healthyLiving == null ? false : Convert.ToBoolean(activities_list[3].healthyLiving),
                        lifeSkills4 = activities_list[3].lifeSkills == null ? false : Convert.ToBoolean(activities_list[3].lifeSkills),
                        relaxationTraining4 = activities_list[3].relaxationTraining == null ? false : Convert.ToBoolean(activities_list[3].relaxationTraining),
                        socialSkills4 = activities_list[3].socialSkills == null ? false : Convert.ToBoolean(activities_list[3].socialSkills),
                        stressManagement4 = activities_list[3].stressManagement == null ? false : Convert.ToBoolean(activities_list[3].stressManagement),

                        AM = activities_list.ElementAtOrDefault(0).AM,
                        PM = activities_list.ElementAtOrDefault(0).PM,
                        TitleNote = activities_list.ElementAtOrDefault(0).TitleNote
                    };
                }                
            }

            return View(model);
        }

        [Authorize(Roles = "Facilitator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivitiesWeek3(Workday_Activity_Facilitator3ViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.AM == false && model.PM == false)
                {
                    
                    return RedirectToAction("CreateActivitiesWeek","Activities", new { id = model.IdWorkday});
                }
                
                FacilitatorEntity facilitator_logged = await _context.Facilitators

                                                                     .Include(f => f.Clinic)

                                                                     .FirstOrDefaultAsync(f => f.LinkedUser == User.Identity.Name);

                WorkdayEntity workday = await _context.Workdays
                                                      .FirstOrDefaultAsync(w => w.Id == model.IdWorkday);

                if ((facilitator_logged == null) || (workday == null))
                {
                    return RedirectToAction("Home/Error404");
                }

                List<Workday_Activity_Facilitator> activities_list = await _context.Workdays_Activities_Facilitators
                                                                                   .Where(waf => (waf.Workday.Id == model.IdWorkday
                                                                                               && waf.Facilitator.Id == facilitator_logged.Id
                                                                                               && waf.AM == model.AM
                                                                                               && waf.PM == model.PM))
                                                                                   .ToListAsync();
                //elimino las actividades que tiene asociada ese facilitator en ese dia
                _context.RemoveRange(activities_list);

                Workday_Activity_Facilitator activity;
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity1),
                    Schema = facilitator_logged.Clinic.Schema,
                    activityDailyLiving = model.activityDailyLiving1,
                    communityResources = model.communityResources1,
                    copingSkills = model.copingSkills1,
                    diseaseManagement = model.diseaseManagement1,
                    healthyLiving = model.healthyLiving1,
                    lifeSkills = model.lifeSkills1,
                    relaxationTraining = model.relaxationTraining1,
                    socialSkills = model.socialSkills1,
                    stressManagement = model.stressManagement1,
                    AM = model.AM,
                    PM = model.PM,
                    TitleNote = model.TitleNote
                };
                _context.Add(activity);
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity2),
                    Schema = facilitator_logged.Clinic.Schema,
                    activityDailyLiving = model.activityDailyLiving2,
                    communityResources = model.communityResources2,
                    copingSkills = model.copingSkills2,
                    diseaseManagement = model.diseaseManagement2,
                    healthyLiving = model.healthyLiving2,
                    lifeSkills = model.lifeSkills2,
                    relaxationTraining = model.relaxationTraining2,
                    socialSkills = model.socialSkills2,
                    stressManagement = model.stressManagement2,
                    AM = model.AM,
                    PM = model.PM,
                    TitleNote = model.TitleNote
                };
                _context.Add(activity);
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity3),
                    Schema = facilitator_logged.Clinic.Schema,
                    activityDailyLiving = model.activityDailyLiving3,
                    communityResources = model.communityResources3,
                    copingSkills = model.copingSkills3,
                    diseaseManagement = model.diseaseManagement3,
                    healthyLiving = model.healthyLiving3,
                    lifeSkills = model.lifeSkills3,
                    relaxationTraining = model.relaxationTraining3,
                    socialSkills = model.socialSkills3,
                    stressManagement = model.stressManagement3,
                    AM = model.AM,
                    PM = model.PM,
                    TitleNote = model.TitleNote
                };
                _context.Add(activity);
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity4),
                    Schema = facilitator_logged.Clinic.Schema,
                    activityDailyLiving = model.activityDailyLiving4,
                    communityResources = model.communityResources4,
                    copingSkills = model.copingSkills4,
                    diseaseManagement = model.diseaseManagement4,
                    healthyLiving = model.healthyLiving4,
                    lifeSkills = model.lifeSkills4,
                    relaxationTraining = model.relaxationTraining4,
                    socialSkills = model.socialSkills4,
                    stressManagement = model.stressManagement4,
                    AM = model.AM,
                    PM = model.PM,
                    TitleNote = model.TitleNote
                };
                _context.Add(activity);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ActivitiesPerWeek));
                }
                catch (Exception ex)
                {

                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the element");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(model);
        }

        //Schema 4
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> CreateActivitiesWeek4(int id = 0)
        {
            List<ThemeEntity> topics;
            List<SelectListItem> list1 = new List<SelectListItem>();
            List<SelectListItem> list2 = new List<SelectListItem>();
            List<SelectListItem> list3 = new List<SelectListItem>();
            
            Workday_Activity_Facilitator4ViewModel model;

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            WorkdayEntity workday = await _context.Workdays.Include(wd => wd.Week)
                                                           .ThenInclude(w => w.Clinic)

                                                           .Include(wd => wd.Workdays_Clients)
                                                           .ThenInclude(wc => wc.Note)

                                                           .FirstOrDefaultAsync(w => w.Id == id);

            //el workday ya tiene notas creadas por el facilitator logueado por tanto no es posible su edición
            if (WorkdayReadOnly(workday))
            {
                ViewBag.Error = "0";
                return View(null);
            }

            FacilitatorEntity facilitator_logged = await _context.Facilitators
                                                                 .FirstOrDefaultAsync(f => f.LinkedUser == User.Identity.Name);

            if ((workday == null) || (facilitator_logged == null))
            {
                return RedirectToAction("Home/Error404");
            }

            List<Workday_Activity_Facilitator> activities_list = await _context.Workdays_Activities_Facilitators
                                                                               .Include(waf => waf.Activity)
                                                                               .ThenInclude(a => a.Theme)
                                                                               .Where(waf => (waf.Workday.Id == workday.Id
                                                                                           && waf.Facilitator.Id == facilitator_logged.Id))
                                                                               .ToListAsync();

            //No hay creadas actividades del facilitador logueado en la fecha seleccionada
            if (activities_list.Count() == 0)
            {
                topics = await _context.Themes
                                       .Where(t => t.Clinic.Id == user_logged.Clinic.Id)
                                       .ToListAsync();
                topics = topics.Where(t => t.Day.ToString() == workday.Day).ToList();

                int index = 0;
                foreach (ThemeEntity value in topics)
                {
                    if (index == 0)
                    {
                        list1.Insert(index, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 1)
                    {
                        list2.Insert(0, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 2)
                    {
                        list3.Insert(0, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 3)
                    {
                        list1.Add(new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        list2.Add(new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        list3.Add(new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                }

                model = new Workday_Activity_Facilitator4ViewModel
                {
                    IdWorkday = id,
                    Date = workday.Date.ToShortDateString(),
                    Day = workday.Date.DayOfWeek.ToString(),

                    IdTopic1 = (list1.Count != 0) ? topics[0].Id : 0,
                    Topics1 = (list1.Count != 0) ? list1 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    Activities1 = (list1.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[0].Id, facilitator_logged.Id, workday.Date) : null,

                    IdTopic2 = (list2.Count != 0) ? topics[1].Id : 0,
                    Topics2 = (list2.Count != 0) ? list2 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    Activities2 = (list2.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[1].Id, facilitator_logged.Id, workday.Date) : null,

                    IdTopic3 = (list3.Count != 0) ? topics[2].Id : 0,
                    Topics3 = (list3.Count != 0) ? list3 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    Activities3 = (list3.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[2].Id, facilitator_logged.Id, workday.Date) : null,                    
                };
            }
            else
            {
                topics = await _context.Themes
                                       .Where(t => t.Clinic.Id == user_logged.Clinic.Id)
                                       .ToListAsync();
                topics = topics.Where(t => t.Day.ToString() == workday.Day).ToList();

                int index = 0;
                foreach (ThemeEntity value in topics)
                {
                    if (index == 0)
                    {
                        list1.Insert(index, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 1)
                    {
                        list2.Insert(0, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 2)
                    {
                        list3.Insert(0, new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                    if (index == 3)
                    {
                        list1.Add(new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        list2.Add(new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        list3.Add(new SelectListItem
                        {
                            Text = value.Name,
                            Value = $"{value.Id}"
                        });
                        index = ++index;
                        continue;
                    }
                }

                model = new Workday_Activity_Facilitator4ViewModel
                {
                    IdWorkday = id,
                    Date = workday.Date.ToShortDateString(),
                    Day = workday.Date.DayOfWeek.ToString(),

                    IdTopic1 = (activities_list.Count > 0) ? activities_list[0].Activity.Theme.Id : 0,
                    Topics1 = (list1.Count != 0) ? list1 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    IdActivity1 = (activities_list.Count > 0) ? activities_list[0].Activity.Id : 0,
                    Activities1 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 0) ? activities_list[0].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),

                    IdTopic2 = (activities_list.Count > 1) ? activities_list[1].Activity.Theme.Id : 0,
                    Topics2 = (list2.Count != 0) ? list2 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    IdActivity2 = (activities_list.Count > 1) ? activities_list[1].Activity.Id : 0,
                    Activities2 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 1) ? activities_list[1].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),

                    IdTopic3 = (activities_list.Count > 2) ? activities_list[2].Activity.Theme.Id : 0,
                    Topics3 = (list3.Count != 0) ? list3 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id, ThemeType.PSR),
                    IdActivity3 = (activities_list.Count > 2) ? activities_list[2].Activity.Id : 0,
                    Activities3 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 2) ? activities_list[2].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),

                    
                };
            }

            return View(model);
        }

        [Authorize(Roles = "Facilitator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivitiesWeek4(Workday_Activity_Facilitator4ViewModel model)
        {
            if (ModelState.IsValid)
            {
                FacilitatorEntity facilitator_logged = await _context.Facilitators
                                                                     .Include(f => f.Clinic)
                                                                     .FirstOrDefaultAsync(f => f.LinkedUser == User.Identity.Name);

                WorkdayEntity workday = await _context.Workdays
                                                      .FirstOrDefaultAsync(w => w.Id == model.IdWorkday);

                if ((facilitator_logged == null) || (workday == null))
                {
                    return RedirectToAction("Home/Error404");
                }

                List<Workday_Activity_Facilitator> activities_list = await _context.Workdays_Activities_Facilitators
                                                                                   .Where(waf => (waf.Workday.Id == model.IdWorkday
                                                                                               && waf.Facilitator.Id == facilitator_logged.Id))
                                                                                   .ToListAsync();
                //elimino las actividades que tiene asociada ese facilitator en ese dia
                _context.RemoveRange(activities_list);

                Workday_Activity_Facilitator activity;
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity1),
                    Schema = facilitator_logged.Clinic.Schema
                };
                _context.Add(activity);
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity2),
                    Schema = facilitator_logged.Clinic.Schema
                };
                _context.Add(activity);
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity3),
                    Schema = facilitator_logged.Clinic.Schema
                };
                _context.Add(activity);
                
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ActivitiesPerWeek));
                }
                catch (Exception ex)
                {

                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the element");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(model);
        }

        //Group Therapy
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> CreateActivitiesGroupWeek(int id = 0)
        {
            List<ThemeEntity> topics;
            Workday_Activity_FacilitatorGroupViewModel model;
            
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            WorkdayEntity workday = await _context.Workdays

                                                  .Include(wd => wd.Week)
                                                  .ThenInclude(w => w.Clinic)

                                                  .Include(wd => wd.Workdays_Clients)
                                                  .ThenInclude(wc => wc.Note)

                                                  .FirstOrDefaultAsync(w => w.Id == id);

            //el workday ya tiene notas creadas por el facilitator logueado por tanto no es posible su edición
            if (GroupWorkdayReadOnly(workday))
            {
                ViewBag.Error = "0";
                return View(null);
            }

            FacilitatorEntity facilitator_logged = await _context.Facilitators
                                                                 .FirstOrDefaultAsync(f => f.LinkedUser == User.Identity.Name);

            if ((workday == null) || (facilitator_logged == null))
            {
                return RedirectToAction("Home/Error404");
            }

            List<Workday_Activity_Facilitator> activities_list = await _context.Workdays_Activities_Facilitators

                                                                               .Include(waf => waf.Activity)
                                                                               .ThenInclude(a => a.Theme)

                                                                               .Where(waf => (waf.Workday.Id == workday.Id
                                                                                           && waf.Facilitator.Id == facilitator_logged.Id))
                                                                               .ToListAsync();

            topics = await _context.Themes
                                   .Where(t => t.Clinic.Id == user_logged.Clinic.Id)
                                   .ToListAsync();

            //No hay creadas actividades del facilitador logueado en la fecha seleccionada
            if (activities_list.Count() == 0)
            {
                //if (user_logged.Clinic.Schema == SchemaType.Schema4)
                //{
                //    return RedirectToAction(nameof(CreateActivitiesWeek4), "Activities", new { id = id });
                //}

                model = new Workday_Activity_FacilitatorGroupViewModel
                {
                    IdWorkday = id,
                    Date = workday.Date.ToShortDateString(),
                    Day = workday.Date.DayOfWeek.ToString(),

                    IdTopic1 = 0,
                    Topics1 = _combosHelper.GetComboThemesByClinic3(workday.Week.Clinic.Id, ThemeType.Group),
                    Activities1 = null,

                    IdTopic2 = 0,
                    Topics2 = _combosHelper.GetComboThemesByClinic3(workday.Week.Clinic.Id, ThemeType.Group),
                    Activities2 = null,                    
                };
            }
            else
            {
                //if (activities_list[0].Schema == SchemaType.Schema4)
                //{
                //    return RedirectToAction(nameof(CreateActivitiesWeek4), "Activities", new { id = id });
                //}               

                model = new Workday_Activity_FacilitatorGroupViewModel
                {
                    IdWorkday = id,
                    Date = workday.Date.ToShortDateString(),
                    Day = workday.Date.DayOfWeek.ToString(),

                    IdTopic1 = (activities_list.Count > 0) ? activities_list[0].Activity.Theme.Id : 0,
                    Topics1 = _combosHelper.GetComboThemesByClinic3(workday.Week.Clinic.Id, ThemeType.Group),
                    IdActivity1 = (activities_list.Count > 0) ? activities_list[0].Activity.Id : 0,
                    Activities1 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 0) ? activities_list[0].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),

                    IdTopic2 = (activities_list.Count > 1) ? activities_list[1].Activity.Theme.Id : 0,
                    Topics2 = _combosHelper.GetComboThemesByClinic3(workday.Week.Clinic.Id, ThemeType.Group),
                    IdActivity2 = (activities_list.Count > 1) ? activities_list[1].Activity.Id : 0,
                    Activities2 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 1) ? activities_list[1].Activity.Theme.Id : 0, facilitator_logged.Id, workday.Date),
                };
            }
            ViewData["Schema"] = user_logged.Clinic.SchemaGroup;
            return View(model);
        }

        [Authorize(Roles = "Facilitator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivitiesGroupWeek(Workday_Activity_FacilitatorGroupViewModel model)
        {
            FacilitatorEntity facilitator_logged = await _context.Facilitators
                                                                    .Include(f => f.Clinic)
                                                                    .FirstOrDefaultAsync(f => f.LinkedUser == User.Identity.Name);
            if (ModelState.IsValid || (facilitator_logged.Clinic.SchemaGroup == SchemaTypeGroup.Schema3 && model.IdTopic1 > 0 && model.IdActivity1 > 0))
            {
               

                WorkdayEntity workday = await _context.Workdays
                                                      .FirstOrDefaultAsync(w => w.Id == model.IdWorkday);

                if ((facilitator_logged == null) || (workday == null))
                {
                    return RedirectToAction("Home/Error404");
                }

                List<Workday_Activity_Facilitator> activities_list = await _context.Workdays_Activities_Facilitators
                                                                                   .Where(waf => (waf.Workday.Id == model.IdWorkday
                                                                                               && waf.Facilitator.Id == facilitator_logged.Id))
                                                                                   .ToListAsync();
                //elimino las actividades que tiene asociada ese facilitator en ese dia
                _context.RemoveRange(activities_list);

                Workday_Activity_Facilitator activity;
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity1),
                    Schema = SchemaType.Schema1
                };
                _context.Add(activity);
                
                if (facilitator_logged.Clinic.SchemaGroup != SchemaTypeGroup.Schema3)
                {
                    activity = new Workday_Activity_Facilitator
                    {
                        Facilitator = facilitator_logged,
                        Workday = workday,
                        Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity2),
                        Schema = SchemaType.Schema1
                    };
                    _context.Add(activity);
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ActivitiesPerGroupWeek));
                }
                catch (Exception ex)
                {

                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the element");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Facilitator")]
        public JsonResult GetActivityList(int idTheme)
        {
            List<ActivityEntity> activities = _context.Activities
                                                      .Where(a => a.Theme.Id == idTheme && a.Status == ActivityStatus.Approved)
                                                      .ToList();

            return Json(new SelectList(activities, "Id", "Name"));
        }

        [Authorize(Roles = "Facilitator")]
        public JsonResult GetActivityList3(int idTheme, string dateOfActivity)
        {
            FacilitatorEntity facilitator_logged = _context.Facilitators
                                                           .FirstOrDefault(f => f.LinkedUser == User.Identity.Name);

            IEnumerable<SelectListItem> list = _combosHelper.GetComboActivitiesByTheme(idTheme, facilitator_logged.Id, Convert.ToDateTime(dateOfActivity));
            
            return Json(new SelectList(list, "Value", "Text"));
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ActivitiesSupervision(int id = 0, int pending = 0)
        {
            if (id == 1)
            {
                ViewBag.Approve = "Y";
            }
                        
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic.Schema == SchemaType.Schema3)
            {
                return RedirectToAction(nameof(FISupervision), "Activities", new { pending = pending });
            }

            if (user_logged.Clinic == null)
                return View(null);

            if (pending == 0)
            { 
                return View(await _context.Activities
                                          .Include(a => a.Theme)
                                          .Include(a => a.Facilitator)
                                          .ThenInclude(t => t.Clinic)
                                          .Where(a => a.Theme.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderBy(a => a.Theme.Name).ToListAsync());
            }
            else
            {
                ViewBag.Pending = "1";
                return View(await _context.Activities
                                          .Include(a => a.Theme)
                                          .Include(a => a.Facilitator)
                                          .ThenInclude(t => t.Clinic)
                                          .Where(a => (a.Theme.Clinic.Id == user_logged.Clinic.Id && a.Status == Common.Enums.ActivityStatus.Pending))
                                          .OrderBy(a => a.Theme.Name).ToListAsync());
            }
            
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> FISupervision(int id = 0, int pending = 0)
        {
            if (id == 1)
            {
                ViewBag.Approve = "Y";
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            
            if (user_logged.Clinic == null)
                return View(null);

            if (pending == 0)
            {
                return View(await _context.Activities
                                          .Include(a => a.Theme)
                                          .Include(a => a.Facilitator)
                                          .ThenInclude(t => t.Clinic)
                                          .Where(a => (a.Theme.Clinic.Id == user_logged.Clinic.Id && a.Theme.Day == null))
                                          .OrderBy(a => a.Theme.Name).ToListAsync());
            }
            else
            {
                ViewBag.Pending = "1";
                return View(await _context.Activities
                                          .Include(a => a.Theme)
                                          .Include(a => a.Facilitator)
                                          .ThenInclude(t => t.Clinic)
                                          .Where(a => (a.Theme.Clinic.Id == user_logged.Clinic.Id && a.Status == Common.Enums.ActivityStatus.Pending && a.Theme.Day == null))
                                          .OrderBy(a => a.Theme.Name).ToListAsync());
            }
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ApproveActivity(int id, int origin = 0)
        {
            ActivityEntity activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == id);
            if (activity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            activity.Status = Common.Enums.ActivityStatus.Approved;
            activity.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(u => u.LinkedUser == User.Identity.Name);
            activity.DateOfApprove = DateTime.Now;

            _context.Update(activity);
            await _context.SaveChangesAsync();

            if (origin == 0)
            {
                return RedirectToAction(nameof(ActivitiesSupervision), new { id = 1 });
            }
            else
            {
                return RedirectToAction(nameof(ActivitiesSupervision), new { id = 1, pending = 1});
            }              
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ApproveFacilitatorIntervention(int id, int origin = 0)
        {
            ActivityEntity activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == id);
            if (activity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            activity.Status = Common.Enums.ActivityStatus.Approved;
            activity.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(u => u.LinkedUser == User.Identity.Name);
            activity.DateOfApprove = DateTime.Now;

            _context.Update(activity);
            await _context.SaveChangesAsync();

            if (origin == 0)
            {
                return RedirectToAction(nameof(FISupervision), new { id = 1 });
            }
            else
            {
                return RedirectToAction(nameof(FISupervision), new { id = 1, pending = 1 });
            }
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> SearchTopic()
        {
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

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> DeleteActivityPerWeek(int? idWorkday)
        {
            UserEntity user_logged = await _context.Users
                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)
                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (idWorkday == null)
            {
                return RedirectToAction("Home/Error404");
            }

            List <Workday_Activity_Facilitator> workdayActivityFacilitator = await _context.Workdays_Activities_Facilitators
                                                                                           
                                                                                           .Where(t => (t.Workday.Id == idWorkday
                                                                                                && t.Facilitator.LinkedUser == user_logged.UserName))
                                                                                           .ToListAsync();

            List<Workday_Client> workdayClient = await _context.Workdays_Clients     
                
                                                               .Where(t => (t.Workday.Id == idWorkday
                                                                    && t.Facilitator.LinkedUser == user_logged.UserName
                                                                    && (t.Note != null
                                                                    || t.NoteP != null
                                                                    || t.IndividualNote != null
                                                                    || t.GroupNote != null)))
                                                               .ToListAsync();

            if (workdayClient.Count() > 0)
            {
                return RedirectToAction("ActivitiesPerWeek", "Activities", new { idError = 1});
            }

            foreach (var item in workdayActivityFacilitator)
            {
                _context.Workdays_Activities_Facilitators.Remove(item);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ActivitiesPerWeek", "Activities");
        }

        #region Utils functions
        public bool WorkdayReadOnly(WorkdayEntity workday, string session = "AM")
        {
            if (workday.Workdays_Clients.Count() > 0)
            {
                FacilitatorEntity facilitator_logged = _context.Facilitators

                                                               .Include(f => f.Clinic)

                                                               .FirstOrDefault(f => f.LinkedUser == User.Identity.Name);

                foreach (Workday_Client item in workday.Workdays_Clients)
                {
                    if (facilitator_logged.Clinic.Schema == SchemaType.Schema3)
                    {
                        if ((item.NoteP != null) && (item.Facilitator == facilitator_logged) && (item.Session == session))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if ((item.Note != null) && (item.Facilitator == facilitator_logged) && (item.Session == session))
                        {
                            return true;
                        }

                    }
                }
                return false;
            }
            else
                return false;
        }

        public bool GroupWorkdayReadOnly(WorkdayEntity workday)
        {
            if (workday.Workdays_Clients.Count > 0)
            {
                FacilitatorEntity facilitator_logged = _context.Facilitators
                                                               .FirstOrDefault(f => f.LinkedUser == User.Identity.Name);
                foreach (Workday_Client item in workday.Workdays_Clients)
                {
                    if ((item.GroupNote != null) && (item.Facilitator == facilitator_logged))
                        return true;
                }
                return false;
            }
            else
                return false;
        }
        #endregion

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> AddLink(int? id, int origin = 0)
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
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    activityViewModel.Themes = _combosHelper.GetComboThemesByClinic(user_logged.Clinic.Id, ThemeType.All);
                }
            }
            activityViewModel.Origin = origin;
            return View(activityViewModel);
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLink(int id, ActivityViewModel activityViewModel)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (id != activityViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                ActivityEntity activityEntity = await _converterHelper.ToActivityEntity(activityViewModel, false, user_logged.UserName);
                activityEntity.Name = activityEntity.Name + activityViewModel.Link;
                _context.Update(activityEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    if (activityViewModel.Origin == 0)
                    {
                        return RedirectToAction(nameof(ActivitiesPerWeek));
                    }
                    if (activityViewModel.Origin == 1)
                    {
                        return RedirectToAction(nameof(ActivitiesPerGroupWeek));
                    }
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
                   /* if (activityEntity.Theme.Service == ThemeType.PSR)
                    {
                        activityViewModel.Themes = _combosHelper.GetComboThemesByClinic3(user_logged.Clinic.Id, ThemeType.PSR);
                    }
                    if (activityEntity.Theme.Service == ThemeType.Group)
                    {
                        activityViewModel.Themes = _combosHelper.GetComboThemesByClinic3(user_logged.Clinic.Id, ThemeType.Group);
                    }*/
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

                ActivityEntity activityEntity = await _converterHelper.ToActivityEntity(activityViewModel, false, user_logged.UserName);
                _context.Update(activityEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<ActivityEntity> list = await _context.Activities
                                                              .Include(a => a.Theme)
                                                              .Include(a => a.Facilitator)
                                                              .ThenInclude(t => t.Clinic)
                                                              .Where(a => (a.Theme.Clinic.Id == user_logged.Clinic.Id 
                                                                        && a.Status == ActivityStatus.Pending
                                                                        && a.Theme.Day == null))
                                                              .OrderBy(a => a.Theme.Name)
                                                              .ToListAsync();


                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewActivities", list) });
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

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> EditSkill(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Workday_Activity_Facilitator activity = await _context.Workdays_Activities_Facilitators.FirstOrDefaultAsync(a => a.Id == id);
            if (activity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Workday_Activity_FacilitatorSkillViewModel activityViewModel = _converterHelper.ToWorkdayActivityFacilitatorViewModel(activity);

            return View(activityViewModel);
        }

        [Authorize(Roles = "Facilitator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSkill(Workday_Activity_FacilitatorSkillViewModel activityViewModel)
        {
            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

                Workday_Activity_Facilitator activityEntity = _converterHelper.ToWorkdayActivityFacilitatorEntity(activityViewModel);
                _context.Update(activityEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("ActivitiesPerWeek", "Activities");
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the facilitator's intervention: {activityEntity.Id}");
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