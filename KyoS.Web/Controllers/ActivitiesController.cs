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
    [Authorize(Roles = "Admin, Facilitator")]
    public class ActivitiesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly ITranslateHelper _translateHelper;
        public ActivitiesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, ITranslateHelper translateHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _translateHelper = translateHelper;
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

        public JsonResult Translate(string text)
        {
            return Json(text = _translateHelper.TranslateText("es", "en", text));
        }

        public async Task<IActionResult> ActivitiesPerWeek()
        {
            if (User.IsInRole("Admin"))
                return View(await _context.Weeks.Include(w => w.Clinic)
                                                .Include(w => w.Days).ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                    return View(null);

                return View(await _context.Weeks.Include(w => w.Days)
                                                .ThenInclude(d => d.Workdays_Activities_Facilitators)
                                                .ThenInclude(waf => waf.Activity)
                                                .ThenInclude(a => a.Theme)
                                                .Where(w => w.Clinic.Id == user_logged.Clinic.Id)
                                                .ToListAsync());
            }
        }

        public async Task<IActionResult> CreateActivitiesWeek(int id = 0)
        {
            List<ThemeEntity> topics;
            List<SelectListItem> list1 = new List<SelectListItem>();
            List<SelectListItem> list2 = new List<SelectListItem>();
            List<SelectListItem> list3 = new List<SelectListItem>();
            List<SelectListItem> list4 = new List<SelectListItem>();
            Workday_Activity_FacilitatorViewModel model;
            UserEntity user_logged;

            WorkdayEntity workday = await _context.Workdays.Include(wd => wd.Week)
                                                           .ThenInclude(w => w.Clinic)
                                                           
                                                           .Include(wd => wd.Workdays_Clients)
                                                           .ThenInclude(wc => wc.Note)

                                                           .FirstOrDefaultAsync(w => w.Id == id);
            
            //el workday ya tiene notas creadas por tanto no es posible su edición
            if (WorkdayReadOnly(workday))
            {
                ViewBag.Error = "0";
                return View(null);
            }

            FacilitatorEntity facilitator_logged = await _context.Facilitators
                                                                 .FirstOrDefaultAsync(f => f.LinkedUser == User.Identity.Name);

            if ((workday == null) || (facilitator_logged == null))
            {
                return NotFound();
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
                user_logged = await _context.Users
                                            .Include(u => u.Clinic)
                                            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

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

                    IdTopic1 = (list1.Count != 0) ? topics[0].Id : 0,
                    Topics1 = (list1.Count != 0) ? list1 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id),
                    Activities1 = (list1.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[0].Id) : null,

                    IdTopic2 = (list2.Count != 0) ? topics[1].Id : 0,
                    Topics2 = (list2.Count != 0) ? list2 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id),
                    Activities2 = (list2.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[1].Id) : null,

                    IdTopic3 = (list3.Count != 0) ? topics[2].Id : 0,
                    Topics3 = (list3.Count != 0) ? list3 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id),
                    Activities3 = (list3.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[2].Id) : null,

                    IdTopic4 = (list4.Count != 0) ? topics[3].Id : 0,
                    Topics4 = (list4.Count != 0) ? list4 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id),
                    Activities4 = (list4.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[3].Id) : null,
                };
            }
            else
            {
                user_logged = await _context.Users
                                            .Include(u => u.Clinic)
                                            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

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

                    IdTopic1 = (activities_list.Count > 0) ? activities_list[0].Activity.Theme.Id : 0,
                    Topics1 = (list1.Count != 0) ? list1 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id),
                    IdActivity1 = (activities_list.Count > 0) ? activities_list[0].Activity.Id : 0,
                    Activities1 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 0) ? activities_list[0].Activity.Theme.Id : 0),

                    IdTopic2 = (activities_list.Count > 1) ? activities_list[1].Activity.Theme.Id : 0,
                    Topics2 = (list2.Count != 0) ? list2 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id),
                    IdActivity2 = (activities_list.Count > 1) ? activities_list[1].Activity.Id : 0,
                    Activities2 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 1) ? activities_list[1].Activity.Theme.Id : 0),

                    IdTopic3 = (activities_list.Count > 2) ? activities_list[2].Activity.Theme.Id : 0,
                    Topics3 = (list3.Count != 0) ? list3 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id),
                    IdActivity3 = (activities_list.Count > 2) ? activities_list[2].Activity.Id : 0,
                    Activities3 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 2) ? activities_list[2].Activity.Theme.Id : 0),

                    IdTopic4 = (activities_list.Count > 3) ? activities_list[3].Activity.Theme.Id : 0,
                    Topics4 = (list4.Count != 0) ? list4 : _combosHelper.GetComboThemesByClinic(workday.Week.Clinic.Id),
                    IdActivity4 = (activities_list.Count > 3) ? activities_list[3].Activity.Id : 0,
                    Activities4 = _combosHelper.GetComboActivitiesByTheme((activities_list.Count > 3) ? activities_list[3].Activity.Theme.Id : 0),
                };
            }            

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivitiesWeek(Workday_Activity_FacilitatorViewModel model)
        {
            if (ModelState.IsValid)
            {
                FacilitatorEntity facilitator_logged = await _context.Facilitators
                                                              .FirstOrDefaultAsync(f => f.LinkedUser == User.Identity.Name);

                WorkdayEntity workday = await _context.Workdays
                                                      .FirstOrDefaultAsync(w => w.Id == model.IdWorkday);

                if ((facilitator_logged == null) || (workday == null))
                {
                    return NotFound();
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
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity1)
                };
                _context.Add(activity);
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity2)
                };
                _context.Add(activity);
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity3)
                };
                _context.Add(activity);
                activity = new Workday_Activity_Facilitator
                {
                    Facilitator = facilitator_logged,
                    Workday = workday,
                    Activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == model.IdActivity4)
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

        public JsonResult GetActivityList(int idTheme)
        {
            List<ActivityEntity> activities = _context.Activities.Where(a => a.Theme.Id == idTheme).ToList();

            return Json(new SelectList(activities, "Id", "Name"));
        }

        public bool WorkdayReadOnly(WorkdayEntity workday)
        {
            if (workday.Workdays_Clients.Count > 0)
            {
                foreach (Workday_Client item in workday.Workdays_Clients)
                {
                    if (item.Note != null)
                        return true;
                }
                return false;
            }
            else
                return false;
        }
    }
}