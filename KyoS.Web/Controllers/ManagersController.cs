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
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManagersController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IExportExcellHelper _exportExcelHelper;
        private readonly IRenderHelper _renderHelper;

        public ManagersController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IExportExcellHelper exportExcelHelper, IRenderHelper renderHelper)
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
            {
                UserEntity user_logged = _context.Users

                                                 .Include(u => u.Clinic)
                                                 .ThenInclude(c => c.Setting)

                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                return View(await _context.Manager
                                          .Include(f => f.Clinic)
                                          .OrderBy(f => f.Name).ToListAsync());                
            }

            return RedirectToAction("Home/Error404");
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

            ManagerViewModel model;

            if (User.IsInRole("Admin"))
            {
                ClinicEntity clinic = _context.Clinics.FirstOrDefault();
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = clinic.Name,
                    Value = $"{clinic.Id}"
                });
                model = new ManagerViewModel
                {
                    Clinics = list,
                    IdClinic = clinic.Id,
                    StatusList = _combosHelper.GetComboClientStatus(),
                    UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Manager, clinic.Id)
                };
                return View(model);
            }

            return RedirectToAction("Home/Error404");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ManagerViewModel managerViewModel)
        {
            if (ModelState.IsValid)
            {
                ManagerEntity manager = await _context.Manager.FirstOrDefaultAsync(f => f.Name == managerViewModel.Name);
                if (manager == null)
                {
                    if (managerViewModel.IdUser == "0")
                    {
                        ModelState.AddModelError(string.Empty, "You must select a linked user");
                        return View(managerViewModel);
                    }

                    string path = string.Empty;
                    if (managerViewModel.SignatureFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(managerViewModel.SignatureFile, "Signatures");
                    }

                    ManagerEntity managerEntity = await _converterHelper.ToManagerEntity(managerViewModel, path, true);

                    _context.Add(managerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the manager: {managerEntity.Name}");
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
            return View(managerViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ManagerEntity managerEntity = await _context.Manager.FirstOrDefaultAsync(t => t.Id == id);
            if (managerEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Manager.Remove(managerEntity);
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

            ManagerEntity managerEntity = await _context.Manager.Include(f => f.Clinic).FirstOrDefaultAsync(f => f.Id == id);
            if (managerEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ManagerViewModel managerViewModel;
            if (User.IsInRole("Admin"))
            {
                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync();
                managerViewModel = _converterHelper.ToManagerViewModel(managerEntity);
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = clinic.Name,
                    Value = $"{clinic.Id}"
                });
            
                return View(managerViewModel);
            }

            return RedirectToAction("Home/Error404");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ManagerViewModel managerViewModel)
        {
            if (id != managerViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                string path = managerViewModel.SignaturePath;
                if (managerViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(managerViewModel.SignatureFile, "Signatures");
                }
                ManagerEntity managerEntity = await _converterHelper.ToManagerEntity(managerViewModel, path, false);
                _context.Update(managerEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the manager: {managerEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(managerViewModel);
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Signatures()
        {
          
            return View(await _context.Manager
                                      .Include(f => f.Clinic)
                                      .OrderBy(f => f.Name).ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditSignature(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ManagerEntity managerEntity = await _context.Manager
                                                        .Include(n => n.Clinic)
                                                        .FirstOrDefaultAsync(c => c.Id == id);

            if (managerEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ManagerViewModel managerViewModel = _converterHelper.ToManagerViewModel(managerEntity);

            return View(managerViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SaveManagerSignature(string id, string dataUrl)
        {
            string signPath = await _imageHelper.UploadSignatureAsync(dataUrl, "Managers"); 

            ManagerEntity manager = await _context.Manager
                                                  .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(id));
            if (manager != null)
            {
                manager.SignaturePath = signPath;
                _context.Update(manager);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectToUrl = Url.Action("Signatures", "Managers") });
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

            ManagerViewModel model;

            if (User.IsInRole("Admin"))
            {
                ClinicEntity clinic = _context.Clinics.FirstOrDefault();
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = clinic.Name,
                    Value = $"{clinic.Id}"
                });
                model = new ManagerViewModel
                {
                    Clinics = list,
                    IdClinic = clinic.Id,
                    StatusList = _combosHelper.GetComboClientStatus(),
                    UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Manager, clinic.Id)
                };
                return View(model);
            }

            return RedirectToAction("Home/Error404");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(ManagerViewModel managerViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                ManagerEntity manager = await _context.Manager.FirstOrDefaultAsync(f => f.Name == managerViewModel.Name);
                if (manager == null)
                {
                    if (managerViewModel.IdUser == "0")
                    {
                        ModelState.AddModelError(string.Empty, "You must select a linked user");
                        ClinicEntity clinic = _context.Clinics.FirstOrDefault();
                        List<SelectListItem> list = new List<SelectListItem>();
                        list.Insert(0, new SelectListItem
                        {
                            Text = clinic.Name,
                            Value = $"{clinic.Id}"
                        });

                        managerViewModel.Clinics = list;
                        managerViewModel.IdClinic = clinic.Id;
                        managerViewModel.StatusList = _combosHelper.GetComboClientStatus();
                        managerViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Manager, clinic.Id);

                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", managerViewModel) });
                    }

                    string path = string.Empty;
                    if (managerViewModel.SignatureFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(managerViewModel.SignatureFile, "Signatures");
                    }

                    ManagerEntity managerEntity = await _converterHelper.ToManagerEntity(managerViewModel, path, true);

                    _context.Add(managerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<ManagerEntity> managers_List = await _context.Manager
                                                                          .Include(n => n.Clinic)
                                                                          .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewManagers", managers_List) });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the manager: {managerEntity.Name}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    ClinicEntity clinic = _context.Clinics.FirstOrDefault();
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });

                    managerViewModel.Clinics = list;
                    managerViewModel.IdClinic = clinic.Id;
                    managerViewModel.StatusList = _combosHelper.GetComboClientStatus();
                    managerViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Manager, clinic.Id);

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", managerViewModel) });
                }
            }
            else
            {

                ClinicEntity clinic = _context.Clinics.FirstOrDefault();
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = clinic.Name,
                    Value = $"{clinic.Id}"
                });

                managerViewModel.Clinics = list;
                managerViewModel.IdClinic = clinic.Id;
                managerViewModel.StatusList = _combosHelper.GetComboClientStatus();
                managerViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Manager, clinic.Id);

            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", managerViewModel) });
        }

        public async Task<IActionResult> EditModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ManagerEntity managerEntity = await _context.Manager.Include(f => f.Clinic).FirstOrDefaultAsync(f => f.Id == id);
            if (managerEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ManagerViewModel managerViewModel;
            if (User.IsInRole("Admin"))
            {
                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync();
                managerViewModel = _converterHelper.ToManagerViewModel(managerEntity);
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = clinic.Name,
                    Value = $"{clinic.Id}"
                });

                return View(managerViewModel);
            }

            return RedirectToAction("Home/Error404");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(int id, ManagerViewModel managerViewModel)
        {
            UserEntity user_logged = _context.Users
                                            .Include(u => u.Clinic)
                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

            ClinicEntity clinic = _context.Clinics.FirstOrDefault();
            List<SelectListItem> list = new List<SelectListItem>();

            if (id != managerViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                if (managerViewModel.IdUser == "0")
                {
                    ModelState.AddModelError(string.Empty, "You must select a linked user");
                   
                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });

                    managerViewModel.Clinics = list;
                    managerViewModel.IdClinic = clinic.Id;
                    managerViewModel.StatusList = _combosHelper.GetComboClientStatus();
                    managerViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Manager, user_logged.Clinic.Id);

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", managerViewModel) });
                }

                string path = managerViewModel.SignaturePath;
                if (managerViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(managerViewModel.SignatureFile, "Signatures");
                }

                ManagerEntity managerEntity = await _converterHelper.ToManagerEntity(managerViewModel, path, false);
                _context.Update(managerEntity);
                
                try
                {
                    await _context.SaveChangesAsync();

                    List<ManagerEntity> managers_List = await _context.Manager
                                                                          .Include(n => n.Clinic)
                                                                          .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewManagers", managers_List) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the manager: {managerEntity.Name}");
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

            managerViewModel.Clinics = list;
            managerViewModel.IdClinic = clinic.Id;
            managerViewModel.StatusList = _combosHelper.GetComboClientStatus();
            managerViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Manager, user_logged.Clinic.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", managerViewModel) });
        }

    }

}