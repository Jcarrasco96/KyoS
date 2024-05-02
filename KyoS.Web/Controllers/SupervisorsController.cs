using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.AspNetCore.Http;
using KyoS.Common.Helpers;

namespace KyoS.Web.Controllers
{
    
    public class SupervisorsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;

        public SupervisorsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _renderHelper = renderHelper;
        }

        [Authorize(Roles = "Admin, Manager, Supervisor")]
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
                if (User.IsInRole("Manager"))
                {
                    UserEntity user_logged = _context.Users

                                                 .Include(u => u.Clinic)
                                                 .ThenInclude(c => c.Setting)

                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }

                    return View(await _context.Supervisors
                                              .Include(f => f.Clinic)
                                              .Where(s => s.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name).ToListAsync());
                }
                if (User.IsInRole("Supervisor"))
                {
                    UserEntity user_logged = _context.Users

                                                 .Include(u => u.Clinic)
                                                 .ThenInclude(c => c.Setting)

                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }

                    return View(await _context.Supervisors
                                              .Include(f => f.Clinic)
                                              .Where(s => s.Clinic.Id == user_logged.Clinic.Id
                                                       && s.LinkedUser == user_logged.UserName)
                                              .OrderBy(f => f.Name).ToListAsync());
                }
            }

            return RedirectToAction("NotAuthorized", "Account");
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

        [Authorize(Roles = "Admin, Manager")]
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

        [Authorize(Roles = "Admin, Manager")]
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

        [Authorize(Roles = "Admin, Manager")]
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

        [Authorize(Roles = "Manager")]
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


            return View(await _context.Supervisors
                                          .Include(f => f.Clinic)
                                          .Where(s => s.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderBy(f => f.Name).ToListAsync());
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditSignature(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SupervisorEntity supervisorEntity = await _context.Supervisors
                                                              .Include(n => n.Clinic)
                                                              .FirstOrDefaultAsync(c => c.Id == id);

            if (supervisorEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SupervisorViewModel supervisorViewModel = _converterHelper.ToSupervisorViewModel(supervisorEntity, supervisorEntity.Clinic.Id);

            return View(supervisorViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SaveSupervisorSignature(string id, string dataUrl)
        {
            string signPath = await _imageHelper.UploadSignatureAsync(dataUrl, "Supervisors");

            SupervisorEntity supervisor = await _context.Supervisors
                                                        .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(id));
            if (supervisor != null)
            {
                supervisor.SignaturePath = signPath;
                _context.Update(supervisor);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectToUrl = Url.Action("Signatures", "Supervisors") });
        }

        [Authorize(Roles = "Admin, Manager")]
        public IActionResult CreateModal(int id = 0)
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
                        UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Supervisor, user_logged.Clinic.Id),

                
                        StatusList = _combosHelper.GetComboClientStatus(),
                        Money = 0,
                        RaterEducation = string.Empty,
                        RaterFMHCertification = string.Empty,
                        IdGender = 0,
                        GenderList = _combosHelper.GetComboGender(),
                        IdAccountType = 0,
                        AccountTypeList = _combosHelper.GetComboAccountType(),
                        IdPaymentMethod = 0,
                        PaymentMethodList = _combosHelper.GetComboPaymentMethod(),
                        Name = "-",
                        DateOfBirth = DateTime.Today,
                        HiringDate = DateTime.Today
                    };
                    return View(model);
                }
            }

            model = new SupervisorViewModel
            {
                Clinics = _combosHelper.GetComboClinics(),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Supervisor, 0),
                StatusList = _combosHelper.GetComboClientStatus(),
                Money = 0,
                RaterEducation = string.Empty,
                RaterFMHCertification = string.Empty,
                IdGender = 0,
                GenderList = _combosHelper.GetComboGender(),
                IdAccountType = 0,
                AccountTypeList = _combosHelper.GetComboAccountType(),
                IdPaymentMethod = 0,
                PaymentMethodList = _combosHelper.GetComboPaymentMethod(),
                Name = "-",
                DateOfBirth = DateTime.Today,
                HiringDate = DateTime.Today
            };
            return View(model);
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(SupervisorViewModel supervisorViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                SupervisorEntity supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Name == supervisorViewModel.Name);
                if (supervisor == null)
                {
                    if (supervisorViewModel.IdUser == "0")
                    {
                        ModelState.AddModelError(string.Empty, "You must select a linked user");
                        
                        ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                        List<SelectListItem> list = new List<SelectListItem>();
                        list.Insert(0, new SelectListItem
                        {
                            Text = clinic.Name,
                            Value = $"{clinic.Id}"
                        });

                        supervisorViewModel.Clinics = list;
                        supervisorViewModel.IdClinic = clinic.Id;
                        supervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Supervisor, user_logged.Clinic.Id);

                        supervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                        supervisorViewModel.GenderList = _combosHelper.GetComboGender();
                        supervisorViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
                        supervisorViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
                        supervisorViewModel.Clinics = _combosHelper.GetComboClinics();

                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", supervisorViewModel) });
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

                        List<SupervisorEntity> supervisors_List = await _context.Supervisors
                                                                                  .Include(n => n.Clinic)
                                                                                  .Where(s => s.Clinic.Id == user_logged.Clinic.Id)
                                                                                  .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSupervisors", supervisors_List) });
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
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", supervisorViewModel) });
                }
            }
            else
            {
                ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = clinic.Name,
                    Value = $"{clinic.Id}"
                });

                supervisorViewModel.Clinics = list;
                supervisorViewModel.IdClinic = clinic.Id;
                supervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Supervisor, user_logged.Clinic.Id);

                supervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                supervisorViewModel.GenderList = _combosHelper.GetComboGender();
                supervisorViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
                supervisorViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
                supervisorViewModel.Clinics = _combosHelper.GetComboClinics();

            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", supervisorViewModel) });
        }

        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> EditModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SupervisorEntity supervisorEntity = await _context.Supervisors
                                                              .Include(s => s.Clinic)
                                                              .Include(s => s.SupervisorCertifications)
                                                              .ThenInclude(f => f.Course)
                                                              .FirstOrDefaultAsync(s => s.Id == id);

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

                    List<SupervisorCertificationEntity> CertificationList = new List<SupervisorCertificationEntity>();
                    SupervisorCertificationEntity Certification = new SupervisorCertificationEntity();
                    List<CourseEntity> coursList = _context.Courses.Where(n => n.Role == UserType.Supervisor).ToList();

                    foreach (var item in coursList)
                    {
                        if (supervisorEntity.SupervisorCertifications.Where(n => n.Course.Id == item.Id).Count() > 0)
                        {
                            foreach (var value in supervisorEntity.SupervisorCertifications.Where(n => n.Course.Id == item.Id).ToList().OrderBy(c => c.ExpirationDate))
                            {
                                Certification.Name = item.Name;
                                Certification.CertificationNumber = value.CertificationNumber;
                                Certification.CertificateDate = value.CertificateDate;
                                Certification.ExpirationDate = value.ExpirationDate;
                                Certification.Id = value.Id;
                                CertificationList.Add(Certification);
                                Certification = new SupervisorCertificationEntity();
                            }
                        }
                        else
                        {
                            Certification.Name = item.Name;
                            Certification.CertificationNumber = "-";
                            Certification.CertificateDate = DateTime.Today;
                            Certification.ExpirationDate = DateTime.Today;
                            Certification.Id = 0;
                            CertificationList.Add(Certification);
                            Certification = new SupervisorCertificationEntity();
                        }

                    }

                    supervisorViewModel.SupervisorCertificationIdealList = CertificationList;

                }
            }
            else
                supervisorViewModel = _converterHelper.ToSupervisorViewModel(supervisorEntity, 0);

            return View(supervisorViewModel);
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(int id, SupervisorViewModel supervisorViewModel)
        {
            if (id != supervisorViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                            .Include(u => u.Clinic)
                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

            ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == supervisorViewModel.IdClinic);
            List<SelectListItem> list = new List<SelectListItem>();

            if (ModelState.IsValid)
            {
                if (supervisorViewModel.IdUser == "0")
                {
                    ModelState.AddModelError(string.Empty, "You must select a linked user");

                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });

                    supervisorViewModel.Clinics = list;
                    supervisorViewModel.IdClinic = clinic.Id;
                    supervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Supervisor, supervisorViewModel.IdClinic);

                    supervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Supervisor, user_logged.Clinic.Id);
                    supervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                    supervisorViewModel.GenderList = _combosHelper.GetComboGender();
                    supervisorViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
                    supervisorViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
                    supervisorViewModel.Clinics = _combosHelper.GetComboClinics();

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", supervisorViewModel) });
                }

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

                    List<SupervisorEntity> supervisors_List = await _context.Supervisors
                                                                                 .Include(n => n.Clinic)
                                                                                 .Where(s => s.Clinic.Id == user_logged.Clinic.Id)
                                                                                 .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSupervisors", supervisors_List) });
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

            list.Insert(0, new SelectListItem
            {
                Text = clinic.Name,
                Value = $"{clinic.Id}"
            });

            supervisorViewModel.Clinics = list;
            supervisorViewModel.IdClinic = clinic.Id;
            supervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Supervisor, supervisorViewModel.IdClinic);

            supervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
            supervisorViewModel.GenderList = _combosHelper.GetComboGender();
            supervisorViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
            supervisorViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
            supervisorViewModel.Clinics = _combosHelper.GetComboClinics();


            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", supervisorViewModel) });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public async Task<IActionResult> SupervisorNotes(int idError = 0, int all = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Manager"))
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                if (all == 0)
                {
                    return View(await _context.MeetingNotes
                                              .Include(f => f.Supervisor)
                                              .Include(f => f.FacilitatorList)
                                              .ThenInclude(f => f.Facilitator)
                                              .Where(s => s.Supervisor.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Date).ToListAsync());
                }
                else
                {
                    return View(await _context.MeetingNotes
                                              .Include(f => f.Supervisor)
                                              .Include(f => f.FacilitatorList)
                                              .ThenInclude(f => f.Facilitator)
                                              .Where(s => s.Supervisor.Clinic.Id == user_logged.Clinic.Id
                                                       && s.Status != NoteStatus.Approved)
                                              .OrderBy(f => f.Date).ToListAsync());
                }
                
            }
            if ( User.IsInRole("Supervisor") )
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                if (all == 0)
                {
                    return View(await _context.MeetingNotes
                                              .Include(f => f.Supervisor)
                                              .Include(f => f.FacilitatorList)
                                              .ThenInclude(f => f.Facilitator)
                                              .Where(s => s.Supervisor.LinkedUser == user_logged.UserName)
                                              .OrderBy(f => f.Date).ToListAsync());
                }
                else
                {
                    return View(await _context.MeetingNotes
                                              .Include(f => f.Supervisor)
                                              .Include(f => f.FacilitatorList)
                                              .ThenInclude(f => f.Facilitator)
                                              .Where(s => s.Supervisor.LinkedUser == user_logged.UserName
                                                       && s.Status != NoteStatus.Approved)
                                              .OrderBy(f => f.Date).ToListAsync());
                }
                
            }
            if (User.IsInRole("Facilitator"))
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                if (all == 0)
                {
                    return View(await _context.MeetingNotes
                                              .Include(f => f.Supervisor)
                                              .Include(f => f.FacilitatorList)
                                              .ThenInclude(f => f.Facilitator)
                                              .Where(s => s.FacilitatorList.Where(n => n.Facilitator.LinkedUser == user_logged.UserName).Count() > 0)
                                              .OrderBy(f => f.Date)
                                              .ToListAsync());
                }
                else
                {
                    return View(await _context.MeetingNotes
                                              .Include(f => f.Supervisor)
                                              .Include(f => f.FacilitatorList)
                                              .ThenInclude(f => f.Facilitator)
                                              .Where(s => s.FacilitatorList.Where(n => n.Facilitator.LinkedUser == user_logged.UserName
                                                                                    && n.Sign == false).Count() > 0)
                                              .OrderBy(f => f.Date)
                                              .ToListAsync());
                }
                
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> CreateNote(int id = 0, int error = 0)
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

            MeetingNotesViewModel model;
            MultiSelectList facilitator_list;
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity>();

            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic != null)
            {
                model = new MeetingNotesViewModel
                {
                   Id = 0,
                   Date = DateTime.Today,
                   IdSupervisor = _context.Supervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName).Id,
                   FacilitatorList = new List<MeetingNotes_Facilitator>()
                   
                };

                facilitators = await _context.Facilitators
                                             .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                       && c.Status == Common.Enums.StatusType.Open))
                                             .OrderBy(c => c.Name).ToListAsync();

                facilitator_list = new MultiSelectList(facilitators, "Id", "Name", facilitators);
                ViewData["facilitators"] = facilitator_list;
                
                return View(model);
            }

            return View(null);
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNote(MeetingNotesViewModel model, IFormCollection form)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                MeetingNoteEntity supervisorNotes = new MeetingNoteEntity();
                List<MeetingNotes_Facilitator> list = new List<MeetingNotes_Facilitator>();
                if (!string.IsNullOrEmpty(form["facilitators"]))
                {
                    string[] facilitators = form["facilitators"].ToString().Split(',');
                    FacilitatorEntity facilitator;
                    
                    foreach (string value in facilitators)
                    {
                        facilitator = await _context.Facilitators
                                                    .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(value));
                        MeetingNotes_Facilitator SupervisorNotes_Facilitators = new MeetingNotes_Facilitator();
                        if (facilitator != null)
                        {
                            SupervisorNotes_Facilitators.Facilitator = facilitator;
                            SupervisorNotes_Facilitators.Intervention = string.Empty;
                            SupervisorNotes_Facilitators.Sign = false;
                        }
                        list.Add(SupervisorNotes_Facilitators);
                    }
                }
                model.FacilitatorList = list;
                SupervisorEntity supervisor = _context.Supervisors.FirstOrDefault(n => n.Id == model.IdSupervisor);
                model.Supervisor = supervisor;
                supervisorNotes = _converterHelper.ToMeetingNoteEntity(model, true);
                _context.Add(supervisorNotes);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("CreateNote", new { id = 1 });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the Note");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            MultiSelectList facilitator_list = new MultiSelectList(await _context.Facilitators.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["facilitators"] = facilitator_list;
            return View(model);
        }


        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> EditNote(int id = 0)
        {
            MeetingNoteEntity supervisorNote = _context.MeetingNotes
                                                       .Include(n => n.FacilitatorList)
                                                       .ThenInclude(n => n.Facilitator)
                                                       .AsSplitQuery()
                                                       .FirstOrDefault(n => n.Id == id);
            if (supervisorNote != null)
            {
                MeetingNotesViewModel model = _converterHelper.ToMeetingNoteViewModel(supervisorNote);
                MultiSelectList facilitator_list;
                List<FacilitatorEntity> facilitators = new List<FacilitatorEntity>();

                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);

                facilitators = await _context.Facilitators
                                             .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                       && c.Status == Common.Enums.StatusType.Open))
                                             .OrderBy(c => c.Name).ToListAsync();

                facilitator_list = new MultiSelectList(facilitators, "Id", "Name", model.FacilitatorList.Select(c => c.Facilitator.Id));
                ViewData["facilitators"] = facilitator_list;

                return View(model);

            }

            return View(null);
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNote(MeetingNotesViewModel model, IFormCollection form)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                //delete SuperisorNotes_Facilitators
                List<MeetingNotes_Facilitator> listFacilitators = _context.MeetingNotes_Facilitators
                                                                          .Include(n => n.Facilitator)
                                                                          .Where(n => n.MeetingNoteEntity.Id == model.Id)
                                                                          .ToList();
                //_context.SupervisorNotes_Facilitators.RemoveRange(listFacilitators);

                MeetingNoteEntity supervisorNotes = new MeetingNoteEntity();
                List<MeetingNotes_Facilitator> list = new List<MeetingNotes_Facilitator>();
                if (!string.IsNullOrEmpty(form["facilitators"]))
                {
                    string[] facilitators = form["facilitators"].ToString().Split(',');
                    FacilitatorEntity facilitator;

                    foreach (string value in facilitators)
                    {
                        facilitator = await _context.Facilitators
                                                    .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(value));
                        MeetingNotes_Facilitator SupervisorNotes_Facilitators = new MeetingNotes_Facilitator();
                        if (facilitator != null && listFacilitators.Where(n => n.Facilitator.Id == facilitator.Id).Count() == 0)
                        {
                            SupervisorNotes_Facilitators.Facilitator = facilitator;
                            SupervisorNotes_Facilitators.Intervention = string.Empty;
                            SupervisorNotes_Facilitators.Sign = false;
                            list.Add(SupervisorNotes_Facilitators);
                        }
                        
                    }
                }
                model.FacilitatorList = list;
                SupervisorEntity supervisor = _context.Supervisors.FirstOrDefault(n => n.Id == model.IdSupervisor);
                model.Supervisor = supervisor;
                supervisorNotes = _converterHelper.ToMeetingNoteEntity(model, false);
                _context.Update(supervisorNotes);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("SupervisorNotes");
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the Note");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            MultiSelectList facilitator_list = new MultiSelectList(await _context.Facilitators.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["facilitators"] = facilitator_list;
            return View(model);
        }

        [Authorize(Roles = "Facilitator")]
        public IActionResult EditNoteFacilitator(int id = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            MeetingNotes_Facilitator entity = _context.MeetingNotes_Facilitators
                                                      .Include(n => n.MeetingNoteEntity)
                                                      .Include(n => n.Facilitator)
                                                      .FirstOrDefault(n => n.Id == id
                                                                        && n.Facilitator.LinkedUser == user_logged.UserName);
            if (entity != null)
            {
                
                MeetingNotesFacilitatorModel model = _converterHelper.ToMeetingNoteFacilitatorViewModel(entity);

                return View(model);

            }

            return View(null);
        }

        [HttpPost]
        [Authorize(Roles = "Facilitator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNoteFacilitator(MeetingNotesFacilitatorModel model, IFormCollection form)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                MeetingNotes_Facilitator noteFacilitator;
                noteFacilitator = _converterHelper.ToMeetingNoteFacilitatorEntity(model, false);
                _context.Update(noteFacilitator);
                
                
                try
                {
                    await _context.SaveChangesAsync();
                    MeetingNoteEntity supervisorNote = _context.MeetingNotes
                                                               .Include(n => n.FacilitatorList)
                                                               .FirstOrDefault(n => n.Id == model.IdSupervisorNote);
                    if (supervisorNote.FacilitatorList.Where(n => n.Sign == false).Count() > 0)
                    {
                        supervisorNote.Status = NoteStatus.Pending;
                    }
                    else
                    {
                        supervisorNote.Status = NoteStatus.Approved;
                    }

                    _context.Update(supervisorNote);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("SupervisorNotes");
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the Note");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public async Task<IActionResult> EditNoteReadOnly(int id = 0)
        {
            MeetingNoteEntity supervisorNote = _context.MeetingNotes
                                                          .Include(n => n.FacilitatorList)
                                                          .ThenInclude(n => n.Facilitator)
                                                          .FirstOrDefault(n => n.Id == id);
            if (supervisorNote != null)
            {
                MeetingNotesViewModel model = _converterHelper.ToMeetingNoteViewModel(supervisorNote);
                MultiSelectList facilitator_list;
                List<FacilitatorEntity> facilitators = new List<FacilitatorEntity>();

                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);

                facilitators = await _context.Facilitators
                                                .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                          && c.Status == Common.Enums.StatusType.Open))
                                                .OrderBy(c => c.Name).ToListAsync();

                facilitator_list = new MultiSelectList(facilitators, "Id", "Name", model.FacilitatorList.Select(c => c.Facilitator.Id));
                ViewData["facilitators"] = facilitator_list;

                return View(model);

            }

            return View(null);
        }

        [Authorize(Roles = "Manager")]
        public IActionResult CreateSupervisorCertification(int id = 0)
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

            SupervisorCertificationViewModel model = new SupervisorCertificationViewModel();

            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    model = new SupervisorCertificationViewModel
                    {
                        CertificateDate = DateTime.Today,
                        ExpirationDate = DateTime.Today,
                        CertificationNumber = string.Empty,
                        Name = user_logged.Clinic.Name,
                        IdCourse = 0,
                        IdSupervisor = 0,
                        Supervisors = _combosHelper.GetComboSupervisorByClinic(user_logged.Clinic.Id, true),
                        Courses = _combosHelper.GetComboCourseByRole(UserType.Supervisor)
                    };
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSupervisorCertification(SupervisorCertificationViewModel CertificationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                SupervisorCertificationEntity Certification = new SupervisorCertificationEntity();
                Certification = _converterHelper.ToSupervisorCertificationEntity(CertificationViewModel, true, user_logged.UserName);
                _context.Add(Certification);
                try
                {
                    await _context.SaveChangesAsync();

                    List<SupervisorCertificationEntity> Supervisor_List = await _context.SupervisorCertifications
                                                                                        .Include(n => n.Course)
                                                                                        .Include(n => n.Supervisor)
                                                                                        .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSupervisorCertifications", Supervisor_List) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Supervisor: {Certification.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }

            }

            CertificationViewModel.Courses = _combosHelper.GetComboCourseByRole(UserType.Documents_Assistant);
            CertificationViewModel.Supervisors = _combosHelper.GetComboSupervisorByClinic(user_logged.Clinic.Id, true);


            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateSupervisorCertification", CertificationViewModel) });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditSupervisorCertification(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            SupervisorCertificationEntity Certification = await _context.SupervisorCertifications
                                                                        .Include(f => f.Course)
                                                                        .Include(f => f.Supervisor)
                                                                        .FirstOrDefaultAsync(f => f.Id == id);
            if (Certification == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SupervisorCertificationViewModel CertificationViewModel;
            if (User.IsInRole("Manager"))
            {
                CertificationViewModel = _converterHelper.ToSupervisorCertificationViewModel(Certification, user_logged.Clinic.Id);
            }
            else
                CertificationViewModel = _converterHelper.ToSupervisorCertificationViewModel(Certification, user_logged.Clinic.Id);

            return View(CertificationViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSupervisorCertification(int id, SupervisorCertificationViewModel CertificationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {

                SupervisorCertificationEntity CertificationEntity = _converterHelper.ToSupervisorCertificationEntity(CertificationViewModel, false, user_logged.UserName);
                _context.Update(CertificationEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    List<SupervisorCertificationEntity> Certification_List = await _context.SupervisorCertifications
                                                                                           .Include(n => n.Course)
                                                                                           .Include(n => n.Supervisor)
                                                                                           .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSupervisorCertifications", Certification_List) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Supervisor: {CertificationEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            CertificationViewModel.Supervisors = _combosHelper.GetComboSupervisorByClinic(user_logged.Clinic.Id, true);
            CertificationViewModel.Courses = _combosHelper.GetComboCourseByRole(UserType.Supervisor);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditSupervisorCertification", CertificationViewModel) });
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public async Task<IActionResult> SupervisorCertifications(int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (User.IsInRole("Manager"))
            {
                if (idError == 1) //Imposible to delete
                {
                    ViewBag.Delete = "N";
                }

                return View(await _context.SupervisorCertifications
                                          .Include(f => f.Course)
                                          .Include(f => f.Supervisor)
                                          .AsSplitQuery()
                                          .Where(f => f.Supervisor.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderBy(f => f.Supervisor.Name)
                                          .ToListAsync());

            }
            else
            {
                if (User.IsInRole("Supervisor"))
                {
                    SupervisorEntity supervisor = _context.Supervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                    if (idError == 1) //Imposible to delete
                    {
                        ViewBag.Delete = "N";
                    }

                    return View(await _context.SupervisorCertifications
                                              .Include(f => f.Course)
                                              .Include(f => f.Supervisor)
                                              .AsSplitQuery()
                                              .Where(f => f.Supervisor.Clinic.Id == user_logged.Clinic.Id
                                                       && f.Supervisor.Id == supervisor.Id)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteCertification(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SupervisorCertificationEntity CertificationEntity = await _context.SupervisorCertifications.FirstOrDefaultAsync(t => t.Id == id);
            if (CertificationEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.SupervisorCertifications.Remove(CertificationEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(SupervisorCertifications));
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public IActionResult AuditCertification()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditCertification> auditCertification_List = new List<AuditCertification>();
            AuditCertification auditCertification = new AuditCertification();
            List<SupervisorEntity> supervisor_List = new List<SupervisorEntity>();
            List<CourseEntity> course_List = new List<CourseEntity>();

            if (User.IsInRole("Manager"))
            {
                supervisor_List = _context.Supervisors
                                          .Include(m => m.SupervisorCertifications)
                                          .ToList();

                course_List = _context.Courses
                                      .Include(m => m.SupervisorCertifications)
                                      .Where(n => n.Role == UserType.Supervisor
                                               && n.Active == true)
                                      .ToList();
            }
            else
            {
                if (User.IsInRole("Supervisor"))
                {
                    SupervisorEntity supervisor = _context.Supervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    supervisor_List = _context.Supervisors
                                              .Include(m => m.SupervisorCertifications)
                                              .Where(n => n.Id == supervisor.Id)
                                              .ToList();

                    course_List = _context.Courses
                                          .Include(m => m.SupervisorCertifications)
                                          .Where(n => n.Role == UserType.Supervisor
                                                   && n.Active == true)
                                          .ToList();
                }
            }

            foreach (var item in supervisor_List)
            {
                foreach (var course in course_List)
                {
                    if (item.SupervisorCertifications.Where(n => n.Course.Id == course.Id).Count() > 0)
                    {
                        if (item.SupervisorCertifications.Where(n => n.Course.Id == course.Id && n.ExpirationDate > DateTime.Today).Count() > 0)
                        {
                            if (item.SupervisorCertifications.Where(n => n.Course.Id == course.Id && n.ExpirationDate > DateTime.Today).Max(d => d.ExpirationDate).AddDays(-30) < DateTime.Today)
                            {
                                auditCertification.TCMName = item.Name;
                                auditCertification.CourseName = course.Name;
                                auditCertification.Description = "Expired soon";
                                auditCertification.ExpirationDate = item.SupervisorCertifications.FirstOrDefault(n => n.Course.Id == course.Id && n.ExpirationDate.AddDays(-30) < DateTime.Today).ExpirationDate.ToShortDateString();
                                auditCertification_List.Add(auditCertification);
                                auditCertification = new AuditCertification();
                            }
                        }
                        else
                        {
                            auditCertification.TCMName = item.Name;
                            auditCertification.CourseName = course.Name;
                            auditCertification.Description = "Expired";
                            auditCertification.ExpirationDate = item.SupervisorCertifications.Where(n => n.Course.Id == course.Id).Max(m => m.ExpirationDate).ToShortDateString();
                            auditCertification_List.Add(auditCertification);
                            auditCertification = new AuditCertification();

                        }
                    }
                    else
                    {
                        auditCertification.TCMName = item.Name;
                        auditCertification.CourseName = course.Name;
                        auditCertification.Description = "Not Exists";
                        auditCertification.ExpirationDate = "-";
                        auditCertification_List.Add(auditCertification);
                        auditCertification = new AuditCertification();
                    }

                }

            }


            return View(auditCertification_List);
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> EditModalReadOnly(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SupervisorEntity entity = await _context.Supervisors
                                                    .Include(f => f.Clinic)
                                                    .Include(f => f.SupervisorCertifications)
                                                    .ThenInclude(f => f.Course)
                                                    .FirstOrDefaultAsync(f => f.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SupervisorViewModel model;
            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                model = _converterHelper.ToSupervisorViewModel(entity, user_logged.Clinic.Id);
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    model.Clinics = list;

                    List<SupervisorCertificationEntity> CertificationList = new List<SupervisorCertificationEntity>();
                    SupervisorCertificationEntity Certification = new SupervisorCertificationEntity();
                    List<CourseEntity> coursList = _context.Courses.Where(n => n.Role == UserType.Supervisor).ToList();

                    foreach (var item in coursList)
                    {
                        if (entity.SupervisorCertifications.Where(n => n.Course.Id == item.Id).Count() > 0)
                        {
                            foreach (var value in entity.SupervisorCertifications.Where(n => n.Course.Id == item.Id).ToList().OrderBy(c => c.ExpirationDate))
                            {
                                Certification.Name = item.Name;
                                Certification.CertificationNumber = value.CertificationNumber;
                                Certification.CertificateDate = value.CertificateDate;
                                Certification.ExpirationDate = value.ExpirationDate;
                                Certification.Id = value.Id;
                                CertificationList.Add(Certification);
                                Certification = new SupervisorCertificationEntity();
                            }
                        }
                        else
                        {
                            Certification.Name = item.Name;
                            Certification.CertificationNumber = "-";
                            Certification.CertificateDate = DateTime.Today;
                            Certification.ExpirationDate = DateTime.Today;
                            Certification.Id = 0;
                            CertificationList.Add(Certification);
                            Certification = new SupervisorCertificationEntity();
                        }

                    }

                    model.SupervisorCertificationIdealList = CertificationList;
                }

            }
            else
                return RedirectToAction("Home/Error404");

            return View(model);
        }


    }
}