using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class SupervisorsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        public SupervisorsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper)
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
                return View(await _context.Supervisors.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u. Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if(user_logged.Clinic == null)
                    return View(await _context.Supervisors.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());               

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if(clinic != null)                 
                    return View(await _context.Supervisors.Include(f => f.Clinic)
                                                          .Where(s => s.Clinic.Id == clinic.Id).OrderBy(f => f.Name).ToListAsync());                     
                else
                    return View(await _context.Supervisors.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());
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

            SupervisorViewModel model;

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
                    model = new SupervisorViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Supervisor, user_logged.Clinic.Id)
                    };                    
                    return View(model);
                }
            }
            
            model = new SupervisorViewModel
            {
                 Clinics = _combosHelper.GetComboClinics(),
                 UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Supervisor, 0)
            };
            return View(model);                        
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupervisorViewModel supervisorViewModel)
        {
            if (ModelState.IsValid)
            {
                SupervisorEntity supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Name == supervisorViewModel.Name);
                if (supervisor == null)
                {
                    if (supervisorViewModel.IdUser == "0")
                    {
                        ModelState.AddModelError(string.Empty, "You must select a linked user");
                        return View(supervisorViewModel);
                    }

                    string path = string.Empty;
                    if (supervisorViewModel.SignatureFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(supervisorViewModel.SignatureFile, "Signatures");
                    }

                    SupervisorEntity supervisorEntity = await _converterHelper.ToSupervisorEntity(supervisorViewModel, path, true);

                    _context.Add(supervisorEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the supervisor: {supervisorEntity.Name}");
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
            return View(supervisorViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SupervisorEntity supervisorEntity = await _context.Supervisors.FirstOrDefaultAsync(s => s.Id == id);
            if (supervisorEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }        

            try
            {
                _context.Supervisors.Remove(supervisorEntity);
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

            SupervisorEntity supervisorEntity = await _context.Supervisors.Include(s => s.Clinic).FirstOrDefaultAsync(s => s.Id == id);
            if (supervisorEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SupervisorViewModel supervisorViewModel;

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                supervisorViewModel = _converterHelper.ToSupervisorViewModel(supervisorEntity, user_logged.Clinic.Id);
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    supervisorViewModel.Clinics = list;
                }
            }            
            else
                supervisorViewModel = _converterHelper.ToSupervisorViewModel(supervisorEntity, 0);

            return View(supervisorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupervisorViewModel supervisorViewModel)
        {
            if (id != supervisorViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                string path = supervisorViewModel.SignaturePath;
                if (supervisorViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(supervisorViewModel.SignatureFile, "Signatures");
                }
                SupervisorEntity supervisorEntity = await _converterHelper.ToSupervisorEntity(supervisorViewModel, path, false);
                _context.Update(supervisorEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the supervisor: {supervisorEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(supervisorViewModel);
        }
    }
}