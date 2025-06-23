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
    public class PayStubsController : Controller
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

        public PayStubsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType, IExportExcellHelper exportExcelHelper, IFileHelper fileHelper, IReportHelper reportHelper, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
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

        [Authorize(Roles = "Manager,  Facilitator, Documents_Assistant")]
        public async Task<IActionResult> Index(int idError = 0, int all = 0, UserType userType = UserType.Admin)
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
                if (all > 0)
                {
                    List<PayStubEntity> salida = await _context.PayStubs
                                                               .Include(c => c.PayStubDetails)
                                                               .Include(c => c.WordayClients)
                                                               .Include(c => c.Facilitator)
                                                               .Include(c => c.Doc_Assisstant)
                                                               .AsSplitQuery()
                                                               .Where(n => n.Role == userType)
                                                               .ToListAsync();
                    return View(salida);
                }
                else
                {
                    List<PayStubEntity> salida = await _context.PayStubs
                                                               .Include(c => c.PayStubDetails)
                                                               .Include(c => c.WordayClients)
                                                               .Include(c => c.Facilitator)
                                                               .Include(c => c.Doc_Assisstant)

                                                               .AsSplitQuery()
                                                               .ToListAsync();
                    return View(salida);
                }
            }
            if (User.IsInRole("Facilitator"))
            {
                List<PayStubEntity> salida = await _context.PayStubs
                                                           .Include(c => c.PayStubDetails)
                                                           .Include(c => c.WordayClients)
                                                           
                                                           .Include(c => c.Facilitator)
                                                          
                                                           .AsSplitQuery()
                                                           .Where(n => n.Facilitator.LinkedUser == user_logged.UserName)
                                                           .ToListAsync();
                return View(salida);
            }
            if (User.IsInRole("Documents_Assistant"))
            {
                List<PayStubEntity> salida = await _context.PayStubs
                                                           .Include(c => c.PayStubDetails)
                                                         
                                                           .Include(c => c.Bios)
                                                           .Include(c => c.Brief)
                                                           .Include(c => c.MTPs)
                                                           .Include(c => c.MedicalHistory)
                                                           .Include(c => c.Fars)
                                                           .Include(c => c.Facilitator)
                                                           .Include(c => c.Doc_Assisstant)

                                                           .AsSplitQuery()
                                                           .Where(n => n.Doc_Assisstant.LinkedUser == user_logged.UserName)
                                                           .ToListAsync();
                return View(salida);
            }


            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager,  Facilitator")]
        private DocumentsPendingByPayStubViewModel PayStubDetailsByDate(DateTime datePayclose)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            DocumentsPendingByPayStubViewModel model = new DocumentsPendingByPayStubViewModel();
            List<Workday_Client> NoteUnPaid = new List<Workday_Client>();
            List<BioEntity> BioUnPaid = new List<BioEntity>();
            List<MTPEntity> MtpUnPaid = new List<MTPEntity>();
            List<FarsFormEntity> FarsUnPaid = new List<FarsFormEntity>();
            List<IntakeMedicalHistoryEntity> MedicalHistoryUnPaid = new List<IntakeMedicalHistoryEntity>();

            if (User.IsInRole("Manager"))
            {
                NoteUnPaid = _context.Workdays_Clients
                                     .Include(n => n.IndividualNote)
                                     .Include(n => n.Client)
                                     .Include(n => n.Facilitator)
                                     .Include(n => n.Workday)

                                     .Where(n => n.PayStub == null
                                              && n.Workday.Date <= datePayclose
                                              && n.Workday.Service == ServiceType.Individual
                                              && n.IndividualNote != null)
                                     .AsSplitQuery()
                                     .ToList();
            }
            else
            {
                if (User.IsInRole("Facilitator"))
                {
                    NoteUnPaid = _context.Workdays_Clients
                                         .Include(n => n.IndividualNote)
                                         .Include(n => n.Client)
                                         .Include(n => n.Facilitator)
                                         .Include(n => n.Workday)

                                         .Where(n => n.PayStub == null
                                                  && n.Workday.Date <= datePayclose
                                                  && n.Workday.Service == ServiceType.Individual
                                                  && n.Facilitator.LinkedUser == user_logged.UserName
                                                  && n.IndividualNote != null)
                                         .AsSplitQuery()
                                         .ToList();
                }
            }

            int Unit = 0;
            int UnitTotal = 0;
            decimal amount = 0.00m;
            decimal amountTotal = 0.00m;

            List<int> idFacilitators = new List<int>();
            foreach (var item in NoteUnPaid.GroupBy(n => n.Facilitator))
            {
                Unit = NoteUnPaid.Where(f => f.Facilitator.Id == item.Key.Id).Sum(n => n.IndividualNote.RealUnits);
                amount = (decimal)(Unit * item.Key.Money / 4);
                if (idFacilitators.Contains(item.Key.Id) == false)
                {
                    idFacilitators.Add(item.Key.Id);
                }
                UnitTotal += Unit;
                amountTotal += amount;
            }

            model = new DocumentsPendingByPayStubViewModel
            {
                DatePayStub = DateTime.Today,
                DatePayStubClose = datePayclose,
                AmountNote = NoteUnPaid.Count(),
                Amount = (decimal)(amountTotal),
                Units = UnitTotal,
                WorkdaysClientList = NoteUnPaid,
                CantFacilitators = idFacilitators.Count()
            };

            return model;

        }

        [Authorize(Roles = "Manager,  Documents_Assistant")]
        private DocumentsPendingByPayStubViewModel PayStubDetailsDoc_AssisstantByDate(DateTime datePayclose)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            DocumentsPendingByPayStubViewModel model = new DocumentsPendingByPayStubViewModel();
            List<BioEntity> BioUnPaid = new List<BioEntity>();
            List<MTPEntity> MtpUnPaid = new List<MTPEntity>();
            List<FarsFormEntity> FarsUnPaid = new List<FarsFormEntity>();
            List<IntakeMedicalHistoryEntity> MedicalHistoryUnPaid = new List<IntakeMedicalHistoryEntity>();
            List<DocumentsByPaystubsDocumentAssisstantViewModel> documentPendingList = new List<DocumentsByPaystubsDocumentAssisstantViewModel>();
            DocumentsByPaystubsDocumentAssisstantViewModel documentPending = new DocumentsByPaystubsDocumentAssisstantViewModel();

            if (User.IsInRole("Manager"))
            {
                BioUnPaid = _context.Bio
                                    .Include(n => n.Client)
                                    .Include(n => n.DocumentsAssistant)
                                    .Where(n => n.PayStub == null
                                              && n.DateBio <= datePayclose)
                                    .AsSplitQuery()
                                    .ToList();
                MtpUnPaid = _context.MTPs
                                    .Include(n => n.Client)
                                    .Include(n => n.DocumentAssistant)
                                    .Where(n => n.PayStub == null
                                             && n.AdmissionDateMTP <= datePayclose)
                                    .AsSplitQuery()
                                    .ToList();
                FarsUnPaid = _context.FarsForm
                                     .Include(n => n.Client)
                                     
                                     .Where(n => n.PayStub == null
                                              && n.EvaluationDate <= datePayclose
                                              && n.Type <= FARSType.Initial)
                                     .AsSplitQuery()
                                     .ToList();
                MedicalHistoryUnPaid = _context.IntakeMedicalHistory
                                               .Include(n => n.Client)
                                               
                                               .Where(n => n.PayStub == null
                                                        && n.DateSignatureEmployee <= datePayclose)
                                               .AsSplitQuery()
                                               .ToList();
            }
            else
            {
                if (User.IsInRole("Documents_Assistant"))
                {
                    BioUnPaid = _context.Bio
                                        .Include(n => n.Client)
                                        .Include(n => n.DocumentsAssistant)
                                        .Where(n => n.PayStub == null
                                                 && n.DateBio <= datePayclose
                                                 && n.CreatedBy == user_logged.UserName)
                                        .AsSplitQuery()
                                        .ToList();
                    MtpUnPaid = _context.MTPs
                                        .Include(n => n.Client)
                                        .Include(n => n.DocumentAssistant)

                                        .Where(n => n.PayStub == null
                                                 && n.AdmissionDateMTP <= datePayclose
                                                 && n.CreatedBy == user_logged.UserName)
                                        .AsSplitQuery()
                                        .ToList();
                    FarsUnPaid = _context.FarsForm
                                         .Include(n => n.Client)
                                         
                                         .Where(n => n.PayStub == null
                                                  && n.EvaluationDate <= datePayclose
                                                  && n.CreatedBy == user_logged.UserName
                                                  && n.Type <= FARSType.Initial)
                                         .AsSplitQuery()
                                         .ToList();
                    MedicalHistoryUnPaid = _context.IntakeMedicalHistory
                                                   .Include(n => n.Client)

                                                   .Where(n => n.PayStub == null
                                                            && n.DateSignatureEmployee <= datePayclose
                                                            && n.CreatedBy == user_logged.UserName)
                                                   .AsSplitQuery()
                                                   .ToList();
                }
            }

            int Unit = 0;
            int UnitTotal = 0;
            decimal amount = 0.00m;
            decimal amountTotal = 0.00m;

            List<int> idDocumentAssisstants = new List<int>();
            foreach (var item in BioUnPaid)
            {
                if (item.DocumentsAssistant != null)
                {
                    Unit = item.Units;
                    amount = (decimal)(Unit * item.DocumentsAssistant.Money / 4);
                    if (idDocumentAssisstants.Contains(item.DocumentsAssistant.Id) == false)
                    {
                        idDocumentAssisstants.Add(item.DocumentsAssistant.Id);
                    }
                    UnitTotal += Unit;
                    amountTotal += amount;

                    documentPending.Units = Unit;
                    documentPending.Document = "BIO";
                    documentPending.Amount = amount;
                    documentPending.ClientName = item.Client.Name;
                    documentPending.DateService = (DateTime)item.DateBio;
                    documentPending.DateBill = (item.BilledDate != null)? (DateTime)item.BilledDate : null;
                    documentPending.EmployeeName = item.DocumentsAssistant.Name;
                    documentPending.Id = item.Id;
                    documentPending.Status = item.Status.ToString();
                    documentPending.Money = item.DocumentsAssistant.Money;
                    documentPending.IdDocumentAssisstant = item.DocumentsAssistant.Id;
                    documentPending.DocumentAssisstant = item.DocumentsAssistant;


                    documentPendingList.Add(documentPending);
                    documentPending = new DocumentsByPaystubsDocumentAssisstantViewModel();
                }                
            }
            foreach (var item in MtpUnPaid)
            {
                if (item.DocumentAssistant != null)
                {
                    Unit = item.Units;
                    amount = (decimal)(Unit * item.DocumentAssistant.Money / 4);
                    if (idDocumentAssisstants.Contains(item.DocumentAssistant.Id) == false)
                    {
                        idDocumentAssisstants.Add(item.DocumentAssistant.Id);
                    }
                    UnitTotal += Unit;
                    amountTotal += amount;

                    documentPending.Units = Unit;
                    documentPending.Document = "MTP";
                    documentPending.Amount = amount;
                    documentPending.ClientName = item.Client.Name;
                    documentPending.DateService = (DateTime)item.AdmissionDateMTP;
                    documentPending.DateBill = (item.BilledDate != null) ? (DateTime)item.BilledDate : null;
                    documentPending.EmployeeName = item.DocumentAssistant.Name;
                    documentPending.Id = item.Id;
                    documentPending.Status = item.Status.ToString();
                    documentPending.Money = item.DocumentAssistant.Money;
                    documentPending.IdDocumentAssisstant = item.DocumentAssistant.Id;
                    documentPending.DocumentAssisstant = item.DocumentAssistant;


                    documentPendingList.Add(documentPending);
                    documentPending = new DocumentsByPaystubsDocumentAssisstantViewModel();
                }
            }
            foreach (var item in FarsUnPaid)
            {
                DocumentsAssistantEntity docAssisstant = _context.DocumentsAssistant.FirstOrDefault(n => n.LinkedUser == item.CreatedBy);

                if (docAssisstant != null)
                {
                    Unit = item.Units;
                    amount = (decimal)(Unit * docAssisstant.Money / 4);
                    if (idDocumentAssisstants.Contains(docAssisstant.Id) == false)
                    {
                        idDocumentAssisstants.Add(docAssisstant.Id);
                    }
                    UnitTotal += Unit;
                    amountTotal += amount;

                    documentPending.Units = Unit;
                    documentPending.Document = "FARS";
                    documentPending.Amount = amount;
                    documentPending.ClientName = item.Client.Name;
                    documentPending.DateService = (DateTime)item.EvaluationDate;
                    documentPending.DateBill = (item.BilledDate != null)? (DateTime)item.BilledDate : null;
                    documentPending.EmployeeName = item.AdmissionedFor;
                    documentPending.Id = item.Id;
                    documentPending.Status = item.Status.ToString();
                    documentPending.Money = docAssisstant.Money;
                    documentPending.IdDocumentAssisstant = docAssisstant.Id;
                    documentPending.DocumentAssisstant = docAssisstant;

                    documentPendingList.Add(documentPending);
                    documentPending = new DocumentsByPaystubsDocumentAssisstantViewModel();
                }
            }
            foreach (var item in MedicalHistoryUnPaid)
            {
                DocumentsAssistantEntity docAssisstant = _context.DocumentsAssistant.FirstOrDefault(n => n.LinkedUser == item.CreatedBy);

                if (docAssisstant != null)
                {
                    Unit = CalcularUnits((item.EndTime - item.StartTime).Minutes);
                    amount = (decimal)(Unit * docAssisstant.Money / 4);
                    if (idDocumentAssisstants.Contains(docAssisstant.Id) == false)
                    {
                        idDocumentAssisstants.Add(docAssisstant.Id);
                    }
                    UnitTotal += Unit;
                    amountTotal += amount;

                    documentPending.Units = Unit;
                    documentPending.Document = "Medical History";
                    documentPending.Amount = amount;
                    documentPending.ClientName = item.Client.Name;
                    //documentPending.DateBill = (DateTime)item.BilledDate;
                    documentPending.DateService = (DateTime)item.DateSignatureEmployee;
                    documentPending.EmployeeName = item.AdmissionedFor;
                    documentPending.Id = item.Id;
                    documentPending.Status = "Create";
                    documentPending.Money = docAssisstant.Money;
                    documentPending.IdDocumentAssisstant = docAssisstant.Id;
                    documentPending.DocumentAssisstant = docAssisstant;

                    documentPendingList.Add(documentPending);
                    documentPending = new DocumentsByPaystubsDocumentAssisstantViewModel();
                }
            }

            model = new DocumentsPendingByPayStubViewModel
            {
                DatePayStub = DateTime.Today,
                DatePayStubClose = datePayclose,
                AmountNote = BioUnPaid.Count() + MtpUnPaid.Count() + FarsUnPaid.Count() + MedicalHistoryUnPaid.Count(),
                Amount = (decimal)(amountTotal),
                Units = UnitTotal,
                //WorkdaysClientList = NoteUnPaid,
                DocumentsPending = documentPendingList,
                IdDocumentAssisstant = documentPending.IdDocumentAssisstant,
                CantDocumentsAssisstant = idDocumentAssisstants.Count()
            };

            return model;

        }

        [Authorize(Roles = "Manager, Facilitator, Documents_Assistant")]
        public IActionResult PendingByPayStub(DateTime datePayStubclose, int facilitator = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            DocumentsPendingByPayStubViewModel model = new DocumentsPendingByPayStubViewModel();
         
            if ((User.IsInRole("Manager") && facilitator == 1) || User.IsInRole("Facilitator"))
            {
                model = PayStubDetailsByDate(datePayStubclose);
                ViewData["facilitator"] = 1;
                return View(model);
            }
            if ((User.IsInRole("Manager") && facilitator == 0) || User.IsInRole("Documents_Assistant"))
            {
                model = PayStubDetailsDoc_AssisstantByDate(datePayStubclose);
                ViewData["facilitator"] = 0;
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> PendingByPayStub(DateTime datePayStubclose, DocumentsPendingByPayStubViewModel payStubViewModel, int facilitator = 0)
        {
            if (ModelState.IsValid)
            {
                DocumentsPendingByPayStubViewModel model = new DocumentsPendingByPayStubViewModel();
                if (facilitator == 1)
                {
                    model = PayStubDetailsByDate(payStubViewModel.DatePayStubClose);

                    if (model.WorkdaysClientList.Count() > 0)
                    {
                        foreach (var employee in model.WorkdaysClientList.GroupBy(n => n.Facilitator).ToList())
                        {
                            model.IdFacilitator = employee.Key.Id;
                            DocumentsPendingByPayStubViewModel modelTemp = new DocumentsPendingByPayStubViewModel
                            {
                                Amount = model.Amount,
                                DatePayStub = model.DatePayStub,
                                DatePayStubClose = model.DatePayStubClose,
                                DatePayStubPayment = model.DatePayStubPayment,
                                IdFacilitator = model.IdFacilitator,
                                IdStatus = model.IdStatus,
                                WorkdaysClientList = new List<Workday_Client>(),
                                PaystubDetails = new List<PayStubDetailsEntity>(),
                                Units = model.Units
                            };
                            List<PayStubDetailsEntity> PaystubDetailsList = new List<PayStubDetailsEntity>();

                            foreach (var item in model.WorkdaysClientList.Where(n => n.Facilitator.Id == employee.Key.Id))
                            {

                                PayStubDetailsEntity PaystubDetails = new PayStubDetailsEntity();
                                PaystubDetails.DateService = item.Workday.Date;
                                PaystubDetails.IdFacilitator = employee.Key.Id;
                                PaystubDetails.IdWorkdayClient = item.Id;
                                PaystubDetails.ClientName = item.Client.Name;

                                PaystubDetails.Unit = item.IndividualNote.RealUnits;
                                PaystubDetails.Amount = (decimal)(item.IndividualNote.RealUnits * item.Facilitator.Money / 4);
                                PaystubDetails.DocumentName = "Ind. Therapy";

                                PaystubDetailsList.Add(PaystubDetails);
                                modelTemp.WorkdaysClientList.Add(item);
                            }
                            modelTemp.PaystubDetails = PaystubDetailsList;
                            modelTemp.Units = PaystubDetailsList.Sum(n => n.Unit);
                            modelTemp.Amount = PaystubDetailsList.Sum(n => n.Amount);


                            PayStubEntity paystub = _converterHelper.ToPayStubEntity(modelTemp, true);
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
                }
                else
                {
                    List<BioEntity> bioList = new List<BioEntity>();
                    List<MTPEntity> mtpList = new List<MTPEntity>();
                    List<FarsFormEntity> farsList = new List<FarsFormEntity>();
                    List<IntakeMedicalHistoryEntity> medicalHistoryList = new List<IntakeMedicalHistoryEntity>();

                    model = PayStubDetailsDoc_AssisstantByDate(payStubViewModel.DatePayStubClose);
                    if (model.DocumentsPending.Count() > 0)
                    {
                        BioEntity bio = new BioEntity();
                        MTPEntity mtp = new MTPEntity();
                        FarsFormEntity fars = new FarsFormEntity();
                        IntakeMedicalHistoryEntity medicalHistory = new IntakeMedicalHistoryEntity();

                        foreach (var employee in model.DocumentsPending.GroupBy(n => n.DocumentAssisstant).ToList())
                        {
                            List<string> clients = new List<string>();
                            model.IdDocumentAssisstant = employee.Key.Id;
                            DocumentsPendingByPayStubViewModel modelTemp = new DocumentsPendingByPayStubViewModel
                            {
                                Amount = model.Amount,
                                DatePayStub = model.DatePayStub,
                                DatePayStubClose = model.DatePayStubClose,
                                DatePayStubPayment = model.DatePayStubPayment,
                                IdDocumentAssisstant = model.IdDocumentAssisstant,
                                IdStatus = model.IdStatus,
                                DocumentsPending = new List<DocumentsByPaystubsDocumentAssisstantViewModel>(),
                                PaystubDetails = new List<PayStubDetailsEntity>(),
                                Units = model.Units
                            };
                            List<PayStubDetailsEntity> PaystubDetailsList = new List<PayStubDetailsEntity>();

                            foreach (var item in model.DocumentsPending.Where(n => n.DocumentAssisstant.Id == employee.Key.Id))
                            {

                                PayStubDetailsEntity PaystubDetails = new PayStubDetailsEntity();
                                PaystubDetails.DateService = item.DateService;
                                PaystubDetails.IdDocuAssisstant = employee.Key.Id;
                                PaystubDetails.ClientName = item.ClientName;

                                PaystubDetails.Unit = item.Units;
                                PaystubDetails.Amount = (decimal)(item.Units * item.DocumentAssisstant.Money / 4);
                                PaystubDetails.DocumentName = item.Document;

                                if (item.Document == "Medical History")
                                {
                                    medicalHistory = _context.IntakeMedicalHistory.FirstOrDefault(n => n.Id == item.Id);
                                    if (medicalHistory != null)
                                        medicalHistoryList.Add(medicalHistory);
                                }
                                if (item.Document == "BIO")
                                {
                                    bio = _context.Bio.FirstOrDefault(n => n.Id == item.Id);
                                    if (bio != null)
                                        bioList.Add(bio);
                                }
                                if (item.Document == "FARS")
                                {
                                    fars = _context.FarsForm.FirstOrDefault(n => n.Id == item.Id);
                                    if (fars != null)
                                        farsList.Add(fars);
                                }
                                if (item.Document == "MTP")
                                {
                                    mtp = _context.MTPs.FirstOrDefault(n => n.Id == item.Id);
                                    if (mtp != null)
                                        mtpList.Add(mtp);
                                }

                                if (clients.Contains(item.ClientName) == false)
                                {
                                    clients.Add(item.ClientName);
                                }

                                PaystubDetailsList.Add(PaystubDetails);
                                modelTemp.DocumentsPending.Add(item);
                            }
                            modelTemp.PaystubDetails = PaystubDetailsList;
                            modelTemp.Units = PaystubDetailsList.Sum(n => n.Unit);
                            modelTemp.Amount = PaystubDetailsList.Sum(n => n.Amount);
                            modelTemp.MtpList = mtpList;
                            modelTemp.BioList = bioList;
                            modelTemp.FarsList = farsList;
                            modelTemp.MedicalHistoryList = medicalHistoryList ;

                            PayStubEntity paystub = _converterHelper.ToPayStubEntity(modelTemp, true);
                            paystub.CantClient = clients.Count();
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
                }
               
            }
            return RedirectToAction("PendingByPayStub", new { dateTime = datePayStubclose });
        }

        [Authorize(Roles = "Manager, Facilitator, Documents_Assistant")]
        public IActionResult PaystubDetails(int id = 0, int facilitador = 0)
        {
            if ((User.IsInRole("Manager") && facilitador == 1)|| User.IsInRole("Facilitator"))
            {
                PayStubEntity entity = _context.PayStubs
                                               .Include(n => n.PayStubDetails)
                                               .Include(n => n.Facilitator)
                                         
                                               .FirstOrDefault(n => n.Id == id);
                return View(entity);
            }
            if ((User.IsInRole("Manager") && facilitador == 0) || User.IsInRole("Documents_Assistant"))
            {
                PayStubEntity entity = _context.PayStubs
                                               .Include(n => n.PayStubDetails)
                                         
                                               .Include(n => n.Doc_Assisstant)
                                               .FirstOrDefault(n => n.Id == id);
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

            PayStubEntity entity = await _context.PayStubs
                                                 .Include(n => n.PayStubDetails)
                                                 .Include(n => n.Facilitator)
                                                 .Include(n => n.Doc_Assisstant)
                                                 .FirstOrDefaultAsync(f => f.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            PaystubPaidViewModel model = new PaystubPaidViewModel();
            if (User.IsInRole("Manager"))
            {
                model = new PaystubPaidViewModel()
                {
                    IdPaystub = entity.Id,
                    Amount = entity.Amount,
                    IdStatus = Convert.ToInt32(entity.StatusPayStub),
                    StatusList = _combosHelper.GetComboPaystubStatus(),
                    DatePayStubPayment = entity.DatePayStubPayment,
                    Facilitator = entity.Facilitator,
                    Doc_Assisstant = entity.Doc_Assisstant,
                    DatePayStubClose = entity.DatePayStubClose,
                    DatePayStub = entity.DatePayStub
                };
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaid(int id, PaystubPaidViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                PayStubEntity entity = await _context.PayStubs.FirstOrDefaultAsync(n => n.Id == model.Id);

                if (entity != null)
                {
                    entity.StatusPayStub = StatusTCMPaystubUtils.GetStatusBillByIndex(model.IdStatus);
                    entity.DatePayStubPayment = model.DatePayStubPayment;

                    _context.Update(entity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        List<PayStubEntity> salida = await _context.PayStubs
                                                                   .Include(c => c.PayStubDetails)
                                                                   .Include(c => c.WordayClients)
                                                                   .Include(c => c.Facilitator)
                                                                   .Include(c => c.Doc_Assisstant)
                                                                   .AsSplitQuery()
                                                                   .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewPayStubs", salida) });

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

        [Authorize(Roles = "Manager, Facilitator, Documents_Assistant")]
        public IActionResult EXCEL(int id)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            PayStubEntity payStub = _context.PayStubs
                                            .Include(n => n.PayStubDetails)
                                            .Include(n => n.Facilitator)
                                            .Include(n => n.Doc_Assisstant)
                                            .FirstOrDefault(n => n.Id == id);

            string Periodo = "Invoice until: " + payStub.DatePayStubClose.ToShortDateString();
            string ReportName = "Invoice " + user_logged.Clinic.Name + " " + payStub.DatePayStubClose.ToShortDateString() + ".xlsx";
            string data = "BILL";

            if (payStub.Facilitator != null)
            {
                byte[] content = _exportExcelHelper.ExportPayStubFacilitatorHelper(payStub, Periodo, _context.Clinics.FirstOrDefault().Name, data);

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ReportName);
            }
            else
            {
                if (payStub.Doc_Assisstant != null)
                {
                    byte[] content = _exportExcelHelper.ExportPayStubDocAssisstantHelper(payStub, Periodo, _context.Clinics.FirstOrDefault().Name, data);

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ReportName);
                }
                else
                {
                    return null;
                }
            }
        }

        public int CalcularUnits(int minutes)
        {
            int units = minutes / 15;
            int residuo = minutes % 15;

            if (residuo > 7)
                return (units + 1);
            else
                return (units);
        }
    }
}

