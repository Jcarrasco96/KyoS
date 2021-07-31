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
    public class EmergencyContactsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        public EmergencyContactsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
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
                return View(await _context.EmergencyContacts.OrderBy(d => d.Name).ToListAsync());
            }
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                {
                    return View(await _context.EmergencyContacts.OrderBy(d => d.Name).ToListAsync());
                }

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);

                if (clinic != null)
                {
                    List<EmergencyContactEntity> ec = await _context.EmergencyContacts.OrderBy(d => d.Name).ToListAsync();
                    List<EmergencyContactEntity> ec_by_clinic = new List<EmergencyContactEntity>();
                    UserEntity user;
                    foreach (EmergencyContactEntity item in ec)
                    {
                        user = _context.Users.FirstOrDefault(u => u.Id == item.CreatedBy);
                        if (clinic.Users.Contains(user))
                        {
                            ec_by_clinic.Add(item);
                        }
                    }
                    return View(ec_by_clinic);
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

            EmergencyContactViewModel model = new EmergencyContactViewModel()
            {
                Country = "United States",
                City = "Miami",
                State = "Florida"
            };
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmergencyContactViewModel emergencyContactViewModel)
        {
            if (ModelState.IsValid)
            {
                EmergencyContactEntity emergencyContact = await _context.EmergencyContacts.FirstOrDefaultAsync(c => c.Name == emergencyContactViewModel.Name);
                if (emergencyContact == null)
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    EmergencyContactEntity emergencyContactEntity = _converterHelper.ToEmergencyContactEntity(emergencyContactViewModel, true, user_logged.Id);

                    _context.Add(emergencyContactEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the emergency contact: {emergencyContactEntity.Name}");
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
            return View(emergencyContactViewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            EmergencyContactEntity emergencyContactEntity = await _context.EmergencyContacts.FirstOrDefaultAsync(c => c.Id == id);
            if (emergencyContactEntity == null)
            {
                return NotFound();
            }

            EmergencyContactViewModel emergencyContactViewModel = _converterHelper.ToEmergencyContactViewModel(emergencyContactEntity);

            return View(emergencyContactViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmergencyContactViewModel emergencyContactViewModel)
        {
            if (id != emergencyContactViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                EmergencyContactEntity emergencyContactEntity = _converterHelper.ToEmergencyContactEntity(emergencyContactViewModel, false, user_logged.Id);
                _context.Update(emergencyContactEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the emergency contact: {emergencyContactEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(emergencyContactViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            EmergencyContactEntity emergencyContactEntity = await _context.EmergencyContacts.FirstOrDefaultAsync(c => c.Id == id);
            if (emergencyContactEntity == null)
            {
                return NotFound();
            }

            try
            {
                _context.EmergencyContacts.Remove(emergencyContactEntity);
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