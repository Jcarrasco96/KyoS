using Microsoft.AspNetCore.Mvc;
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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class TCMsupervisorsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;
        public TCMsupervisorsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _renderHelper = renderHelper;
        }

        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Admin"))
                return View(await _context.TCMsupervisors.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                    return View(await _context.TCMsupervisors.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                    return View(await _context.TCMsupervisors.Include(f => f.Clinic)
                                                          .Where(s => s.Clinic.Id == clinic.Id).OrderBy(f => f.Name).ToListAsync());
                else
                    return View(await _context.TCMsupervisors.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());
            }
        }
        [Authorize(Roles = "Admin, Manager")]
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

            TCMsupervisorViewModel model;

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
               /* if (user_logged.Clinic != null)
                {
                    ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });
                    model = new TCMsupervisorViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMsupervisor, user_logged.Clinic.Id)
                    };
                    return View(model);
                }*/
            }

            model = new TCMsupervisorViewModel
            {
                Clinics = _combosHelper.GetComboClinics(),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMsupervisor, 0)
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create(TCMsupervisorViewModel tcmSupervisorViewModel)
        {
            if (ModelState.IsValid)
            {
                TCMsupervisorEntity tcmSupervisorEntity = await _context.TCMsupervisors.FirstOrDefaultAsync(s => s.Name == tcmSupervisorViewModel.Name);
                if (tcmSupervisorEntity == null)
                {
                    if (tcmSupervisorViewModel.IdUser == "0")
                    {
                        tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();
                        tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMsupervisor, 0);
                        ModelState.AddModelError(string.Empty, "You must select a linked user");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmSupervisorViewModel) });
                    }
                    string path = string.Empty;
                    if (tcmSupervisorViewModel.SignatureFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(tcmSupervisorViewModel.SignatureFile, "Signatures");
                    }

                    tcmSupervisorEntity = await _converterHelper.ToTCMsupervisorEntity(tcmSupervisorViewModel, path, true);
                    _context.Add(tcmSupervisorEntity);
                    try
                    {
                          await _context.SaveChangesAsync();
                          return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMsupervisors", "TCM supervisor has been successfully created") });
                    }
                    catch (System.Exception ex)
                    {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMsupervisors", "Error. TCM supervisor already exist") });
                }
            }
            tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();
            tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMsupervisor, 0);
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmSupervisorViewModel) });
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMsupervisorEntity tcmSupervisorEntity = await _context.TCMsupervisors.FirstOrDefaultAsync(s => s.Id == id);
            if (tcmSupervisorEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMsupervisors.Remove(tcmSupervisorEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMsupervisorEntity tcmSupervisorEntity = await _context.TCMsupervisors.Include(s => s.Clinic).FirstOrDefaultAsync(s => s.Id == id);
            if (tcmSupervisorEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMsupervisorViewModel tcmSupervisorViewModel;

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                tcmSupervisorViewModel = _converterHelper.ToTCMsupervisorViewModel(tcmSupervisorEntity, user_logged.Clinic.Id);
               
                if (user_logged.Clinic != null)
                {
                   tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();
                }
                if (tcmSupervisorViewModel.LinkedUser != null)
                {
                   tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMsupervisor, 0);
                }
                tcmSupervisorViewModel.TCM_Active = tcmSupervisorEntity.TCMActive;
            }
            else
                tcmSupervisorViewModel = _converterHelper.ToTCMsupervisorViewModel(tcmSupervisorEntity, 0);
           return View(tcmSupervisorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int id, TCMsupervisorViewModel tcmSupervisorViewModel)
        {
            if (id != tcmSupervisorViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                if (tcmSupervisorViewModel.IdUser == "0")
                {
                    tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();
                    tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMsupervisor, 0);
                    ModelState.AddModelError(string.Empty, "You must select a linked user");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmSupervisorViewModel) });
                }
                string path = tcmSupervisorViewModel.SignaturePath;
                if (tcmSupervisorViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(tcmSupervisorViewModel.SignatureFile, "Signatures");
                }
                TCMsupervisorEntity tcmSupervisorEntity = await _converterHelper.ToTCMsupervisorEntity(tcmSupervisorViewModel, path, false);
                _context.Update(tcmSupervisorEntity);
                try
                {
                   await _context.SaveChangesAsync();
                   return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMsupervisors", "TCM supervisor has been successfully updated") });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                       ModelState.AddModelError(string.Empty, $"Already exists the TCM supervisor: {tcmSupervisorEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message); // cuando sale el texto en español, no lo detecta
                    }
                }
            }
            tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();
            tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMsupervisor, 0);
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmSupervisorViewModel) });
            
        }
    }
}
