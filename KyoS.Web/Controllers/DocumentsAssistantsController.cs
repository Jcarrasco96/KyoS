using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Common.Helpers;
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
    public class DocumentsAssistantsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;

        public DocumentsAssistantsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper)
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
                return View(await _context.Supervisors.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());
            else
            {
                UserEntity user_logged = _context.Users

                                                 .Include(u => u.Clinic)
                                                 .ThenInclude(c => c.Setting)

                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }               
                         
                return View(await _context.DocumentsAssistant
                                          .Include(f => f.Clinic)
                                          .Where(s => s.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderBy(f => f.Name).ToListAsync());             
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

            DocumentsAssistantViewModel model;

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
                    model = new DocumentsAssistantViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Documents_Assistant, user_logged.Clinic.Id)
                    };                    
                    return View(model);
                }
            }
            
            model = new DocumentsAssistantViewModel
            {
                 Clinics = _combosHelper.GetComboClinics(),
                 UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Documents_Assistant, 0)
            };
            return View(model);                        
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentsAssistantViewModel documentAssistantViewModel)
        {
            if (ModelState.IsValid)
            {
                DocumentsAssistantEntity supervisor = await _context.DocumentsAssistant.FirstOrDefaultAsync(s => s.Name == documentAssistantViewModel.Name);
                if (supervisor == null)
                {
                    if (documentAssistantViewModel.IdUser == "0")
                    {
                        ModelState.AddModelError(string.Empty, "You must select a linked user");
                        return View(documentAssistantViewModel);
                    }

                    string path = string.Empty;
                    if (documentAssistantViewModel.SignatureFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(documentAssistantViewModel.SignatureFile, "Signatures");
                    }

                    DocumentsAssistantEntity documentsEntity = await _converterHelper.ToDocumentsAssistantEntity(documentAssistantViewModel, path, true);

                    _context.Add(documentsEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the documents assistant: {documentsEntity.Name}");
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
            return View(documentAssistantViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentsAssistantEntity documentsAssistantEntity = await _context.DocumentsAssistant.FirstOrDefaultAsync(s => s.Id == id);
            if (documentsAssistantEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }        

            try
            {
                _context.DocumentsAssistant.Remove(documentsAssistantEntity);
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

            DocumentsAssistantEntity dosumentAssistantEntity = await _context.DocumentsAssistant.Include(s => s.Clinic).FirstOrDefaultAsync(s => s.Id == id);
            if (dosumentAssistantEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentsAssistantViewModel documentsAssistantViewModel;

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                documentsAssistantViewModel = _converterHelper.ToDocumentsAssistantViewModel(dosumentAssistantEntity, user_logged.Clinic.Id);
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    documentsAssistantViewModel.Clinics = list;
                }
            }            
            else
                documentsAssistantViewModel = _converterHelper.ToDocumentsAssistantViewModel(dosumentAssistantEntity, 0);

            return View(documentsAssistantViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DocumentsAssistantViewModel documentsAssistantViewModel)
        {
            if (id != documentsAssistantViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                string path = documentsAssistantViewModel.SignaturePath;
                if (documentsAssistantViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(documentsAssistantViewModel.SignatureFile, "Signatures");
                }
                DocumentsAssistantEntity documentsAssitantEntity = await _converterHelper.ToDocumentsAssistantEntity(documentsAssistantViewModel, path, false);
                _context.Update(documentsAssitantEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the documents assistant: {documentsAssitantEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(documentsAssistantViewModel);
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


            return View(await _context.DocumentsAssistant
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

            DocumentsAssistantEntity assistantEntity = await _context.DocumentsAssistant
                                                                     .Include(n => n.Clinic)
                                                                     .FirstOrDefaultAsync(c => c.Id == id);

            if (assistantEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentsAssistantViewModel documentViewModel = _converterHelper.ToDocumentsAssistantViewModel(assistantEntity, assistantEntity.Clinic.Id);

            return View(documentViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SaveDocumentAssistantSignature(string id, string dataUrl)
        {
            string signPath = await _imageHelper.UploadSignatureAsync(dataUrl, "Assistants");

            DocumentsAssistantEntity assistant = await _context.DocumentsAssistant
                                                               .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(id));
            if (assistant != null)
            {
                assistant.SignaturePath = signPath;
                _context.Update(assistant);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectToUrl = Url.Action("Signatures", "DocumentsAssistants") });
        }

        [Authorize(Roles = "Manager")]
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

            DocumentsAssistantViewModel model;

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
                    model = new DocumentsAssistantViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Documents_Assistant, user_logged.Clinic.Id),

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

            model = new DocumentsAssistantViewModel
            {
                Clinics = _combosHelper.GetComboClinics(),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Documents_Assistant, 0),
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

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(DocumentsAssistantViewModel documentAssistantViewModel)
        {
            UserEntity user_logged = _context.Users
                                               .Include(u => u.Clinic)
                                               .FirstOrDefault(u => u.UserName == User.Identity.Name);

            ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
            List<SelectListItem> list = new List<SelectListItem>();

            if (ModelState.IsValid)
            {
               

                DocumentsAssistantEntity supervisor = await _context.DocumentsAssistant.FirstOrDefaultAsync(s => s.Name == documentAssistantViewModel.Name);
                if (supervisor == null)
                {
                    if (documentAssistantViewModel.IdUser == "0")
                    {
                        ModelState.AddModelError(string.Empty, "You must select a linked user");

                        
                        list.Insert(0, new SelectListItem
                        {
                            Text = clinic.Name,
                            Value = $"{clinic.Id}"
                        });

                        documentAssistantViewModel.Clinics = list;
                        documentAssistantViewModel.IdClinic = clinic.Id;
                        documentAssistantViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Documents_Assistant, user_logged.Clinic.Id);
                        documentAssistantViewModel.StatusList = _combosHelper.GetComboClientStatus();
                        documentAssistantViewModel.GenderList = _combosHelper.GetComboGender();
                        documentAssistantViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
                        documentAssistantViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
                        documentAssistantViewModel.Clinics = _combosHelper.GetComboClinics();

                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", documentAssistantViewModel) });
                    }

                    string path = string.Empty;
                    if (documentAssistantViewModel.SignatureFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(documentAssistantViewModel.SignatureFile, "Signatures");
                    }

                    DocumentsAssistantEntity documentsEntity = await _converterHelper.ToDocumentsAssistantEntity(documentAssistantViewModel, path, true);

                    _context.Add(documentsEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<DocumentsAssistantEntity> assistant_List = await _context.DocumentsAssistant

                                                                            .Include(h => h.Clinic)

                                                                            .Where(ch => (ch.Clinic.Id == user_logged.Clinic.Id))
                                                                            .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocumentAssistant", assistant_List) });
                        
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the documents assistant: {documentsEntity.Name}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", documentAssistantViewModel )});
                }
            }

            clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
            list = new List<SelectListItem>();
            list.Insert(0, new SelectListItem
            {
                Text = clinic.Name,
                Value = $"{clinic.Id}"
            });

            documentAssistantViewModel.Clinics = list;
            documentAssistantViewModel.IdClinic = clinic.Id;
            documentAssistantViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Documents_Assistant, user_logged.Clinic.Id);
            documentAssistantViewModel.StatusList = _combosHelper.GetComboClientStatus();
            documentAssistantViewModel.GenderList = _combosHelper.GetComboGender();
            documentAssistantViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
            documentAssistantViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
            documentAssistantViewModel.Clinics = _combosHelper.GetComboClinics();

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", documentAssistantViewModel) });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentsAssistantEntity dosumentAssistantEntity = await _context.DocumentsAssistant
                                                                             .Include(s => s.Clinic)
                                                                             .Include(s => s.DocumentAssistantCertifications)
                                                                             .FirstOrDefaultAsync(s => s.Id == id);
            if (dosumentAssistantEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentsAssistantViewModel documentsAssistantViewModel;

            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);

                documentsAssistantViewModel = _converterHelper.ToDocumentsAssistantViewModel(dosumentAssistantEntity, user_logged.Clinic.Id);

                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    documentsAssistantViewModel.Clinics = list;

                    List<DocumentAssistantCertificationEntity> CertificationList = new List<DocumentAssistantCertificationEntity>();
                    DocumentAssistantCertificationEntity Certification = new DocumentAssistantCertificationEntity();
                    List<CourseEntity> coursList = _context.Courses.Where(n => n.Role == UserType.Documents_Assistant).ToList();

                    foreach (var item in coursList)
                    {
                        if (dosumentAssistantEntity.DocumentAssistantCertifications.Where(n => n.Course.Id == item.Id).Count() > 0)
                        {
                            foreach (var value in dosumentAssistantEntity.DocumentAssistantCertifications.Where(n => n.Course.Id == item.Id).ToList().OrderBy(c => c.ExpirationDate))
                            {
                                Certification.Name = item.Name;
                                Certification.CertificationNumber = value.CertificationNumber;
                                Certification.CertificateDate = value.CertificateDate;
                                Certification.ExpirationDate = value.ExpirationDate;
                                Certification.Id = value.Id;
                                CertificationList.Add(Certification);
                                Certification = new DocumentAssistantCertificationEntity();
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
                            Certification = new DocumentAssistantCertificationEntity();
                        }

                    }

                    documentsAssistantViewModel.DocumentAssistantCertificationIdealList = CertificationList;

                }
                return View(documentsAssistantViewModel);
            }

            return RedirectToAction("Home/Error404");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(int id, DocumentsAssistantViewModel documentsAssistantViewModel)
        {
            if (id != documentsAssistantViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                              .Include(u => u.Clinic)
                                              .FirstOrDefault(u => u.UserName == User.Identity.Name);

            ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
            List<SelectListItem> list = new List<SelectListItem>();

            if (ModelState.IsValid)
            {
                if (documentsAssistantViewModel.IdUser == "0")
                {
                    ModelState.AddModelError(string.Empty, "You must select a linked user");


                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });

                    documentsAssistantViewModel.Clinics = list;
                    documentsAssistantViewModel.IdClinic = clinic.Id;
                    documentsAssistantViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Documents_Assistant, user_logged.Clinic.Id);
                    documentsAssistantViewModel.StatusList = _combosHelper.GetComboClientStatus();
                    documentsAssistantViewModel.GenderList = _combosHelper.GetComboGender();
                    documentsAssistantViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
                    documentsAssistantViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
                    documentsAssistantViewModel.Clinics = _combosHelper.GetComboClinics();

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", documentsAssistantViewModel) });
                }

                string path = documentsAssistantViewModel.SignaturePath;
                if (documentsAssistantViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(documentsAssistantViewModel.SignatureFile, "Signatures");
                }
                DocumentsAssistantEntity documentsAssitantEntity = await _converterHelper.ToDocumentsAssistantEntity(documentsAssistantViewModel, path, false);
                _context.Update(documentsAssitantEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<DocumentsAssistantEntity> assistant_list = await _context.DocumentsAssistant

                                                                        .Include(h => h.Clinic)

                                                                        .Where(ch => (ch.Clinic.Id == user_logged.Clinic.Id))
                                                                        .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocumentAssistant", assistant_list) });

                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the documents assistant: {documentsAssitantEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
            list = new List<SelectListItem>();
            list.Insert(0, new SelectListItem
            {
                Text = clinic.Name,
                Value = $"{clinic.Id}"
            });

            documentsAssistantViewModel.Clinics = list;
            documentsAssistantViewModel.IdClinic = clinic.Id;
            documentsAssistantViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Documents_Assistant, user_logged.Clinic.Id);
            documentsAssistantViewModel.StatusList = _combosHelper.GetComboClientStatus();
            documentsAssistantViewModel.GenderList = _combosHelper.GetComboGender();
            documentsAssistantViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
            documentsAssistantViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
            documentsAssistantViewModel.Clinics = _combosHelper.GetComboClinics();

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", documentsAssistantViewModel) });
        }

        [Authorize(Roles = "Manager")]
        public IActionResult CreateDocumentAssistantCertification(int id = 0)
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

            DocumentAssistantCertificationViewModel model = new DocumentAssistantCertificationViewModel();

            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    model = new DocumentAssistantCertificationViewModel
                    {
                        CertificateDate = DateTime.Today,
                        ExpirationDate = DateTime.Today,
                        CertificationNumber = string.Empty,
                        Name = user_logged.Clinic.Name,
                        IdCourse = 0,
                        IdDocumentAssistant = 0,
                        DocumentAssistants = _combosHelper.GetComboDocumentsAssistantByClinic(user_logged.Clinic.Id),
                        Courses = _combosHelper.GetComboCourseByRole(UserType.Documents_Assistant)
                    };
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDocumentAssistantCertification(DocumentAssistantCertificationViewModel CertificationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                DocumentAssistantCertificationEntity Certification = new DocumentAssistantCertificationEntity();
                Certification = _converterHelper.ToDocumentAssistantCertificationEntity(CertificationViewModel, true, user_logged.UserName);
                _context.Add(Certification);
                try
                {
                    await _context.SaveChangesAsync();

                    List<DocumentAssistantCertificationEntity> Doc_Assist_List = await _context.DocumentAssistantCertifications
                                                                                               .Include(n => n.Course)
                                                                                               .Include(n => n.DocumentAssistant)
                                                                                               .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocumentAssistantCertifications", Doc_Assist_List) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Document Assistant: {Certification.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }

            }

            CertificationViewModel.Courses = _combosHelper.GetComboCourseByRole(UserType.Documents_Assistant);
            CertificationViewModel.DocumentAssistants = _combosHelper.GetComboDocumentsAssistantByClinic(user_logged.Clinic.Id);


            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateFacilitatorCertification", CertificationViewModel) });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditDocumentAssistantCertification(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            DocumentAssistantCertificationEntity Certification = await _context.DocumentAssistantCertifications
                                                                               .Include(f => f.Course)
                                                                               .Include(f => f.DocumentAssistant)
                                                                               .FirstOrDefaultAsync(f => f.Id == id);
            if (Certification == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentAssistantCertificationViewModel CertificationViewModel;
            if (User.IsInRole("Manager"))
            {
                CertificationViewModel = _converterHelper.ToDocumentAssistantCertificationViewModel(Certification, user_logged.Clinic.Id);

            }
            else
                CertificationViewModel = _converterHelper.ToDocumentAssistantCertificationViewModel(Certification, user_logged.Clinic.Id);

            return View(CertificationViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDocumentAssistantCertification(int id, DocumentAssistantCertificationViewModel CertificationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {

                DocumentAssistantCertificationEntity CertificationEntity = _converterHelper.ToDocumentAssistantCertificationEntity(CertificationViewModel, false, user_logged.UserName);
                _context.Update(CertificationEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    List<DocumentAssistantCertificationEntity> Certification_List = await _context.DocumentAssistantCertifications
                                                                                                  .Include(n => n.Course)
                                                                                                  .Include(n => n.DocumentAssistant)
                                                                                                  .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocumentAssistantCertifications", Certification_List) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Document Asistant: {CertificationEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            CertificationViewModel.DocumentAssistants = _combosHelper.GetComboDocumentsAssistantByClinic(user_logged.Clinic.Id);
            CertificationViewModel.Courses = _combosHelper.GetComboCourseByRole(UserType.Documents_Assistant);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditDocumentAssistantCertification", CertificationViewModel) });
        }

        [Authorize(Roles = "Manager, Documents_Assistant")]
        public async Task<IActionResult> DocumentAssistantCertifications(int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (User.IsInRole("Manager"))
            {
                if (idError == 1) //Imposible to delete
                {
                    ViewBag.Delete = "N";
                }

                return View(await _context.DocumentAssistantCertifications
                                          .Include(f => f.Course)
                                          .Include(f => f.DocumentAssistant)
                                          .AsSplitQuery()
                                          .Where(f => f.DocumentAssistant.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderBy(f => f.DocumentAssistant.Name)
                                          .ToListAsync());

            }
            else
            {
                if (User.IsInRole("Documents_Assistant"))
                {
                    DocumentsAssistantEntity Doc_Assistant = _context.DocumentsAssistant.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                    if (idError == 1) //Imposible to delete
                    {
                        ViewBag.Delete = "N";
                    }

                    return View(await _context.DocumentAssistantCertifications
                                              .Include(f => f.Course)
                                              .Include(f => f.DocumentAssistant)
                                              .AsSplitQuery()
                                              .Where(f => f.DocumentAssistant.Clinic.Id == user_logged.Clinic.Id
                                                       && f.DocumentAssistant.Id == Doc_Assistant.Id)
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

            DocumentAssistantCertificationEntity CertificationEntity = await _context.DocumentAssistantCertifications.FirstOrDefaultAsync(t => t.Id == id);
            if (CertificationEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.DocumentAssistantCertifications.Remove(CertificationEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(DocumentAssistantCertifications));
        }

        [Authorize(Roles = "Manager, Documents_Assistant")]
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
            List<DocumentsAssistantEntity> Doc_Assistant_List = new List<DocumentsAssistantEntity>();
            List<CourseEntity> course_List = new List<CourseEntity>();

            if (User.IsInRole("Manager"))
            {
                Doc_Assistant_List = _context.DocumentsAssistant
                                             .Include(m => m.DocumentAssistantCertifications)
                                             .ToList();

                course_List = _context.Courses
                                      .Include(m => m.DocumentAssistantCertifications)
                                      .Where(n => n.Role == UserType.Documents_Assistant
                                               && n.Active == true)
                                      .ToList();
            }
            else
            {
                if (User.IsInRole("Documents_Assistant"))
                {
                    DocumentsAssistantEntity Doc_Assistant = _context.DocumentsAssistant.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    Doc_Assistant_List = _context.DocumentsAssistant
                                                 .Include(m => m.DocumentAssistantCertifications)
                                                 .Where(n => n.Id == Doc_Assistant.Id)
                                                 .ToList();

                    course_List = _context.Courses
                                          .Include(m => m.DocumentAssistantCertifications)
                                          .Where(n => n.Role == UserType.Documents_Assistant
                                                   && n.Active == true)
                                          .ToList();
                }
            }

            foreach (var item in Doc_Assistant_List)
            {
                foreach (var course in course_List)
                {
                    if (item.DocumentAssistantCertifications.Where(n => n.Course.Id == course.Id).Count() > 0)
                    {
                        if (item.DocumentAssistantCertifications.Where(n => n.Course.Id == course.Id && n.ExpirationDate > DateTime.Today).Count() > 0)
                        {
                            if (item.DocumentAssistantCertifications.Where(n => n.Course.Id == course.Id && n.ExpirationDate > DateTime.Today).Max(d => d.ExpirationDate).AddDays(-30) < DateTime.Today)
                            {
                                auditCertification.TCMName = item.Name;
                                auditCertification.CourseName = course.Name;
                                auditCertification.Description = "Expired soon";
                                auditCertification.ExpirationDate = item.DocumentAssistantCertifications.FirstOrDefault(n => n.Course.Id == course.Id && n.ExpirationDate.AddDays(-30) < DateTime.Today).ExpirationDate.ToShortDateString();
                                auditCertification_List.Add(auditCertification);
                                auditCertification = new AuditCertification();
                            }
                        }
                        else
                        {
                            auditCertification.TCMName = item.Name;
                            auditCertification.CourseName = course.Name;
                            auditCertification.Description = "Expired";
                            auditCertification.ExpirationDate = item.DocumentAssistantCertifications.Where(n => n.Course.Id == course.Id).Max(m => m.ExpirationDate).ToShortDateString();
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

        [Authorize(Roles = "Documents_Assistant")]
        public async Task<IActionResult> EditModalReadOnly(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentsAssistantEntity Doc_AssistantEntity = await _context.DocumentsAssistant
                                                                         .Include(f => f.Clinic)
                                                                         .Include(f => f.DocumentAssistantCertifications)
                                                                         .FirstOrDefaultAsync(f => f.Id == id);
            if (Doc_AssistantEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentsAssistantViewModel model;
            if (User.IsInRole("Documents_Assistant"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                model = _converterHelper.ToDocumentsAssistantViewModel(Doc_AssistantEntity, user_logged.Clinic.Id);
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    model.Clinics = list;
                }

            }
            else
                return RedirectToAction("Home/Error404");

            return View(model);
        }

    }
}