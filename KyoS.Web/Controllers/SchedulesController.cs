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
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    public class SchedulesController:Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;

        public SchedulesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
        }

        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            List<ScheduleEntity> schedules = new List<ScheduleEntity>();
            if (User.IsInRole("Manager"))
            {
                schedules = await _context.Schedule
                                         .Include(n => n.SubSchedules)
                                         .OrderBy(d => d.Service).ThenBy(n => n.Session).ThenBy(n => n.InitialTime)
                                         .ToListAsync();
            }
            return View(schedules);
        }

        public async Task<IActionResult> Create(int id = 0)
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

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ScheduleViewModel model = new ScheduleViewModel();
            model = new  ScheduleViewModel()
            {
                IdService = 0,
                Services = _combosHelper.GetComboServices(),
                IdSession = 0,
                Sessions = _combosHelper.GetComboSession()                

            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleViewModel scheduleViewModel)
        {
           
            if (ModelState.IsValid)
            {
                ScheduleEntity schedule = await _context.Schedule.FirstOrDefaultAsync(c => c.Id == scheduleViewModel.Id);
                if (schedule == null)
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    ScheduleEntity scheduleEntity = _converterHelper.ToScheduleEntity(scheduleViewModel,true, user_logged);

                    _context.Add(scheduleEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<ScheduleEntity> listSchedules = await _context.Schedule
                                                                           .Include(n => n.SubSchedules)
                                                                           .OrderBy(d => d.Service).ThenBy(n => n.Session).ThenBy(n => n.InitialTime)
                                                                           .ToListAsync();
                        
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSchedules", listSchedules)});
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the schedule: {scheduleEntity.Id}");
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
            return View(scheduleViewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ScheduleEntity scheduleEntity = await _context.Schedule.FirstOrDefaultAsync(c => c.Id == id);
            if (scheduleEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ScheduleViewModel scheduleViewModel = _converterHelper.ToScheduleViewModel(scheduleEntity);

            return View(scheduleViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ScheduleViewModel scheduleViewModel)
        {
            if (id != scheduleViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                ScheduleEntity scheduleEntity = _converterHelper.ToScheduleEntity(scheduleViewModel, false, user_logged);
                _context.Update(scheduleEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    List<ScheduleEntity> listSchedules = await _context.Schedule
                                                                           .Include(n => n.SubSchedules)
                                                                           .OrderBy(d => d.Service).ThenBy(n => n.Session).ThenBy(n => n.InitialTime)
                                                                           .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSchedules", listSchedules) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the schedule: {scheduleEntity.Id}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(scheduleViewModel);
        }

        [Authorize(Roles = "Manager")]
        public IActionResult Delete(int id = 0)
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
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(DeleteViewModel scheduleViewModel)
        {
            
            if (ModelState.IsValid)
            {
                ScheduleEntity schedule = await _context.Schedule
                                                    .Include(n => n.SubSchedules)
                                                    .FirstAsync(n => n.Id == scheduleViewModel.Id_Element);

                try
                {
                    _context.Schedule.Remove(schedule);
                    await _context.SaveChangesAsync();
                    List<ScheduleEntity> ListSchedule = await _context.Schedule
                                                                      .Include(n => n.SubSchedules)
                                                                      .OrderBy(d => d.Service).ThenBy(n => n.Session).ThenBy(n => n.InitialTime)
                                                                      .ToListAsync();
                    
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSchedules", ListSchedule) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSchedules", _context.Schedule.Include(n => n.SubSchedules).ToListAsync()) });
                }

               
            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSchedules", _context.Schedule.Include(n => n.SubSchedules).ToListAsync()) });
        }

        public async Task<IActionResult> CreateSubSchedule(int id = 0, int error = 0)
        {
            ViewData["error"] = error;
           
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ScheduleEntity schedule = await _context.Schedule
                                                    .Include(n => n.SubSchedules)
                                                    .FirstOrDefaultAsync(n => n.Id == id);

            DateTime initial = new DateTime();
            DateTime end = new DateTime();

            if (schedule.SubSchedules.Count() == 0)
            {
                initial = schedule.InitialTime;
                end = schedule.EndTime;
            }
            else
            {
                initial = schedule.SubSchedules.Max(n => n.EndTime).AddMinutes(5);
                end = schedule.EndTime;
            }
            SubScheduleViewModel model = new SubScheduleViewModel();
            model = new SubScheduleViewModel()
            {
                IdSchedule = id,
                InitialTime = initial,
                EndTime = end
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubSchedule(SubScheduleViewModel subScheduleViewModel)
        {

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                          .FirstOrDefault(u => u.UserName == User.Identity.Name);

                SubScheduleEntity subScheduleEntity = _converterHelper.ToSubScheduleEntity(subScheduleViewModel, true, user_logged);

                if (this.VerifySubScheduleTime(subScheduleViewModel.InitialTime, subScheduleViewModel.EndTime, subScheduleViewModel.IdSchedule, 0))
                {
                    ViewData["error"] = 1;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateSubSchedule", subScheduleViewModel) });
                }

                _context.Add(subScheduleEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<ScheduleEntity> listSchedules = await _context.Schedule
                                                                       .Include(n => n.SubSchedules)
                                                                       .OrderBy(d => d.Service).ThenBy(n => n.Session).ThenBy(n => n.InitialTime)
                                                                       .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSchedules", listSchedules) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the schedule: {subScheduleEntity.Id}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateSubSchedule", subScheduleViewModel) });
        }

        public async Task<IActionResult> EditSubSchedule(int? id, int error = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ViewData["error"] = error;

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            SubScheduleEntity subScheduleEntity = await _context.SubSchedule
                                                                .Include(n => n.Schedule)
                                                                .FirstOrDefaultAsync(c => c.Id == id);
            if (subScheduleEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SubScheduleViewModel subScheduleViewModel = _converterHelper.ToSubScheduleViewModel(subScheduleEntity);

            return View(subScheduleViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubSchedule(int id, SubScheduleViewModel subScheduleViewModel)
        {
            if (id != subScheduleViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);
                SubScheduleEntity subScheduleEntity = _converterHelper.ToSubScheduleEntity(subScheduleViewModel, false, user_logged);

                /*if (this.VerifySubScheduleTime(subScheduleViewModel.IdSchedule, subScheduleViewModel.Id, subScheduleViewModel.InitialTime, subScheduleViewModel.EndTime))
                 {
                     ViewData["error"] = 1;
                     return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditSubSchedule", subScheduleViewModel) });
                 }*/
                 if (subScheduleViewModel.InitialTime > subScheduleViewModel.EndTime)
                 {
                     ViewData["error"] = 1;
                     return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditSubSchedule", subScheduleViewModel) });
                 }

                /* List<SubScheduleEntity> ListSubSchedule = await _context.SubSchedule
                                                                         .Where(n => n.Schedule.Id == subScheduleViewModel.IdSchedule)
                                                                         .ToListAsync();
                 foreach(var item in ListSubSchedule)
                 {
                     if (item.Id != subScheduleViewModel.Id && item.InitialTime <= subScheduleViewModel.InitialTime && item.EndTime >= subScheduleViewModel.InitialTime)
                     {
                         ViewData["error"] = 1;
                         return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditSubSchedule", subScheduleViewModel) });
                     }

                 }*/

                _context.Update(subScheduleEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    List<ScheduleEntity> listSchedules = await _context.Schedule
                                                                       .Include(n => n.SubSchedules)
                                                                       .OrderBy(d => d.Service).ThenBy(n => n.Session).ThenBy(n => n.InitialTime)
                                                                       .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSchedules", listSchedules) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the schedule: {subScheduleEntity.Id}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(subScheduleViewModel);
        }

        [Authorize(Roles = "Manager")]
        public IActionResult DeleteSubSchedule(int id = 0)
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
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteSubSchedule(DeleteViewModel subScheduleViewModel)
        {

            if (ModelState.IsValid)
            {
                SubScheduleEntity subSchedule = await _context.SubSchedule
                                                              .FirstAsync(n => n.Id == subScheduleViewModel.Id_Element);

                try
                {
                    _context.SubSchedule.Remove(subSchedule);
                    await _context.SaveChangesAsync();
                    List<ScheduleEntity> ListSchedule = await _context.Schedule
                                                                      .Include(n => n.SubSchedules)
                                                                      .OrderBy(d => d.Service).ThenBy(n => n.Session).ThenBy(n => n.InitialTime)
                                                                      .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSchedules", ListSchedule) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSchedules", _context.Schedule.Include(n => n.SubSchedules).ToListAsync()) });
                }


            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSchedules", _context.Schedule.Include(n => n.SubSchedules).ToListAsync()) });
        }

        #region Utils funtions
        private bool VerifySubScheduleTime(DateTime initialTime, DateTime endTime, int idSchedule = 0, int idSubSchedule = 0)
        {
            ScheduleEntity schedule = _context.Schedule
                                              .Include(n => n.SubSchedules)
                                              .FirstOrDefault(n => n.Id == idSchedule);
            if (endTime <= initialTime)
            {
                return true;
            }
            DateTime inicio = schedule.InitialTime;
            DateTime final = schedule.EndTime;
            if (schedule != null)
            {
                if (initialTime.TimeOfDay >= inicio.TimeOfDay && initialTime.TimeOfDay < final.TimeOfDay
                    && endTime.TimeOfDay > inicio.TimeOfDay && endTime.TimeOfDay <= final.TimeOfDay)
                {
                    if (schedule.SubSchedules.Count() > 0)
                    {
                        foreach (var item in schedule.SubSchedules)
                        {
                            if (item.Id != idSubSchedule)
                            {
                                if ((initialTime.TimeOfDay >= item.InitialTime.TimeOfDay && initialTime.TimeOfDay <= item.EndTime.TimeOfDay)
                                    || (endTime.TimeOfDay >= item.InitialTime.TimeOfDay && endTime.TimeOfDay <= item.EndTime.TimeOfDay))
                                {
                                    return true;
                                }
                            }

                        }
                    }
                    return false;
                }
                else
                    return true;
            }
            
           
            return true;
        }
        #endregion
    }
}
