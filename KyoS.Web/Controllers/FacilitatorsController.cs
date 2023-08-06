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

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FacilitatorEntity facilitatorEntity = await _context.Facilitators.Include(f => f.Clinic).FirstOrDefaultAsync(f => f.Id == id);
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
               
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", facilitatorViewModel) });
        }

        public async Task<IActionResult> EditModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FacilitatorEntity facilitatorEntity = await _context.Facilitators.Include(f => f.Clinic).FirstOrDefaultAsync(f => f.Id == id);
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
            if (ModelState.IsValid)
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
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", facilitatorViewModel) });
        }

    }

}