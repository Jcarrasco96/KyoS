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
    public class TCMStagesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;

        public TCMStagesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper)
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
                return View(await _context.TCMStages
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


                return View(await _context.TCMStages
                                        .Include(f => f.Clinic)
                                        .Where(s => s.Clinic.Id == user_logged.Clinic.Id)
                                        .OrderBy(f => f.Name)
                                        .ToListAsync());
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

            TCMStageViewModel model;

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
                    model = new TCMStageViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        Id_TCMService = id,// revisar como es
                        //StatusList = _combosHelper.GetComboClientStatus(),
                        //UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, user_logged.Clinic.Id)
                    };
                    return View(model);
                }
            }

            model = new TCMStageViewModel
            {
                Clinics = _combosHelper.GetComboClinics(),
                Id_TCMService = id,
                //IdStatus = 1,
                //StatusList = _combosHelper.GetComboClientStatus(),
                //UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, 0)
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create(TCMStageViewModel tcmStageViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMStageEntity tcmStageEntity = await _context.TCMStages.FirstOrDefaultAsync(s => s.Name == tcmStageViewModel.Name);
                if (tcmStageEntity == null)
                {
                    /*if (tcmStageViewModel.IdUser == "0")
                    {
                        if (User.IsInRole("Admin"))
                        {
                            tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();
                            tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, 0);
                        }
                        else
                        {
                            List<SelectListItem> list = new List<SelectListItem>();
                            list.Insert(0, new SelectListItem
                            {
                                Text = user_logged.Clinic.Name,
                                Value = $"{user_logged.Clinic.Id}"
                            });
                            tcmSupervisorViewModel.Clinics = list;
                            tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, user_logged.Clinic.Id);
                        }

                        tcmSupervisorViewModel.IdStatus = 1;
                        tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();

                        ModelState.AddModelError(string.Empty, "You must select a linked user");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmSupervisorViewModel) });
                    }
                    string path = string.Empty;
                    if (tcmSupervisorViewModel.SignatureFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(tcmSupervisorViewModel.SignatureFile, "Signatures");
                    }
                    */
                    tcmStageEntity = await _converterHelper.ToTCMStageEntity(tcmStageViewModel, true);
                    _context.Add(tcmStageEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        List<TCMStageEntity> tcmStage = await _context.TCMStages

                                                                                .Include(s => s.Clinic)

                                                                                .OrderBy(t => t.Name)
                                                                                .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMStages", tcmStage) });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM stage.");

                    if (User.IsInRole("Admin"))
                    {
                        tcmStageViewModel.Clinics = _combosHelper.GetComboClinics();
                        
                    }
                    else
                    {
                        List<SelectListItem> list = new List<SelectListItem>();
                        list.Insert(0, new SelectListItem
                        {
                            Text = user_logged.Clinic.Name,
                            Value = $"{user_logged.Clinic.Id}"
                        });

                        tcmStageViewModel.Clinics = list;
                    }
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmStageViewModel) });
                }
            }

            //recovery data
            if (User.IsInRole("Admin"))
            {
                tcmStageViewModel.Clinics = _combosHelper.GetComboClinics();
            }
            else
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });

                tcmStageViewModel.Clinics = list;
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmStageViewModel) });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMStageEntity tcmStageEntity = await _context.TCMStages.FirstOrDefaultAsync(s => s.Id == id);
            if (tcmStageEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMStages.Remove(tcmStageEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
