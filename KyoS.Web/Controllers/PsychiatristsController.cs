using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Manager")]
    public class PsychiatristsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        
        public PsychiatristsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
        }
        
        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }               
                
            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);

            if (clinic != null)
            {
                List<PsychiatristEntity> psychiatrists = await _context.Psychiatrists.OrderBy(d => d.Name).ToListAsync();
                List<PsychiatristEntity> psychiatrists_by_clinic = new List<PsychiatristEntity>();
                UserEntity user;
                foreach (PsychiatristEntity item in psychiatrists)
                {
                    user = _context.Users.FirstOrDefault(u => u.Id == item.CreatedBy);
                    if (clinic.Users.Contains(user))
                    {
                        psychiatrists_by_clinic.Add(item);
                    }
                }
                return View(psychiatrists_by_clinic);
            }
            else
            {
                return View(null);
            }            
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

            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            PsychiatristViewModel model = new PsychiatristViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PsychiatristViewModel psychiatristViewModel)
        {
            if (ModelState.IsValid)
            {
                PsychiatristEntity psychiatrist = await _context.Psychiatrists.FirstOrDefaultAsync(c => c.Name == psychiatristViewModel.Name);
                if (psychiatrist == null)
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    PsychiatristEntity psychiatristEntity = _converterHelper.ToPsychiatristEntity(psychiatristViewModel, true, user_logged.Id);

                    _context.Add(psychiatristEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the psychiatrist: {psychiatristEntity.Name}");
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
            return View(psychiatristViewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            PsychiatristEntity psychiatristEntity = await _context.Psychiatrists.FirstOrDefaultAsync(c => c.Id == id);
            if (psychiatristEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            PsychiatristViewModel psychiatristViewModel = _converterHelper.ToPsychiatristViewModel(psychiatristEntity);

            return View(psychiatristViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PsychiatristViewModel psychiatristViewModel)
        {
            if (id != psychiatristViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                PsychiatristEntity psychiatristEntity = _converterHelper.ToPsychiatristEntity(psychiatristViewModel, false, user_logged.Id);
                _context.Update(psychiatristEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the psychiatrist: {psychiatristEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(psychiatristViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            PsychiatristEntity psychiatristEntity = await _context.Psychiatrists.FirstOrDefaultAsync(c => c.Id == id);
            if (psychiatristEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Psychiatrists.Remove(psychiatristEntity);
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