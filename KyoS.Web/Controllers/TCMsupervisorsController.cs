 using KyoS.Common.Enums;
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
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class TCMSupervisorsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;
        
        public TCMSupervisorsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper)
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
                return View(await _context.TCMSupervisors
                                          .Include(f => f.Clinic)
                                          .Include(f => f.CaseManagerList)
                                          .ThenInclude(f => f.TCMClients)
                                          .ThenInclude(f => f.Client)
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

                List<TCMSupervisorEntity> tcmSupervisorList = await _context.TCMSupervisors
                                                                            .Include(f => f.Clinic)
                                                                            .Include(f => f.CaseManagerList)
                                                                            .ThenInclude(f => f.TCMClients)
                                                                            .ThenInclude(f => f.Client)
                                                                            .Where(s => s.Clinic.Id == user_logged.Clinic.Id)
                                                                            .OrderBy(f => f.Name)
                                                                            .ToListAsync();
                
               
                return View(tcmSupervisorList);                
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

            TCMSupervisorViewModel model;

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
                     model = new TCMSupervisorViewModel
                     {
                         Clinics = list,
                         IdClinic = clinic.Id,
                         StatusList = _combosHelper.GetComboClientStatus(),
                         UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, user_logged.Clinic.Id),
                         CreatedBy = user_logged.UserName,
                         CreatedOn = DateTime.Now,
                         RaterEducation = string.Empty,
                         RaterFMHCertification = string.Empty
                     };
                     return View(model);
                }
            }

            model = new TCMSupervisorViewModel
            {
                Clinics = _combosHelper.GetComboClinics(),
                IdStatus = 1,
                StatusList = _combosHelper.GetComboClientStatus(),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, 0),
                RaterEducation = string.Empty,
                RaterFMHCertification = string.Empty
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create(TCMSupervisorViewModel tcmSupervisorViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMSupervisorEntity tcmSupervisorEntity = await _context.TCMSupervisors.FirstOrDefaultAsync(s => s.Name == tcmSupervisorViewModel.Name);
                if (tcmSupervisorEntity == null)
                {
                    if (tcmSupervisorViewModel.IdUser == "0")
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

                    tcmSupervisorEntity = await _converterHelper.ToTCMsupervisorEntity(tcmSupervisorViewModel, path, true, user_logged.UserName);
                    _context.Add(tcmSupervisorEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        List<TCMSupervisorEntity> tcmSupervisor = await _context.TCMSupervisors

                                                                                .Include(s => s.Clinic)
                                                                                .Include(f => f.CaseManagerList)
                                                                                .ThenInclude(f => f.TCMClients)
                                                                                .ThenInclude(f => f.Client)

                                                                                .OrderBy(t => t.Name)
                                                                                .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSupervisors", tcmSupervisor) });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM supervisor.");

                    if (User.IsInRole("Admin"))
                    {
                        tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();
                        tcmSupervisorViewModel.IdStatus = 1;
                        tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
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
                        tcmSupervisorViewModel.IdStatus = 1;
                        tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                        tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, user_logged.Clinic.Id);
                    }
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmSupervisorViewModel) });
                }
            }

            //recovery data
            if (User.IsInRole("Admin"))
            {
                tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();
                tcmSupervisorViewModel.IdStatus = 1;
                tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
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
                tcmSupervisorViewModel.IdStatus = 1;
                tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, user_logged.Clinic.Id);
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmSupervisorViewModel) });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMSupervisorEntity tcmSupervisorEntity = await _context.TCMSupervisors.FirstOrDefaultAsync(s => s.Id == id);
            if (tcmSupervisorEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMSupervisors.Remove(tcmSupervisorEntity);
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

            TCMSupervisorEntity tcmSupervisorEntity = await _context.TCMSupervisors.Include(s => s.Clinic).FirstOrDefaultAsync(s => s.Id == id);
            if (tcmSupervisorEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMSupervisorViewModel tcmSupervisorViewModel;

            if (!User.IsInRole("Admin"))
            {
                tcmSupervisorViewModel = _converterHelper.ToTCMsupervisorViewModel(tcmSupervisorEntity, user_logged.Clinic.Id);

                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });

                tcmSupervisorViewModel.Clinics = list;
                tcmSupervisorViewModel.IdClinic = user_logged.Clinic.Id;             
            }
            else
            {
                tcmSupervisorViewModel = _converterHelper.ToTCMsupervisorViewModel(tcmSupervisorEntity, 0);                
            }           

            return View(tcmSupervisorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int id, TCMSupervisorViewModel tcmSupervisorViewModel)
        {
            if (id != tcmSupervisorViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                if (tcmSupervisorViewModel.IdUser == "0")
                {
                    if (User.IsInRole("Admin"))
                    {
                        tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();
                        tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, 0);
                        tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
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
                        tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                    }

                    ModelState.AddModelError(string.Empty, "You must select a linked user");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmSupervisorViewModel) });
                }

                string path = tcmSupervisorViewModel.SignaturePath;
                if (tcmSupervisorViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(tcmSupervisorViewModel.SignatureFile, "Signatures");
                }
                TCMSupervisorEntity tcmSupervisorEntity = await _converterHelper.ToTCMsupervisorEntity(tcmSupervisorViewModel, path, false, user_logged.UserName);
                _context.Update(tcmSupervisorEntity);

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMSupervisorEntity> tcmSupervisor = await _context.TCMSupervisors

                                                                            .Include(s => s.Clinic)
                                                                            .Include(f => f.CaseManagerList)
                                                                            .ThenInclude(f => f.TCMClients)
                                                                            .ThenInclude(f => f.Client)
                                                                            .OrderBy(f => f.Name)
                                                                            .ToListAsync();
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSupervisors", tcmSupervisor) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the TCM supervisor: {tcmSupervisorEntity.Name}");
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
                tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();                
                tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
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
                tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, user_logged.Clinic.Id);
            }
            
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmSupervisorViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> Signatures()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }


            return View(await _context.TCMSupervisors

                                      .Include(c => c.Clinic)
                                      .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderBy(c => c.Name).ToListAsync());
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> EditSignature(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMSupervisorEntity tcmSupervisorEntity = await _context.TCMSupervisors
                                                                    .FirstOrDefaultAsync(c => c.Id == id);

            if (tcmSupervisorEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMSupervisorViewModel tcmSupervisorViewModel = _converterHelper.ToTCMsupervisorViewModel(tcmSupervisorEntity, user_logged.Clinic.Id);

            return View(tcmSupervisorViewModel);
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<JsonResult> SaveTCMSupervisorSignature(string id, string dataUrl)
        {
            string signPath = await _imageHelper.UploadSignatureAsync(dataUrl, "TCMsupervisors");

            TCMSupervisorEntity tcmSupervisor = await _context.TCMSupervisors
                                                              .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(id));
            if (tcmSupervisor != null)
            {
                tcmSupervisor.SignaturePath = signPath;
                _context.Update(tcmSupervisor);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectToUrl = Url.Action("Signatures", "TCMsupervisors") });
        }

    }
}
