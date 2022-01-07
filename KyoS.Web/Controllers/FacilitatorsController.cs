using KyoS.Common.Enums;
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
    [Authorize(Roles = "Admin, Mannager")]
    public class FacilitatorsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;

        public FacilitatorsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
        }

        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Admin"))
               return View(await _context.Facilitators.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                   return View(await _context.Facilitators.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                    return View(await _context.Facilitators.Include(f => f.Clinic)
                                                          .Where(f => f.Clinic.Id == clinic.Id).OrderBy(f => f.Name).ToListAsync());
                else
                    return View(await _context.Facilitators.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());
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

            FacilitatorViewModel model;

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
                    model = new FacilitatorViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        StatusList = _combosHelper.GetComboClientStatus(),
                        UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Facilitator, user_logged.Clinic.Id)
                    };
                    return View(model);
                }
            }

            model = new FacilitatorViewModel
            {
                Clinics = _combosHelper.GetComboClinics(),
                IdStatus = 1,
                StatusList = _combosHelper.GetComboClientStatus(),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Facilitator, 0)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacilitatorViewModel facilitatorViewModel)
        {
            if (ModelState.IsValid)
            {
                FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.Name == facilitatorViewModel.Name);
                if (facilitator == null)
                {
                    if (facilitatorViewModel.IdUser == "0")
                    {
                        ModelState.AddModelError(string.Empty, "You must select a linked user");
                        return View(facilitatorViewModel);
                    }

                    string path = string.Empty;
                    if (facilitatorViewModel.SignatureFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(facilitatorViewModel.SignatureFile, "Signatures");
                    }

                    FacilitatorEntity facilitatorEntity = await _converterHelper.ToFacilitatorEntity(facilitatorViewModel, path, true);

                    _context.Add(facilitatorEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the facilitator: {facilitatorEntity.Name}");
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
            return View(facilitatorViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            FacilitatorEntity facilitatorEntity = await _context.Facilitators.FirstOrDefaultAsync(t => t.Id == id);
            if (facilitatorEntity == null)
            {
                return NotFound();
            }

            try
            {
                _context.Facilitators.Remove(facilitatorEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            FacilitatorEntity facilitatorEntity = await _context.Facilitators.Include(f => f.Clinic).FirstOrDefaultAsync(f => f.Id == id);
            if (facilitatorEntity == null)
            {
                return NotFound();
            }

            FacilitatorViewModel facilitatorViewModel;
            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                facilitatorViewModel = _converterHelper.ToFacilitatorViewModel(facilitatorEntity, user_logged.Clinic.Id);
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    facilitatorViewModel.Clinics = list;
                }

            }
            else
                facilitatorViewModel = _converterHelper.ToFacilitatorViewModel(facilitatorEntity, 0);

            return View(facilitatorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FacilitatorViewModel facilitatorViewModel)
        {
            if (id != facilitatorViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string path = facilitatorViewModel.SignaturePath;
                if (facilitatorViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(facilitatorViewModel.SignatureFile, "Signatures");
                }
                FacilitatorEntity facilitatorEntity = await _converterHelper.ToFacilitatorEntity(facilitatorViewModel, path, false);
                _context.Update(facilitatorEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the facilitator: {facilitatorEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(facilitatorViewModel);
        }
    }
}