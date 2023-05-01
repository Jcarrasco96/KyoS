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

        public EligibilityController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _renderHelper = renderHelper;
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
        public IActionResult Create(int idClient = 0)
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
        public async Task<IActionResult> Create(int id, EligibilityViewModel eligibilityViewModel)
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

                return RedirectToAction(nameof(Index));
            }
            EligibilityViewModel salida = new EligibilityViewModel()
            {
                IdClient = eligibilityViewModel.IdClient
            };
           
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", salida) });

        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            DiagnosticEntity diagnosticEntity = await _context.Diagnostics.FirstOrDefaultAsync(c => c.Id == id);
            if (diagnosticEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DiagnosticViewModel diagnosticViewModel = _converterHelper.ToDiagnosticViewModel(diagnosticEntity);

            return View(diagnosticViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiagnosticViewModel diagnosticViewModel)
        {
            if (id != diagnosticViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DiagnosticEntity diagnosticEntity = _converterHelper.ToDiagnosticEntity(diagnosticViewModel, false, user_logged.Id);
                _context.Update(diagnosticEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the diagnostic: {diagnosticEntity.Code}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(diagnosticViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DiagnosticEntity diagnosticEntity = await _context.Diagnostics.FirstOrDefaultAsync(c => c.Id == id);
            if (diagnosticEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Diagnostics.Remove(diagnosticEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {

                return RedirectToAction("Index", new { idError = 1 });
            }
            
            return RedirectToAction(nameof(Index));
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
                                                           .Where(m => m.Clinic.Id == user_logged.Clinic.Id 
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

    }
}