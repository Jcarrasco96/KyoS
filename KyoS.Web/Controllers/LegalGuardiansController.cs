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
    [Authorize(Roles = "Admin, Mannager")]
    public class LegalGuardiansController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        public LegalGuardiansController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
        }
        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Admin"))
            {
                return View(await _context.LegalGuardians.OrderBy(d => d.Name).ToListAsync());
            }
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                {
                    return View(await _context.LegalGuardians.OrderBy(d => d.Name).ToListAsync());
                }

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);

                if (clinic != null)
                {
                    List<LegalGuardianEntity> legalguardians = await _context.LegalGuardians.OrderBy(lg => lg.Name).ToListAsync();
                    List<LegalGuardianEntity> lg_by_clinic = new List<LegalGuardianEntity>();
                    UserEntity user;
                    foreach (LegalGuardianEntity item in legalguardians)
                    {
                        user = _context.Users.FirstOrDefault(u => u.Id == item.CreatedBy);
                        if (clinic.Users.Contains(user))
                        {
                            lg_by_clinic.Add(item);
                        }
                    }
                    return View(lg_by_clinic);
                }
                else
                {
                    return View(null);
                }
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

            LegalGuardianViewModel model = new LegalGuardianViewModel()
            {
                Country = "United States",
                City = "Miami",
                State = "Florida"
            };            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LegalGuardianViewModel legalGuardianViewModel)
        {
            if (ModelState.IsValid)
            {
                LegalGuardianEntity legalGuardian = await _context.LegalGuardians.FirstOrDefaultAsync(c => c.Name == legalGuardianViewModel.Name);
                if (legalGuardian == null)
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    LegalGuardianEntity legalGuardianEntity = _converterHelper.ToLegalGuardianEntity(legalGuardianViewModel, true, user_logged.Id);

                    _context.Add(legalGuardianEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the legal guardian: {legalGuardianEntity.Name}");
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
            return View(legalGuardianViewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            LegalGuardianEntity legalGuardianEntity = await _context.LegalGuardians.FirstOrDefaultAsync(c => c.Id == id);
            if (legalGuardianEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            LegalGuardianViewModel legalGuardianViewModel = _converterHelper.ToLegalGuardianViewModel(legalGuardianEntity);

            return View(legalGuardianViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LegalGuardianViewModel legalGuardianViewModel)
        {
            if (id != legalGuardianViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                LegalGuardianEntity legalGuardianEntity = _converterHelper.ToLegalGuardianEntity(legalGuardianViewModel, false, user_logged.Id);
                _context.Update(legalGuardianEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the legal guardian: {legalGuardianEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(legalGuardianViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            LegalGuardianEntity legalGuardiansEntity = await _context.LegalGuardians.FirstOrDefaultAsync(c => c.Id == id);
            if (legalGuardiansEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.LegalGuardians.Remove(legalGuardiansEntity);
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