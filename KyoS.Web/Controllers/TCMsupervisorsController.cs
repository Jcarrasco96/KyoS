 using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
   
    public class TCMSupervisorsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;

        public IConfiguration Configuration { get; }
        public TCMSupervisorsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper, IConfiguration configuration)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _renderHelper = renderHelper;
            Configuration = configuration;
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
        
        [Authorize(Roles = "Admin, Manager")]
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

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult CreateSupervisionTime(int id = 0)
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

            TCMSupervisionTimeViewModel model = new TCMSupervisionTimeViewModel();

            if (User.IsInRole("TCMSupervisor"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    TCMSupervisorEntity tcmSupervisor = _context.TCMSupervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    List<CaseMannagerEntity> caseManager = _context.CaseManagers
                                                                   .Where(c => c.TCMSupervisor.Id == tcmSupervisor.Id)
                                                                   .ToList();
                    List<SelectListItem> list = new List<SelectListItem>();
                    foreach (var item in caseManager)
                    {
                        list.Add(new SelectListItem
                        {
                            Text = item.Name,
                            Value = $"{item.Id}"
                        });
                    }
                    list.Insert(0, new SelectListItem
                    {
                        Text = "Select a CaseManager",
                        Value = $"{0}"
                    });
                    model = new TCMSupervisionTimeViewModel
                    {
                        CaseManagers = list,
                        DateSupervision = DateTime.Today,
                        Description = string.Empty,
                        StartTime = DateTime.Today,
                        EndTime = DateTime.Today,
                        TCMSupervisor = tcmSupervisor,
                        IdTCMSupervisor = tcmSupervisor.Id,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Today

                    };
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> CreateSupervisionTime(TCMSupervisionTimeViewModel tcmSupervisionTimeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMSupervisorEntity tcmSupervisor = _context.TCMSupervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
            List<CaseMannagerEntity> caseManager = _context.CaseManagers
                                                           .Where(c => c.TCMSupervisor.Id == tcmSupervisor.Id)
                                                           .ToList();
            List<SelectListItem> list = new List<SelectListItem>();

            if (ModelState.IsValid)
            {
                DateTime start = new DateTime(tcmSupervisionTimeViewModel.DateSupervision.Year, tcmSupervisionTimeViewModel.DateSupervision.Month, tcmSupervisionTimeViewModel.DateSupervision.Day, tcmSupervisionTimeViewModel.StartTime.Hour, tcmSupervisionTimeViewModel.StartTime.Minute, tcmSupervisionTimeViewModel.StartTime.Second);
                DateTime end = new DateTime(tcmSupervisionTimeViewModel.DateSupervision.Year, tcmSupervisionTimeViewModel.DateSupervision.Month, tcmSupervisionTimeViewModel.DateSupervision.Day, tcmSupervisionTimeViewModel.EndTime.Hour, tcmSupervisionTimeViewModel.EndTime.Minute, tcmSupervisionTimeViewModel.EndTime.Second);
                tcmSupervisionTimeViewModel.StartTime = start;
                tcmSupervisionTimeViewModel.EndTime = end;
                TCMSupervisionTimeEntity tcmSupervisionTimeEntity = await _context.TCMSupervisionTimes
                                                                                  .FirstOrDefaultAsync(s => s.DateSupervision.Date == tcmSupervisionTimeViewModel.DateSupervision.Date
                                                                                  && ((s.StartTime.TimeOfDay <= tcmSupervisionTimeViewModel.StartTime.TimeOfDay
                                                                                      && s.EndTime.TimeOfDay >= tcmSupervisionTimeViewModel.StartTime.TimeOfDay)
                                                                                    ||(s.StartTime.TimeOfDay <= tcmSupervisionTimeViewModel.EndTime.TimeOfDay
                                                                                      && s.EndTime.TimeOfDay >= tcmSupervisionTimeViewModel.EndTime.TimeOfDay)));
                if (tcmSupervisionTimeEntity == null)
                {
                    tcmSupervisionTimeEntity = await _converterHelper.ToTCMSupervisionTimeEntity(tcmSupervisionTimeViewModel, true, user_logged.UserName);
                    _context.Add(tcmSupervisionTimeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        /*List<TCMSupervisionTimeEntity> tcmSupervisionTimes = await _context.TCMSupervisionTimes
                                                                                           .Include(f => f.CaseManager)
                                                                                           .Include(f => f.TCMSupervisor)
                                                                                           .ToListAsync();
                        
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSupervisionTime", tcmSupervisionTimes) });*/

                        // return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "TCMSupervisionTime1") });
                        return Json(new { isValid = true, html = tcmSupervisionTimeViewModel.IdCaseManager.ToString() });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    
                    foreach (var item in caseManager)
                    {
                        list.Add(new SelectListItem
                        {
                            Text = item.Name,
                            Value = $"{item.Id}"
                        });
                    }
                    list.Insert(0, new SelectListItem
                    {
                        Text = "Select a CaseManager",
                        Value = $"{0}"
                    });
                    tcmSupervisionTimeViewModel.CaseManagers = list;
                    ModelState.AddModelError(string.Empty, $"The selected interval time is busy");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateSupervisionTime", tcmSupervisionTimeViewModel) });
                }
                
            }

            //recovery data
           
            foreach (var item in caseManager)
            {
                list.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = $"{item.Id}"
                });
            }
            list.Insert(0, new SelectListItem
            {
                Text = "Select a CaseManager",
                Value = $"{0}"
            });
            tcmSupervisionTimeViewModel.CaseManagers = list;

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateSupervisionTime", tcmSupervisionTimeViewModel) });
        }

        public async Task<IActionResult> TCMSupervisionTime(int idError = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("Manager"))
            {
                return View(await _context.TCMSupervisionTimes
                                          .Include(f => f.CaseManager)
                                          .Include(f => f.TCMSupervisor)
                                          .ToListAsync());
            }
            if (User.IsInRole("CaseManager"))
            {
                return View(await _context.TCMSupervisionTimes
                                          .Include(f => f.CaseManager)
                                          .Include(f => f.TCMSupervisor)
                                          .Where(n => n.CaseManager.LinkedUser == user_logged.UserName)
                                          .ToListAsync());
            }
            if (User.IsInRole("TCMSupervisor"))
            {
                return View(await _context.TCMSupervisionTimes
                                          .Include(f => f.CaseManager)
                                          .Include(f => f.TCMSupervisor)
                                          .Where(n => n.TCMSupervisor.LinkedUser == user_logged.UserName)
                                          .ToListAsync());
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditSupervisionTime(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMSupervisionTimeEntity tcmSupervisioTimeEntity = await _context.TCMSupervisionTimes
                                                                             .Include(s => s.CaseManager)
                                                                             .Include(s => s.TCMSupervisor)
                                                                             .FirstOrDefaultAsync(s => s.Id == id);
            if (tcmSupervisioTimeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMSupervisionTimeViewModel tcmSupervisionTimeViewModel = new TCMSupervisionTimeViewModel();

            if (User.IsInRole("TCMSupervisor"))
            {
                tcmSupervisionTimeViewModel = _converterHelper.ToTCMSupervisionTimeViewModel(tcmSupervisioTimeEntity, user_logged.Clinic.Id);

                TCMSupervisorEntity tcmSupervisor = _context.TCMSupervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                List<CaseMannagerEntity> caseManager = _context.CaseManagers
                                                               .Where(c => c.TCMSupervisor.Id == tcmSupervisor.Id)
                                                               .ToList();
                List<SelectListItem> list = new List<SelectListItem>();
                foreach (var item in caseManager)
                {
                    list.Add(new SelectListItem
                    {
                        Text = item.Name,
                        Value = $"{item.Id}"
                    });
                }
                list.Insert(0, new SelectListItem
                {
                    Text = "Select a CaseManager",
                    Value = $"{0}"
                });

                tcmSupervisionTimeViewModel.CaseManagers = list;
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View(tcmSupervisionTimeViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditSupervisionTime(int id, TCMSupervisionTimeViewModel tcmSupervisionTimeViewModel)
        {
            if (id != tcmSupervisionTimeViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            TCMSupervisorEntity tcmSupervisor = _context.TCMSupervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
            List<CaseMannagerEntity> caseManager = _context.CaseManagers
                                                           .Where(c => c.TCMSupervisor.Id == tcmSupervisor.Id)
                                                           .ToList();
            List<SelectListItem> list = new List<SelectListItem>();

            if (ModelState.IsValid)
            {
                DateTime start = new DateTime(tcmSupervisionTimeViewModel.DateSupervision.Year, tcmSupervisionTimeViewModel.DateSupervision.Month, tcmSupervisionTimeViewModel.DateSupervision.Day, tcmSupervisionTimeViewModel.StartTime.Hour, tcmSupervisionTimeViewModel.StartTime.Minute, tcmSupervisionTimeViewModel.StartTime.Second);
                DateTime end = new DateTime(tcmSupervisionTimeViewModel.DateSupervision.Year, tcmSupervisionTimeViewModel.DateSupervision.Month, tcmSupervisionTimeViewModel.DateSupervision.Day, tcmSupervisionTimeViewModel.EndTime.Hour, tcmSupervisionTimeViewModel.EndTime.Minute, tcmSupervisionTimeViewModel.EndTime.Second);
                tcmSupervisionTimeViewModel.StartTime = start;
                tcmSupervisionTimeViewModel.EndTime = end;

                TCMSupervisionTimeEntity tcmSupervisionTimeEntity = await _context.TCMSupervisionTimes
                                                                                   .FirstOrDefaultAsync(s => s.DateSupervision.Date == tcmSupervisionTimeViewModel.DateSupervision.Date
                                                                                                        && ((s.StartTime.TimeOfDay <= tcmSupervisionTimeViewModel.StartTime.TimeOfDay
                                                                                                               && s.EndTime.TimeOfDay >= tcmSupervisionTimeViewModel.StartTime.TimeOfDay)
                                                                                                            || (s.StartTime.TimeOfDay <= tcmSupervisionTimeViewModel.EndTime.TimeOfDay
                                                                                                               && s.EndTime.TimeOfDay >= tcmSupervisionTimeViewModel.EndTime.TimeOfDay))
                                                                                                               && s.Id != tcmSupervisionTimeViewModel.Id);
                if (tcmSupervisionTimeEntity == null)
                {
                    tcmSupervisionTimeEntity = await _converterHelper.ToTCMSupervisionTimeEntity(tcmSupervisionTimeViewModel, false, user_logged.UserName);
                    _context.Update(tcmSupervisionTimeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        /*List<TCMSupervisionTimeEntity> tcmSupervisionTimes = await _context.TCMSupervisionTimes
                                                                                           .Include(f => f.CaseManager)
                                                                                           .Include(f => f.TCMSupervisor)
                                                                                           .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSupervisionTime", tcmSupervisionTimes) });*/
                        return Json(new { isValid = true, html = "0" });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {

                    foreach (var item in caseManager)
                    {
                        list.Add(new SelectListItem
                        {
                            Text = item.Name,
                            Value = $"{item.Id}"
                        });
                    }
                    list.Insert(0, new SelectListItem
                    {
                        Text = "Select a CaseManager",
                        Value = $"{0}"
                    });
                    tcmSupervisionTimeViewModel.CaseManagers = list;
                    ModelState.AddModelError(string.Empty, $"The selected interval time is busy");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditSupervisionTime", tcmSupervisionTimeViewModel) });
                }
            }

            //recovery data
            foreach (var item in caseManager)
            {
                list.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = $"{item.Id}"
                });
            }
            list.Insert(0, new SelectListItem
            {
                Text = "Select a CaseManager",
                Value = $"{0}"
            });
            tcmSupervisionTimeViewModel.CaseManagers = list;
            ModelState.AddModelError(string.Empty, $"The selected interval time is busy");
           
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditSupervisionTime", tcmSupervisionTimeViewModel) });
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> DeleteTCMSupervisionTime(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMSupervisionTimeEntity tcmSupervisionTimeEntity = await _context.TCMSupervisionTimes.FirstOrDefaultAsync(s => s.Id == id);
            if (tcmSupervisionTimeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMSupervisionTimes.Remove(tcmSupervisionTimeEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("TCMSupervisionTime", new { idError = 1 });
            }

            return RedirectToAction(nameof(TCMSupervisionTime));
        }

        [Authorize(Roles = "TCMSupervisor, Manager,CaseManager")]
        public IActionResult TCMSupervisionTimeCalendar()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMSupervisionTimeViewModel model = new TCMSupervisionTimeViewModel();

            if (User.IsInRole("TCMSupervisor"))
            {
                model = new TCMSupervisionTimeViewModel
                {
                    IdCaseManager = 0,
                    CaseManagers = _combosHelper.GetComboCaseManagersByTCMSupervisor(user_logged.UserName)
                };
            }
            if (User.IsInRole("Manager"))
            {
                model = new TCMSupervisionTimeViewModel
                {
                    IdCaseManager = 0,
                    CaseManagers = _combosHelper.GetComboCaseManagersActive()
                };
            }
            if (User.IsInRole("CaseManager"))
            {
                List<SelectListItem> list = new List<SelectListItem>();
                CaseMannagerEntity caseManager = _context.CaseManagers

                                               .FirstOrDefault(c => c.LinkedUser == user_logged.UserName);
                
                list.Insert(0, new SelectListItem
                {
                    Text = $"{caseManager.Name} ",
                    Value = $"{caseManager.Id} "
                });
                model = new TCMSupervisionTimeViewModel
                {
                    IdCaseManager = 0,
                    CaseManagers = list
                };
            }

            return View(model);
        }

        [Authorize(Roles = "TCMSupervisor, Manager, CaseManager")]
        private async Task<List<object>> SupervisionByTCMsupervisor(string start, string end, int idTCMSupervisor = 0, int idCaseManager = 0)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<TCMSupervisionTimeEntity> listSupervisions;

            UserEntity user_logged = _context.Users
                                           .Include(u => u.Clinic)
                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

            using (DataContext db = new DataContext(options))
            {
                listSupervisions = await db.TCMSupervisionTimes

                                           .Include(c => c.CaseManager)

                                           .Include(c => c.TCMSupervisor)

                                          // .Where(c => (c.DateCite >= initDate && c.DateCite <= finalDate))
                                           .ToListAsync();


                if (idTCMSupervisor != 0)
                {
                    if (idCaseManager == 0)
                    {
                        listSupervisions = listSupervisions.Where(c => c.TCMSupervisor.Id == idTCMSupervisor)
                                                           .ToList();
                    }
                    else
                    {
                        listSupervisions = listSupervisions.Where(c => c.TCMSupervisor.Id == idTCMSupervisor
                                                                    && c.CaseManager.Id == idCaseManager)
                                                           .ToList();
                    }
                }
                else
                {
                    if (idCaseManager == 0)
                    {
                        listSupervisions = listSupervisions.ToList();
                    }
                    else
                    {
                        listSupervisions = listSupervisions.Where(c => c.CaseManager.Id == idCaseManager)
                                                           .ToList();
                    }
                }
            }
            return listSupervisions
                        .Select(t => new
                        {
                            id = t.Id,
                            title = t.CaseManager.Name.ToString(),
                            start = t.StartTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                            end = t.EndTime.ToString("yyyy-MM-ddTHH:mm:ssK"),
                            // url = Url.Action("EditSupervisionTime", "TCMsupervisors", new { id = t.Id }),
                            backgroundColor = (t.Present == true) ? "#dff0d8" : "#d9edf7",
                            textColor = (t.Present == true) ? "#417c49" : "#487c93",
                            borderColor = (t.Present == true) ? "#417c49" : "#487c93"
                        })
                        .Distinct()
                        .ToList<object>();

        }

        public async Task<IActionResult> Events(string start, string end, int idCaseManager = 0)
        {
            DateTime initDate = Convert.ToDateTime(start);
            DateTime finalDate = Convert.ToDateTime(end);
            UserEntity user_logged = _context.Users
                                          .Include(u => u.Clinic)
                                          .FirstOrDefault(u => u.UserName == User.Identity.Name);
            TCMSupervisorEntity tcmSupervisor;
            if (idCaseManager != 0)
            {
                tcmSupervisor  = _context.CaseManagers.Include(n => n.TCMSupervisor).FirstOrDefault(n => n.Id == idCaseManager).TCMSupervisor;
            }
            else
            {
                if (User.IsInRole("Manager"))
                {
                    tcmSupervisor = new TCMSupervisorEntity();
                }
                else
                {
                    tcmSupervisor = _context.TCMSupervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                }
                
            }

            Task<List<object>> notesTask = SupervisionByTCMsupervisor(start, end, tcmSupervisor.Id, idCaseManager);

            await Task.WhenAll(notesTask);

            var notes = await notesTask;

            List<object> events = new List<object>();
            events.AddRange(notes);

            return new JsonResult(events);
        }
    }
}
