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
using KyoS.Common.Helpers;
using AspNetCore.Reporting;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace KyoS.Web.Controllers
{
    public class TCMPayStubController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IMimeType _mimeType;
        private readonly IExportExcellHelper _exportExcelHelper;
        private readonly IFileHelper _fileHelper;
        private readonly IReportHelper _reportHelper;
    
        public IConfiguration Configuration { get; }

        public TCMPayStubController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType, IExportExcellHelper exportExcelHelper, IFileHelper fileHelper, IReportHelper reportHelper, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _mimeType = mimeType;
            _exportExcelHelper = exportExcelHelper;
            _fileHelper = fileHelper;
            _reportHelper = reportHelper;
            Configuration = configuration;
        }

        [Authorize(Roles = "Manager, CaseManager")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
           
            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            
            if (User.IsInRole("Manager"))
            {
                List<TCMPayStubEntity> salida = await _context.TCMPayStubs
                                                              .Include(c => c.TCMPayStubDetails)
                                                              .Include(c => c.TCMNotes)
                                                              .Include(c => c.CaseMannager)
                                                              .AsSplitQuery()
                                                              .ToListAsync();
                return View(salida);
            }
            if (User.IsInRole("CaseManager"))
            {
                List<TCMPayStubEntity> salida = await _context.TCMPayStubs
                                                              .Include(c => c.TCMPayStubDetails)
                                                              .Include(c => c.TCMNotes)
                                                              .Include(c => c.CaseMannager)
                                                              .AsSplitQuery()
                                                              .Where(n => n.CaseMannager.LinkedUser == user_logged.UserName)
                                                              .ToListAsync();
                return View(salida);
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, CaseManager")]
        public IActionResult TCMNotesPendingByPayStub(DateTime datePayStubclose)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            TCMNotePendingByPayStubViewModel model = new TCMNotePendingByPayStubViewModel();
            List<TCMNoteEntity> tcmNotesUnPaid = new List<TCMNoteEntity>();
            if (User.IsInRole("Manager") || User.IsInRole("CaseManager"))
            {
                model = PayStubDetailsByDate(datePayStubclose);

                if (user_logged.Clinic.Setting.TCMPayStub_Filtro == TCMPayStubFiltro.Created)
                    ViewData["note"] = "The established payment condition is the CREATED services in the application.";
                if (user_logged.Clinic.Setting.TCMPayStub_Filtro == TCMPayStubFiltro.Approved)
                    ViewData["note"] = "The established payment condition is the APPROVED services by the supervisor.";
                if (user_logged.Clinic.Setting.TCMPayStub_Filtro == TCMPayStubFiltro.Billed)
                    ViewData["note"] = "The established payment condition is the BILLED services to the insurance.";
                if (user_logged.Clinic.Setting.TCMPayStub_Filtro == TCMPayStubFiltro.Paid)
                    ViewData["note"] = "The established payment condition is the PAID services by the insurance.";
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> TCMNotesPendingByPayStub(DateTime datePayStubclose, TCMNotePendingByPayStubViewModel payStubViewModel)
        {
            if (ModelState.IsValid)
            {
                TCMNotePendingByPayStubViewModel model = PayStubDetailsByDate(payStubViewModel.DatePayStubClose);
               
                if (model.TCMNoteList.Count() > 0)
                {
                    foreach (var tcm in model.TCMNoteList.GroupBy(n => n.TCMClient.Casemanager).ToList())
                    {
                        model.IdCaseManager = tcm.Key.Id;
                        TCMNotePendingByPayStubViewModel modelTemp = new TCMNotePendingByPayStubViewModel
                        {
                            Amount = model.Amount,
                            DatePayStub = model.DatePayStub,
                            DatePayStubClose = model.DatePayStubClose,
                            DatePayStubPayment = model.DatePayStubPayment,
                            IdCaseManager = model.IdCaseManager,
                            IdFiltro = model.IdFiltro,
                            IdStatus = model.IdStatus,
                            TCMNoteList = new List<TCMNoteEntity>(),
                            TCMPaystubDetails = new List<TCMPayStubDetailsEntity>(),
                            Units = model.Units
                        };
                        List<TCMPayStubDetailsEntity> PaystubDetailsList = new List<TCMPayStubDetailsEntity>();
                        List<TCMNoteEntity> tcmNoteList = new List<TCMNoteEntity>();
                        foreach (var item in model.TCMNoteList.Where(n => n.TCMClient.Casemanager.Id == tcm.Key.Id))
                        {
                            
                            TCMPayStubDetailsEntity PaystubDetails = new TCMPayStubDetailsEntity();
                            PaystubDetails.DateService = item.DateOfService;
                            PaystubDetails.IdCaseManager = tcm.Key.Id;
                            PaystubDetails.IdTCMNotes = item.Id;
                            PaystubDetails.NameClient = item.TCMClient.Client.Name;
                            
                            int Unit = 0;
                            decimal amount = 0.00m;
                            int minutesTCM = 0;
                            int residuoTCM = 0;
                            int unitTCM = 0;

                            minutesTCM = item.TCMNoteActivity.Sum(n => n.Minutes);
                            unitTCM = minutesTCM / 15;
                            residuoTCM = minutesTCM % 15;
                            if (residuoTCM > 7)
                            {
                                Unit = unitTCM + 1;
                                amount = (decimal)(unitTCM + 1) * item.TCMClient.Casemanager.Money / 4;
                            }
                            else
                            {
                                Unit = unitTCM;
                                amount = (decimal)(unitTCM * item.TCMClient.Casemanager.Money / 4);
                            }

                            PaystubDetails.Unit = Unit;
                            PaystubDetails.Amount = amount;

                            PaystubDetailsList.Add(PaystubDetails);
                            modelTemp.TCMNoteList.Add(item);
                        }
                        modelTemp.TCMPaystubDetails = PaystubDetailsList;
                        modelTemp.Units = PaystubDetailsList.Sum(n => n.Unit);
                        modelTemp.Amount = PaystubDetailsList.Sum(n => n.Amount);


                        TCMPayStubEntity paystub = _converterHelper.ToPayStubEntity(modelTemp, true);
                        _context.Add(paystub);
                    }
                    
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the Pay Stub with close date: {payStubViewModel.DatePayStubClose}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    return RedirectToAction("TCMNotesPendingByPayStub", new { dateTime = datePayStubclose });
                }
            }
            return RedirectToAction("TCMNotesPendingByPayStub", new { dateTime = datePayStubclose });
        }

        [Authorize(Roles = "Manager, CaseManager")]
        private  TCMNotePendingByPayStubViewModel PayStubDetailsByDate(DateTime datePayclose)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMNotePendingByPayStubViewModel model = new TCMNotePendingByPayStubViewModel();
            List<TCMNoteEntity> tcmNotesUnPaid = new List<TCMNoteEntity>();

            if (User.IsInRole("Manager"))
            {
                if (user_logged.Clinic.Setting.TCMPayStub_Filtro == TCMPayStubFiltro.Paid)
                {
                    tcmNotesUnPaid = _context.TCMNote
                                             .Include(n => n.TCMNoteActivity)
                                             .Include(n => n.TCMClient)
                                             .ThenInclude(n => n.Client)
                                             .Include(n => n.TCMClient)
                                             .ThenInclude(n => n.Casemanager)
                                             .Where(n => n.PayStub == null
                                                      && n.DateOfService <= datePayclose
                                                      && n.PaymentDate != null)
                                             .ToList();
                }
                else
                {
                    if (user_logged.Clinic.Setting.TCMPayStub_Filtro == TCMPayStubFiltro.Billed)
                    {
                        tcmNotesUnPaid = _context.TCMNote
                                                 .Include(n => n.TCMNoteActivity)
                                                 .Include(n => n.TCMClient)
                                                 .ThenInclude(n => n.Client)
                                                 .Include(n => n.TCMClient)
                                                 .ThenInclude(n => n.Casemanager)
                                                 .Where(n => n.PayStub == null
                                                          && n.DateOfService <= datePayclose
                                                          && n.BilledDate != null)
                                                 .ToList();
                    }
                    else
                    {
                        if (user_logged.Clinic.Setting.TCMPayStub_Filtro == TCMPayStubFiltro.Approved)
                        {
                            tcmNotesUnPaid = _context.TCMNote
                                                     .Include(n => n.TCMNoteActivity)
                                                     .Include(n => n.TCMClient)
                                                     .ThenInclude(n => n.Client)
                                                     .Include(n => n.TCMClient)
                                                     .ThenInclude(n => n.Casemanager)
                                                     .Where(n => n.PayStub == null
                                                              && n.DateOfService <= datePayclose
                                                              && n.ApprovedDate != null)
                                                     .ToList();
                        }
                        else
                        {
                            tcmNotesUnPaid = _context.TCMNote
                                                     .Include(n => n.TCMNoteActivity)
                                                     .Include(n => n.TCMClient)
                                                     .ThenInclude(n => n.Client)
                                                     .Include(n => n.TCMClient)
                                                     .ThenInclude(n => n.Casemanager)
                                                     .Where(n => n.PayStub == null
                                                              && n.DateOfService <= datePayclose)
                                                     .ToList();
                        }
                    }
                }

                int Unit = 0;
                int UnitTotal = 0;
                decimal amount = 0.00m;
                decimal amountTotal = 0.00m;

                int minutesTCM = 0;
                int residuoTCM = 0;
                int unitTCM = 0;
                List<int> idTCMs = new List<int>();
                foreach (var item in tcmNotesUnPaid)
                {
                    minutesTCM = item.TCMNoteActivity.Sum(n => n.Minutes);
                    unitTCM = minutesTCM / 15;
                    residuoTCM = minutesTCM % 15;
                    if (residuoTCM > 7)
                    {
                        Unit = unitTCM + 1;
                        amount = (decimal)(unitTCM + 1) * item.TCMClient.Casemanager.Money / 4;
                    }
                    else
                    {
                        Unit = unitTCM;
                        amount = (decimal)(unitTCM * item.TCMClient.Casemanager.Money / 4);
                    }

                    if (idTCMs.Contains(item.TCMClient.Casemanager.Id) == false)
                    {
                        idTCMs.Add(item.TCMClient.Casemanager.Id);
                    }
                    UnitTotal += Unit;
                    amountTotal += amount;
                }

                model = new TCMNotePendingByPayStubViewModel
                {
                    DatePayStub = DateTime.Today,
                    DatePayStubClose = datePayclose,
                    AmountTCMNotes = tcmNotesUnPaid.Count(),
                    Amount = (decimal)(amountTotal),
                    Units = UnitTotal,
                    TCMNoteList = tcmNotesUnPaid,
                    CantTCM = idTCMs.Count()
                };

                return model;
            }
            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic.Setting.TCMPayStub_Filtro == TCMPayStubFiltro.Paid)
                {
                    tcmNotesUnPaid = _context.TCMNote
                                             .Include(n => n.TCMNoteActivity)
                                             .Include(n => n.TCMClient)
                                             .ThenInclude(n => n.Client)
                                             .Include(n => n.TCMClient)
                                             .ThenInclude(n => n.Casemanager)
                                             .Where(n => n.PayStub == null
                                                      && n.DateOfService <= datePayclose
                                                      && n.PaymentDate != null
                                                      && n.TCMClient.Casemanager.LinkedUser == user_logged.UserName)
                                             .ToList();
                }
                else
                {
                    if (user_logged.Clinic.Setting.TCMPayStub_Filtro == TCMPayStubFiltro.Billed)
                    {
                        tcmNotesUnPaid = _context.TCMNote
                                                 .Include(n => n.TCMNoteActivity)
                                                 .Include(n => n.TCMClient)
                                                 .ThenInclude(n => n.Client)
                                                 .Include(n => n.TCMClient)
                                                 .ThenInclude(n => n.Casemanager)
                                                 .Where(n => n.PayStub == null
                                                          && n.DateOfService <= datePayclose
                                                          && n.BilledDate != null
                                                          && n.TCMClient.Casemanager.LinkedUser == user_logged.UserName)
                                                 .ToList();
                    }
                    else
                    {
                        if (user_logged.Clinic.Setting.TCMPayStub_Filtro == TCMPayStubFiltro.Approved)
                        {
                            tcmNotesUnPaid = _context.TCMNote
                                                     .Include(n => n.TCMNoteActivity)
                                                     .Include(n => n.TCMClient)
                                                     .ThenInclude(n => n.Client)
                                                     .Include(n => n.TCMClient)
                                                     .ThenInclude(n => n.Casemanager)
                                                     .Where(n => n.PayStub == null
                                                              && n.DateOfService <= datePayclose
                                                              && n.ApprovedDate != null
                                                              && n.TCMClient.Casemanager.LinkedUser == user_logged.UserName)
                                                     .ToList();
                        }
                        else
                        {
                            tcmNotesUnPaid = _context.TCMNote
                                                     .Include(n => n.TCMNoteActivity)
                                                     .Include(n => n.TCMClient)
                                                     .ThenInclude(n => n.Client)
                                                     .Include(n => n.TCMClient)
                                                     .ThenInclude(n => n.Casemanager)
                                                     .Where(n => n.PayStub == null
                                                              && n.DateOfService <= datePayclose
                                                              && n.TCMClient.Casemanager.LinkedUser == user_logged.UserName)
                                                     .ToList();
                        }
                    }
                }

                int Unit = 0;
                int UnitTotal = 0;
                decimal amount = 0.00m;
                decimal amountTotal = 0.00m;

                int minutesTCM = 0;
                int residuoTCM = 0;
                int unitTCM = 0;
                List<int> idTCMs = new List<int>();
                foreach (var item in tcmNotesUnPaid)
                {
                    minutesTCM = item.TCMNoteActivity.Sum(n => n.Minutes);
                    unitTCM = minutesTCM / 15;
                    residuoTCM = minutesTCM % 15;
                    if (residuoTCM > 7)
                    {
                        Unit = unitTCM + 1;
                        amount = (decimal)(unitTCM + 1) * item.TCMClient.Casemanager.Money / 4;
                    }
                    else
                    {
                        Unit = unitTCM;
                        amount = (decimal)(unitTCM * item.TCMClient.Casemanager.Money / 4);
                    }

                    if (idTCMs.Contains(item.TCMClient.Casemanager.Id) == false)
                    {
                        idTCMs.Add(item.TCMClient.Casemanager.Id);
                    }
                    UnitTotal += Unit;
                    amountTotal += amount;
                }

                model = new TCMNotePendingByPayStubViewModel
                {
                    DatePayStub = DateTime.Today,
                    DatePayStubClose = datePayclose,
                    AmountTCMNotes = tcmNotesUnPaid.Count(),
                    Amount = (decimal)(amountTotal),
                    Units = UnitTotal,
                    TCMNoteList = tcmNotesUnPaid,
                    CantTCM = idTCMs.Count()
                };

                return model;
            }
            return model;
        }

        [Authorize(Roles = "Manager, Casemanager")]
        public IActionResult TCMPaystubDetails(int id = 0)
        {
            TCMPayStubEntity entity = _context.TCMPayStubs
                                              .Include(n => n.TCMPayStubDetails)
                                              .Include(n => n.CaseMannager)
                                              .FirstOrDefault(n => n.Id == id);
            
            if (User.IsInRole("Manager") || User.IsInRole("Casemanager"))
            {
                return View(entity);
            }

            return null;
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdatePaid(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMPayStubEntity entity = await _context.TCMPayStubs
                                                    .Include(n => n.TCMPayStubDetails)
                                                    .Include(n => n.CaseMannager)
                                                    .FirstOrDefaultAsync(f => f.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMPaystubPaidViewModel model = new TCMPaystubPaidViewModel();
            if (User.IsInRole("Manager"))
            {
                model = new TCMPaystubPaidViewModel()
                {
                    IdTCMPaystub = entity.Id,
                    Amount = entity.Amount,
                    IdStatus = Convert.ToInt32(entity.StatusPayStub),
                    StatusList = _combosHelper.GetComboPaystubStatus(),
                    DatePayStubPayment = entity.DatePayStubPayment,
                    CaseMannager = entity.CaseMannager,
                    DatePayStubClose = entity.DatePayStubClose,
                    DatePayStub = entity.DatePayStub
                };
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaid(int id, TCMPaystubPaidViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                TCMPayStubEntity entity = await _context.TCMPayStubs.FirstOrDefaultAsync(n => n.Id == model.Id);

                if (entity != null)
                {
                    entity.StatusPayStub = StatusTCMPaystubUtils.GetStatusBillByIndex(model.IdStatus);
                    entity.DatePayStubPayment = model.DatePayStubPayment;

                    _context.Update(entity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        List<TCMPayStubEntity> salida = await _context.TCMPayStubs
                                                                      .Include(c => c.TCMPayStubDetails)
                                                                      .Include(c => c.TCMNotes)
                                                                      .Include(c => c.CaseMannager)
                                                                      .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMPayStubs", salida) });

                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the Paystub: {model.DatePayStubPayment}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }              
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "UpdatePaid", model) });
        }

    }
}
