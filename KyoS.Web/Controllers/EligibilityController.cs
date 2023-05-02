using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Manager")]
    public class EligibilityController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IMimeType _mimeType;
        private readonly IExportExcellHelper _exportExcelHelper;
        private readonly IFileHelper _fileHelper;

        public EligibilityController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType, IExportExcellHelper exportExcelHelper, IFileHelper fileHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _renderHelper = renderHelper;
            _mimeType = mimeType;
            _exportExcelHelper = exportExcelHelper;
            _fileHelper = fileHelper;
        }
        
        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Manager"))
            {
                List<ClientEntity> clients = await _context.Clients
                                                           .Include(n => n.EligibilityList)
                                                           .Include(n => n.Clients_HealthInsurances)
                                                           .ThenInclude(n => n.HealthInsurance)
                                                           .Where(m => m.Clinic.Id == user_logged.Clinic.Id)
                                                           .ToListAsync();


                return View(clients);
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public IActionResult Create(int idClient = 0, int origin = 0)
        {
            if (idClient != 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);

                EligibilityViewModel model = new EligibilityViewModel()
                {
                    IdClient = idClient,
                    AdmissionedFor = user_logged.FullName,
                    EligibilityDate = DateTime.Today
                };
                ViewData["origin"] = origin;
                return View(model);
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
        public async Task<IActionResult> Create(int id, EligibilityViewModel eligibilityViewModel, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (ModelState.IsValid)
            {
                string documentPath = string.Empty;
                if (eligibilityViewModel.DocumentFile != null)
                {
                    documentPath = await _imageHelper.UploadFileAsync(eligibilityViewModel.DocumentFile, "Clients");
                }

                if (id == 0)
                {
                    EligibilityViewModel eligibility = new EligibilityViewModel
                    {
                        Id = 0,
                        FileUrl = documentPath,
                        FileName = eligibilityViewModel.DocumentFile.FileName,
                        
                        CreatedOn = DateTime.Today,
                        AdmissionedFor = user_logged.FullName,
                        Client = _context.Clients.FirstOrDefault(n => n.Id == eligibilityViewModel.IdClient),
                        DateUp = DateTime.Today,
                        Exists = true,
                        CreatedBy = user_logged.UserName,
                        EligibilityDate = eligibilityViewModel.EligibilityDate
                        
                    };
                    _context.Add(eligibility);
                    await _context.SaveChangesAsync();
                }

                if (origin == 0)
                {
                    List<ClientEntity> clients = await _context.Clients
                                                               .Include(n => n.EligibilityList)
                                                               .Include(n => n.Clients_HealthInsurances)
                                                               .ThenInclude(n => n.HealthInsurance)
                                                               .Where(m => m.Clinic.Id == user_logged.Clinic.Id)
                                                               .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewIndex", clients) });
                }

                if (origin == 1)
                {
                    List<ClientEntity> clients = await _context.Clients
                                                               .Include(n => n.EligibilityList)
                                                               .Include(n => n.Clients_HealthInsurances)
                                                               .ThenInclude(n => n.HealthInsurance)
                                                               .Where(m => m.Status == Common.Enums.StatusType.Open
                                                                && m.Clinic.Id == user_logged.Clinic.Id
                                                                && m.EligibilityList.Where(n => n.EligibilityDate.Month == DateTime.Today.Month
                                                                                             && n.EligibilityDate.Year == DateTime.Today.Year).Count() == 0)
                                                               .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewEligibilityClients", clients) });
                }
                
            }
            EligibilityViewModel salida = new EligibilityViewModel()
            {
                IdClient = eligibilityViewModel.IdClient
            };
           
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", salida) });

        }

        [Authorize(Roles = "Manager")]
        public IActionResult Delete(int id = 0)
        {
            if (id > 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DeleteViewModel model = new DeleteViewModel
                {
                    Id_Element = id,
                    Desciption = "Do you want to delete this record?"

                };
                return View(model);
            }
            else
            {
                //Edit
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(DeleteViewModel elegibilityModel)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                EligibilityEntity eligibility = await _context.Eligibilities
                                                              .FirstAsync(n => n.Id == elegibilityModel.Id_Element);
                try
                {
                    _context.Eligibilities.Remove(eligibility);
                    await _context.SaveChangesAsync();

                    List<ClientEntity> clients = await _context.Clients
                                                               .Include(n => n.EligibilityList)
                                                               .Include(n => n.Clients_HealthInsurances)
                                                               .ThenInclude(n => n.HealthInsurance)
                                                               .Where(m => m.Clinic.Id == user_logged.Clinic.Id)
                                                               .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewIndex", clients) });
                }
                catch (Exception)
                {
                    List<ClientEntity> clients = await _context.Clients
                                                               .Include(n => n.EligibilityList)
                                                               .Include(n => n.Clients_HealthInsurances)
                                                               .ThenInclude(n => n.HealthInsurance)
                                                               .Where(m => m.Clinic.Id == user_logged.Clinic.Id)
                                                               .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewIndex", clients) });

                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Index") });
        }


        public async Task<IActionResult> EligibilityClients(int idError = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Manager"))
            {
                List<ClientEntity> clients = await _context.Clients
                                                           .Include(n => n.EligibilityList)
                                                           .Include(n => n.Clients_HealthInsurances)
                                                           .ThenInclude(n => n.HealthInsurance)
                                                           .Where(m => m.Status == Common.Enums.StatusType.Open
                                                            && m.Clinic.Id == user_logged.Clinic.Id 
                                                            && m.EligibilityList.Where(n => n.EligibilityDate.Month == DateTime.Today.Month 
                                                                                         && n.EligibilityDate.Year == DateTime.Today.Year).Count() == 0)
                                                           .ToListAsync();
                string mounth = string.Empty;
                if (DateTime.Today.Month == 1)
                {
                    mounth = "January";
                }
                if (DateTime.Today.Month == 2)
                {
                    mounth = "February";
                }
                if (DateTime.Today.Month == 3)
                {
                    mounth = "March";
                }
                if (DateTime.Today.Month == 4)
                {
                    mounth = "April";
                }
                if (DateTime.Today.Month == 5)
                {
                    mounth = "May";
                }
                if (DateTime.Today.Month == 6)
                {
                    mounth = "June";
                }
                if (DateTime.Today.Month == 7)
                {
                    mounth = "July";
                }
                if (DateTime.Today.Month == 8)
                {
                    mounth = "August";
                }
                if (DateTime.Today.Month == 9)
                {
                    mounth = "September";
                }
                if (DateTime.Today.Month == 10)
                {
                    mounth = "October";
                }
                if (DateTime.Today.Month == 11)
                {
                    mounth = "November";
                }
                if (DateTime.Today.Month == 12)
                {
                    mounth = "December";
                }
                ViewData["mounth"] = mounth;
                return View(clients);
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> OpenDocument(int id)
        {
            EligibilityEntity elegibility = await _context.Eligibilities
                                                          .FirstOrDefaultAsync(d => d.Id == id);
            if (elegibility == null)
            {
                return RedirectToAction("Home/Error404");
            }
            string mimeType = _mimeType.GetMimeType(elegibility.FileName);
            return File(elegibility.FileUrl, mimeType);
        }

    }
}