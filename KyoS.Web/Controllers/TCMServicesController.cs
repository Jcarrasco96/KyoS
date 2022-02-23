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
    public class TCMServicesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;

        public TCMServicesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _renderHelper = renderHelper;
        }
        public async Task<IActionResult> Index(int idError = 0)
        {
            if (User.IsInRole("Admin"))
            {
                return View(await _context.TCMServices
                                          .Include(f => f.Clinic)
                                          .OrderBy(f => f.Name)
                                          .ToListAsync());
            }
            else
            {
                UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                if (idError == 1) //Imposible to delete
                {
                    ViewBag.Delete = "N";
                }


                return View(await _context.TCMServices
                                        .Include(f => f.Clinic)
                                        .Where(s => s.Clinic.Id == user_logged.Clinic.Id)
                                        .OrderBy(f => f.Name)
                                        .ToListAsync());
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

            TCMServiceViewModel model;

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
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
                    model = new TCMServiceViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id
                    };
                    return View(model);
                }
            }

            model = new TCMServiceViewModel
            {
                Clinics = _combosHelper.GetComboClinics(),
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create(TCMServiceViewModel tcmServiceViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMServiceEntity tcmServiceEntity = await _context.TCMServices.FirstOrDefaultAsync(s => s.Name == tcmServiceViewModel.Name);
                if (tcmServiceEntity == null)
                {
                    /*if (tcmServisorViewModel.IdUser == "0")
                    {
                        if (User.IsInRole("Admin"))
                        {
                            tcmServiceViewModel.Clinics = _combosHelper.GetComboClinics();
                        }
                        else
                        {
                            List<SelectListItem> list = new List<SelectListItem>();
                            list.Insert(0, new SelectListItem
                            {
                                Text = user_logged.Clinic.Name,
                                Value = $"{user_logged.Clinic.Id}"
                            });
                            tcmServiceViewModel.Clinics = list;
                        }
   
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServiceViewModel) });
                    }*/
                    
                    tcmServiceEntity = await _converterHelper.ToTCMServiceEntity(tcmServiceViewModel,true);
                    _context.Add(tcmServiceEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        List<TCMServiceEntity> tcmService = await _context.TCMServices

                                                                                .Include(s => s.Clinic)

                                                                                .OrderBy(t => t.Name)
                                                                                .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMServices", tcmService) });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM service.");

                    if (User.IsInRole("Admin"))
                    {
                        tcmServiceViewModel.Clinics = _combosHelper.GetComboClinics();
                    }
                    else
                    {
                        List<SelectListItem> list = new List<SelectListItem>();
                        list.Insert(0, new SelectListItem
                        {
                            Text = user_logged.Clinic.Name,
                            Value = $"{user_logged.Clinic.Id}"
                        });

                        tcmServiceViewModel.Clinics = list;
                    }
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServiceViewModel) });
                }
            }

            //recovery data
            if (User.IsInRole("Admin"))
            {
                tcmServiceViewModel.Clinics = _combosHelper.GetComboClinics();
            }
            else
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });

                tcmServiceViewModel.Clinics = list;
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServiceViewModel) });
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMServiceEntity tcmServiceEntity = await _context.TCMServices.FirstOrDefaultAsync(s => s.Id == id);
            if (tcmServiceEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMServices.Remove(tcmServiceEntity);
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

            TCMServiceEntity tcmServiceEntity = await _context.TCMServices.Include(s => s.Clinic).FirstOrDefaultAsync(s => s.Id == id);
            if (tcmServiceEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMServiceViewModel tcmServiceViewModel;

            if (!User.IsInRole("Admin"))
            {
                tcmServiceViewModel = _converterHelper.ToTCMServiceViewModel(tcmServiceEntity, user_logged.Clinic.Id);

                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });

                tcmServiceViewModel.Clinics = list;
                tcmServiceViewModel.IdClinic = user_logged.Clinic.Id;
            }
            else
            {
                tcmServiceViewModel = _converterHelper.ToTCMServiceViewModel(tcmServiceEntity, 0);
            }

            return View(tcmServiceViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int id, TCMServiceViewModel tcmServiceViewModel)
        {
            if (id != tcmServiceViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                /*if (tcmServiceViewModel.IdUser == "0")
                {
                    if (User.IsInRole("Admin"))
                    {
                        tcmServiceViewModel.Clinics = _combosHelper.GetComboClinics();
                    }
                    else
                    {
                        List<SelectListItem> list = new List<SelectListItem>();
                        list.Insert(0, new SelectListItem
                        {
                            Text = user_logged.Clinic.Name,
                            Value = $"{user_logged.Clinic.Id}"
                        });
                        tcmServiceViewModel.Clinics = list;
                    }

                    ModelState.AddModelError(string.Empty, "You must select a linked user");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmServiceViewModel) });
                }*/

                TCMServiceEntity tcmServiceEntity = await _converterHelper.ToTCMServiceEntity(tcmServiceViewModel, false);
                _context.Update(tcmServiceEntity);

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMServiceEntity> tcmService = await _context.TCMServices

                                                                            .Include(s => s.Clinic)

                                                                            .OrderBy(f => f.Name)
                                                                            .ToListAsync();
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMServices", tcmService) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the TCM service: {tcmServiceEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            //recovery data
            if (User.IsInRole("Admin"))
            {
                tcmServiceViewModel.Clinics = _combosHelper.GetComboClinics();
            }
            else
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });

                tcmServiceViewModel.Clinics = list;
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmServiceViewModel) });
        }
    }
}
