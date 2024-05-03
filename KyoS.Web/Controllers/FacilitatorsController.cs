//using ClosedXML.Excel;
using ClosedXML.Excel;
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using KyoS.Common.Helpers;

namespace KyoS.Web.Controllers
{
    public class FacilitatorsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IExportExcellHelper _exportExcelHelper;
        private readonly IRenderHelper _renderHelper;

        public FacilitatorsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IExportExcellHelper exportExcelHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _exportExcelHelper = exportExcelHelper;
            _renderHelper = renderHelper;


        }

        [Authorize(Roles = "Admin, Manager, Facilitator")]
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

                    return View(await _context.Facilitators
                                              .Include(f => f.Clinic)
                                              .Where(f => f.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name).ToListAsync());
                }
                if (User.IsInRole("Facilitator"))
                {
                    UserEntity user_logged = _context.Users

                                                     .Include(u => u.Clinic)
                                                     .ThenInclude(c => c.Setting)

                                                     .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }

                    return View(await _context.Facilitators
                                              .Include(f => f.Clinic)
                                              .Where(f => f.Clinic.Id == user_logged.Clinic.Id
                                                       && f.LinkedUser == user_logged.UserName)
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
        [Authorize(Roles = "Admin, Manager")]
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

        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FacilitatorEntity facilitatorEntity = await _context.Facilitators.FirstOrDefaultAsync(t => t.Id == id);
            if (facilitatorEntity == null)
            {
                return RedirectToAction("Home/Error404");
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

        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FacilitatorEntity facilitatorEntity = await _context.Facilitators
                                                                .Include(f => f.Clinic)
                                                                .Include(f => f.FacilitatorCertifications)
                                                                .FirstOrDefaultAsync(f => f.Id == id);
            if (facilitatorEntity == null)
            {
                return RedirectToAction("Home/Error404");
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
        [Authorize(Roles = "Admin, Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FacilitatorViewModel facilitatorViewModel)
        {
            if (id != facilitatorViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
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

        [Authorize(Roles = "Admin, Manager")]
        public IActionResult EXCEL()
        {
            /* var facilitator = _context.Facilitators.ToList();

             using (var workbook = new XLWorkbook())
             {
                 var worksheet = workbook.Worksheets.Add("facilitator");
                 var currentRow = 4;
                 worksheet.Cell(currentRow, 4).Value = "Name";
                 worksheet.Cell(currentRow, 5).Value = "Status";
                 worksheet.Cell(currentRow, 6).Value = "Link User";
                 worksheet.Style.Font.Bold = true;
                 IXLRange range =  worksheet.Range(worksheet.Cell(4, 4).Address, worksheet.Cell(4, 6).Address);
                 range.Style.Fill.SetBackgroundColor(XLColor.DarkGray);
                 range.SetAutoFilter();
                 foreach (var item in facilitator)
                 {
                     currentRow++;
                     worksheet.Cell(currentRow, 4).Value = item.Name;
                     worksheet.Cell(currentRow, 5).Value = item.Status;
                     worksheet.Cell(currentRow, 6).Value = item.LinkedUser;

                 }

                 worksheet.ColumnsUsed().AdjustToContents();

                 using (var stream = new MemoryStream())
                 {
                     workbook.SaveAs(stream);
                     var content = stream.ToArray();
                     return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facilitator.xlsx");
                 }
             }*/

            byte[] content = _exportExcelHelper.ExportFacilitatorHelper(_context.Facilitators.ToList());

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Facilitator.xlsx");
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


            return View(await _context.Facilitators
                                      .Include(f => f.Clinic)
                                      .Where(f => f.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderBy(f => f.Name).ToListAsync());
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditSignature(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FacilitatorEntity facilitatorEntity = await _context.Facilitators
                                                                .Include(n => n.Clinic)
                                                                .FirstOrDefaultAsync(c => c.Id == id);

            if (facilitatorEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FacilitatorViewModel facilitatorViewModel = _converterHelper.ToFacilitatorViewModel(facilitatorEntity, facilitatorEntity.Clinic.Id);

            return View(facilitatorViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SaveFacilitatorSignature(string id, string dataUrl)
        {
            string signPath = await _imageHelper.UploadSignatureAsync(dataUrl, "Facilitators");

            FacilitatorEntity facilitator = await _context.Facilitators
                                                          .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(id));
            if (facilitator != null)
            {
                facilitator.SignaturePath = signPath;
                _context.Update(facilitator);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectToUrl = Url.Action("Signatures", "Facilitators") });
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
                        UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Facilitator, user_logged.Clinic.Id),
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
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(FacilitatorViewModel facilitatorViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (ModelState.IsValid)
            {
                FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.Name == facilitatorViewModel.Name);
                if (facilitator == null)
                {
                    if (facilitatorViewModel.IdUser == "0")
                    {
                        ModelState.AddModelError(string.Empty, "You must select a linked user");
                        ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                        List<SelectListItem> list = new List<SelectListItem>();
                        list.Insert(0, new SelectListItem
                        {
                            Text = clinic.Name,
                            Value = $"{clinic.Id}"
                        });

                        facilitatorViewModel.Clinics = list;
                        facilitatorViewModel.IdClinic = clinic.Id;
                        facilitatorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                        facilitatorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Facilitator, user_logged.Clinic.Id);
                        facilitatorViewModel.GenderList = _combosHelper.GetComboGender();
                        facilitatorViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
                        facilitatorViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
                        facilitatorViewModel.Clinics = _combosHelper.GetComboClinics();
                       

                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", facilitatorViewModel) });
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

                        List<FacilitatorEntity> facilitators_List = await _context.Facilitators
                                                                                  .Include(n => n.Clinic)
                                                                                  .Where(f => f.Clinic.Id == user_logged.Clinic.Id)
                                                                                  .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewFacilitators", facilitators_List) });
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
                    ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });

                    facilitatorViewModel.Clinics = list;
                    facilitatorViewModel.IdClinic = clinic.Id;
                    facilitatorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                    facilitatorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Facilitator, user_logged.Clinic.Id);
                    facilitatorViewModel.GenderList = _combosHelper.GetComboGender();
                    facilitatorViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
                    facilitatorViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
                    
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", facilitatorViewModel) });
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

                facilitatorViewModel.Clinics = list;
                facilitatorViewModel.IdClinic = clinic.Id;
                facilitatorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                facilitatorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Facilitator, user_logged.Clinic.Id);
                facilitatorViewModel.GenderList = _combosHelper.GetComboGender();
                facilitatorViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
                facilitatorViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
                
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", facilitatorViewModel) });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FacilitatorEntity facilitatorEntity = await _context.Facilitators
                                                                .Include(f => f.Clinic)
                                                                .Include(f => f.FacilitatorCertifications)
                                                                .FirstOrDefaultAsync(f => f.Id == id);
            if (facilitatorEntity == null)
            {
                return RedirectToAction("Home/Error404");
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

                    List<FacilitatorCertificationEntity> CertificationList = new List<FacilitatorCertificationEntity>();
                    FacilitatorCertificationEntity Certification = new FacilitatorCertificationEntity();
                    List<CourseEntity> coursList = _context.Courses.Where(n => n.Role == UserType.Facilitator).ToList();

                    foreach (var item in coursList)
                    {
                        if (facilitatorEntity.FacilitatorCertifications.Where(n => n.Course.Id == item.Id).Count() > 0)
                        {
                            foreach (var value in facilitatorEntity.FacilitatorCertifications.Where(n => n.Course.Id == item.Id).ToList().OrderBy(c => c.ExpirationDate))
                            {
                                Certification.Name = item.Name;
                                Certification.CertificationNumber = value.CertificationNumber;
                                Certification.CertificateDate = value.CertificateDate;
                                Certification.ExpirationDate = value.ExpirationDate;
                                Certification.Id = value.Id;
                                CertificationList.Add(Certification);
                                Certification = new FacilitatorCertificationEntity();
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
                            Certification = new FacilitatorCertificationEntity();
                        }

                    }
                    facilitatorViewModel.FacilitatorCertificationIdealList = CertificationList;
                }

            }
            else
                facilitatorViewModel = _converterHelper.ToFacilitatorViewModel(facilitatorEntity, 0);

            return View(facilitatorViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(int id, FacilitatorViewModel facilitatorViewModel)
        {
            if (id != facilitatorViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == facilitatorViewModel.IdClinic);
            List<SelectListItem> list = new List<SelectListItem>();

            if (ModelState.IsValid)
            {
                if (facilitatorViewModel.IdUser == "0")
                {
                    ModelState.AddModelError(string.Empty, "You must select a linked user");
                    
                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });

                    facilitatorViewModel.Clinics = list;
                    facilitatorViewModel.IdClinic = clinic.Id;
                    facilitatorViewModel.GenderList = _combosHelper.GetComboGender();
                    facilitatorViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
                    facilitatorViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
                    facilitatorViewModel.Clinics = _combosHelper.GetComboClinics();
                    facilitatorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                    facilitatorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Facilitator, facilitatorViewModel.IdClinic);

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", facilitatorViewModel) });
                }

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

                    List<FacilitatorEntity> facilitators_List = await _context.Facilitators
                                                                                  .Include(n => n.Clinic)
                                                                                  .Where(f => f.Clinic.Id == user_logged.Clinic.Id)
                                                                                  .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewFacilitators", facilitators_List) });
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

            list.Insert(0, new SelectListItem
            {
                Text = clinic.Name,
                Value = $"{clinic.Id}"
            });

            facilitatorViewModel.Clinics = list;
            facilitatorViewModel.IdClinic = clinic.Id;
            facilitatorViewModel.StatusList = _combosHelper.GetComboClientStatus();
            facilitatorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Facilitator, facilitatorViewModel.IdClinic);
            facilitatorViewModel.GenderList = _combosHelper.GetComboGender();
            facilitatorViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
            facilitatorViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
            facilitatorViewModel.Clinics = _combosHelper.GetComboClinics();

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", facilitatorViewModel) });
        }

        [Authorize(Roles = "Manager")]
        public IActionResult CreateFacilitatorCertification(int id = 0)
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

            FacilitatorCertificationViewModel model = new FacilitatorCertificationViewModel();

            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    model = new FacilitatorCertificationViewModel
                    {
                        CertificateDate = DateTime.Today,
                        ExpirationDate = DateTime.Today,
                        CertificationNumber = string.Empty,
                        Name = user_logged.Clinic.Name,
                        IdCourse = 0,
                        IdFacilitator = 0,
                        Facilitators = _combosHelper.GetComboFacilitators(),
                        Courses = _combosHelper.GetComboCourseByRole(UserType.Facilitator)
                    };
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFacilitatorCertification(FacilitatorCertificationViewModel CertificationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                FacilitatorCertificationEntity Certification = new FacilitatorCertificationEntity();
                Certification = _converterHelper.ToFacilitatorCertificationEntity(CertificationViewModel, true, user_logged.UserName);
                _context.Add(Certification);
                try
                {
                    await _context.SaveChangesAsync();

                    List<FacilitatorCertificationEntity> Facilitator_List = await _context.FacilitatorCertifications
                                                                                          .Include(n => n.Course)
                                                                                          .Include(n => n.Facilitator)
                                                                                          .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewFacilitatorCertifications", Facilitator_List) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Case Mannager: {Certification.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }

            }

            CertificationViewModel.Courses = _combosHelper.GetComboCourseByRole(UserType.CaseManager);
            CertificationViewModel.Facilitators = _combosHelper.GetComboFacilitators();


            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateFacilitatorCertification", CertificationViewModel) });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditFacilitatorCertification(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FacilitatorCertificationEntity Certification = await _context.FacilitatorCertifications
                                                                         .Include(f => f.Course)
                                                                         .Include(f => f.Facilitator)
                                                                         .FirstOrDefaultAsync(f => f.Id == id);
            if (Certification == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FacilitatorCertificationViewModel CertificationViewModel;
            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                CertificationViewModel = _converterHelper.ToFacilitatorCertificationViewModel(Certification);


            }
            else
                CertificationViewModel = _converterHelper.ToFacilitatorCertificationViewModel(Certification);

            return View(CertificationViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFacilitatorCertification(int id, FacilitatorCertificationViewModel CertificationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {

                FacilitatorCertificationEntity CertificationEntity = _converterHelper.ToFacilitatorCertificationEntity(CertificationViewModel, false, user_logged.UserName);
                _context.Update(CertificationEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    List<FacilitatorCertificationEntity> Certification_List = await _context.FacilitatorCertifications
                                                                                            .Include(n => n.Course)
                                                                                            .Include(n => n.Facilitator)
                                                                                            .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewFacilitatorCertifications", Certification_List) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Facilitator: {CertificationEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            CertificationViewModel.Facilitators = _combosHelper.GetComboFacilitators();
            CertificationViewModel.Courses = _combosHelper.GetComboCourseByRole(UserType.Facilitator);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditFacilitatorCertification", CertificationViewModel) });
        }

        [Authorize(Roles = "Manager, Facilitator")]
        public async Task<IActionResult> FacilitatorCertifications(int idError = 0)
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

                return View(await _context.FacilitatorCertifications
                                          .Include(f => f.Course)
                                          .Include(f => f.Facilitator)
                                          .AsSplitQuery()
                                          .Where(f => f.Facilitator.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderBy(f => f.Facilitator.Name)
                                          .ToListAsync());

            }
            else
            {
                if (User.IsInRole("Facilitator"))
                {
                    FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                    if (idError == 1) //Imposible to delete
                    {
                        ViewBag.Delete = "N";
                    }

                    return View(await _context.FacilitatorCertifications
                                              .Include(f => f.Course)
                                              .Include(f => f.Facilitator)
                                              .AsSplitQuery()
                                              .Where(f => f.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                       && f.Facilitator.Id == facilitator.Id)
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

            FacilitatorCertificationEntity CertificationEntity = await _context.FacilitatorCertifications.FirstOrDefaultAsync(t => t.Id == id);
            if (CertificationEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.FacilitatorCertifications.Remove(CertificationEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(FacilitatorCertifications));
        }

        [Authorize(Roles = "Manager, TCMSupervisor, Facilitator")]
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
            List<FacilitatorEntity> facilitator_List = new List<FacilitatorEntity>();
            List<CourseEntity> course_List = new List<CourseEntity>();

            if (User.IsInRole("Manager"))
            {
                facilitator_List = _context.Facilitators
                                           .Include(m => m.FacilitatorCertifications)
                                           .ToList();

                course_List = _context.Courses
                                      .Include(m => m.FacilitatorCertifications)
                                      .Where(n => n.Role == UserType.Facilitator
                                               && n.Active == true)
                                      .ToList();
            }
            else
            {
                if (User.IsInRole("Facilitator"))
                {
                    FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    facilitator_List = _context.Facilitators
                                               .Include(m => m.FacilitatorCertifications)
                                               .Where(n => n.Id == facilitator.Id)
                                               .ToList();

                    course_List = _context.Courses
                                          .Include(m => m.FacilitatorCertifications)
                                          .Where(n => n.Role == UserType.Facilitator
                                                   && n.Active == true)
                                          .ToList();
                }
            }

            foreach (var item in facilitator_List)
            {
                foreach (var course in course_List)
                {
                    if (item.FacilitatorCertifications.Where(n => n.Course.Id == course.Id).Count() > 0)
                    {
                        if (item.FacilitatorCertifications.Where(n => n.Course.Id == course.Id && n.ExpirationDate > DateTime.Today).Count() > 0)
                        {
                            if (item.FacilitatorCertifications.Where(n => n.Course.Id == course.Id && n.ExpirationDate > DateTime.Today).Max(d => d.ExpirationDate).AddDays(-30) < DateTime.Today)
                            {
                                auditCertification.TCMName = item.Name;
                                auditCertification.CourseName = course.Name;
                                auditCertification.Description = "Expired soon";
                                auditCertification.ExpirationDate = item.FacilitatorCertifications.FirstOrDefault(n => n.Course.Id == course.Id && n.ExpirationDate.AddDays(-30) < DateTime.Today).ExpirationDate.ToShortDateString();
                                auditCertification_List.Add(auditCertification);
                                auditCertification = new AuditCertification();
                            }
                        }
                        else
                        {
                            auditCertification.TCMName = item.Name;
                            auditCertification.CourseName = course.Name;
                            auditCertification.Description = "Expired";
                            auditCertification.ExpirationDate = item.FacilitatorCertifications.Where(n => n.Course.Id == course.Id).Max(m => m.ExpirationDate).ToShortDateString();
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

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> EditModalReadOnly(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FacilitatorEntity facilitatorEntity = await _context.Facilitators
                                                                .Include(f => f.Clinic)
                                                                .Include(f => f.FacilitatorCertifications)
                                                                .ThenInclude(f => f.Course)
                                                                .FirstOrDefaultAsync(f => f.Id == id);
            if (facilitatorEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FacilitatorViewModel facilitatorViewModel;
            if (User.IsInRole("Facilitator"))
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

                    List<FacilitatorCertificationEntity> CertificationList = new List<FacilitatorCertificationEntity>();
                    FacilitatorCertificationEntity Certification = new FacilitatorCertificationEntity();
                    List<CourseEntity> coursList = _context.Courses.Where(n => n.Role == UserType.Facilitator).ToList();

                    foreach (var item in coursList)
                    {
                        if (facilitatorEntity.FacilitatorCertifications.Where(n => n.Course.Id == item.Id).Count() > 0)
                        {
                            foreach (var value in facilitatorEntity.FacilitatorCertifications.Where(n => n.Course.Id == item.Id).ToList().OrderBy(c => c.ExpirationDate))
                            {
                                Certification.Name = item.Name;
                                Certification.CertificationNumber = value.CertificationNumber;
                                Certification.CertificateDate = value.CertificateDate;
                                Certification.ExpirationDate = value.ExpirationDate;
                                Certification.Id = value.Id;
                                CertificationList.Add(Certification);
                                Certification = new FacilitatorCertificationEntity();
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
                            Certification = new FacilitatorCertificationEntity();
                        }

                    }
                    facilitatorViewModel.FacilitatorCertificationIdealList = CertificationList;
                }

            }
            else
                return RedirectToAction("Home/Error404");

            return View(facilitatorViewModel);
        }

    }

}