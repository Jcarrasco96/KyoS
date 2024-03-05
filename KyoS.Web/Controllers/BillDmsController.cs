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
    public class BillDmsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        //private readonly IImageHelper _imageHelper;
        private readonly IMimeType _mimeType;
        private readonly IExportExcellHelper _exportExcelHelper;
        private readonly IFileHelper _fileHelper;
        private readonly IReportHelper _reportHelper;
        //private readonly IWebHostEnvironment _webhostEnvironment;

        public IConfiguration Configuration { get; }

        public BillDmsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType, IExportExcellHelper exportExcelHelper, IFileHelper fileHelper, IReportHelper reportHelper, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
           // _imageHelper = imageHelper;
            _mimeType = mimeType;
            _exportExcelHelper = exportExcelHelper;
            _fileHelper = fileHelper;
            _reportHelper = reportHelper;
            //_webhostEnvironment = webHostEnvironment;
            Configuration = configuration;
        }

        [Authorize(Roles = "Manager, Admin")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("Admin"))
            {
                List<BillDmsEntity> salida = await _context.BillDms
                                                            .Include(c => c.BillDmsDetails)
                                                            .Include(c => c.BillDmsPaids)
                                                            .ToListAsync();
                return View(salida);
            }

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }
            if (User.IsInRole("Manager"))
            {
                List<BillDmsEntity> salida = await _context.BillDms
                                                           .Include(c => c.BillDmsDetails)
                                                           .Include(c => c.BillDmsPaids)
                                                           .ToListAsync();
                return View(salida);
            }
           


            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, Admin")]
        public IActionResult Create(DateTime dateBillclose)
        {
            BillDmsViewModel model = new BillDmsViewModel();


            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                model = BillDetailsByDate(dateBillclose);
                model.IdStatus = 1;
                return View(model);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(DateTime dateTime, BillDmsViewModel billDmsViewModel)
        {
            if (ModelState.IsValid)
            {
                BillDmsViewModel model = BillDetailsByDate(billDmsViewModel.DateBillClose, StatusBill.Billed);
                BillDmsEntity BillDms = await _context.BillDms.FirstOrDefaultAsync(f => f.DateBillClose == dateTime);
                if (BillDms == null)
                {
                    model.StatusBill = StatusBill.Billed;
                    BillDmsEntity billDmsEntity = _converterHelper.ToBillDMSEntity(model, true);

                    _context.Add(billDmsEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the Bil with close date: {billDmsViewModel.DateBillClose}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Create", new { dateTime = dateTime });
                }
            }
            return RedirectToAction("Create", new { dateTime = dateTime });
        }

        [Authorize(Roles = "Manager, Admin")]
        private BillDmsViewModel BillDetailsByDate(DateTime dateBillclose, StatusBill status = StatusBill.Unbilled)
        {
            BillDmsViewModel model = new BillDmsViewModel();
            BillDmsDetailsViewModel billDmsDetailsTemp = new BillDmsDetailsViewModel();
            List<BillDmsDetailsEntity> billDmsDetailsList = new List<BillDmsDetailsEntity>();

            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                List<Workday_Client> workdayClientUnbilled = _context.Workdays_Clients
                                                                     .Include(n => n.Workday)
                                                                     .Include(n => n.Client)
                                                                     .Include(n => n.Note)
                                                                     .ThenInclude(n => n.Notes_Activities)
                                                                     .Include(n => n.NoteP)
                                                                     .ThenInclude(n => n.NotesP_Activities)
                                                                     .Include(n => n.GroupNote)
                                                                     .ThenInclude(n => n.GroupNotes_Activities)
                                                                     .Include(n => n.GroupNote2)
                                                                     .ThenInclude(n => n.GroupNotes2_Activities)
                                                                     .Include(n => n.IndividualNote)
                                                                     .Include(n => n.Schedule)
                                                                     .ThenInclude(n => n.SubSchedules)
                                                                     .Where(n => n.BillDms == null
                                                                              && n.Client != null
                                                                              && n.Present == true
                                                                              && n.Workday.Date <= dateBillclose)
                                                                     .ToList();

                int Unit = 0;
                int UnitTotal = 0;
                decimal amount = 0.00m;
                int notes = 0;

                foreach (var item in workdayClientUnbilled)
                {
                    if (item.Note != null)
                    {
                        Unit = 16;
                        amount = (decimal)(16 * 0.2);
                        notes++;
                    }
                    else
                    {
                        if (item.NoteP != null)
                        {
                            Unit = item.NoteP.RealUnits;
                            amount = (decimal)(item.NoteP.RealUnits * 0.2);
                            notes++;
                        }
                        else
                        {
                            if (item.IndividualNote != null)
                            {
                                Unit = 4;
                                amount = (decimal)(4 * 0.2);
                                notes++;
                            }
                            else
                            {
                                if (item.GroupNote2 != null)
                                {
                                    Unit = item.GroupNote2.GroupNotes2_Activities.Count() * 4;
                                    amount = (decimal)(item.GroupNote2.GroupNotes2_Activities.Count() * 4 * 0.2);
                                    notes++;
                                }
                                else
                                {
                                    Unit = item.Schedule.SubSchedules.Count() * 4;
                                    amount = (decimal)(item.Schedule.SubSchedules.Count() * 4 * 0.2);
                                    notes++;
                                }

                            }
                        }
                    }

                    billDmsDetailsTemp = new BillDmsDetailsViewModel()
                    {
                        DateService = item.Workday.Date,
                        IdCLient = item.Client.Id,
                        IdWorkddayClient = item.Id,
                        ServiceAgency = ServiceAgency.CMH,
                        Unit = Unit,
                        StatusBill = status,
                        NameClient = item.Client.Name,
                        Amount = amount

                    };

                    if (item.BilledDate == null)
                    {
                        billDmsDetailsTemp.MedicaidBill = false;
                    }
                    else
                    {
                        billDmsDetailsTemp.MedicaidBill = true;
                    }

                    UnitTotal += Unit;
                    Unit = 0;
                    amount = 0;
                    billDmsDetailsList.Add(billDmsDetailsTemp);
                }

                List<TCMNoteEntity> tcmNotesUnbilled = _context.TCMNote
                                                               .Include(n => n.TCMNoteActivity)
                                                               .Include(n => n.TCMClient)
                                                               .ThenInclude(n => n.Client)
                                                               .Where(n => n.BillDms == null
                                                                     && n.DateOfService <= dateBillclose)
                                                               .ToList();
                int UnitMH = UnitTotal;
                int minutesTCM = 0;
                int residuoTCM = 0;
                int unitTCM = 0;
                int unitTCMEnd = 0;
                foreach (var item in tcmNotesUnbilled)
                {
                    minutesTCM = item.TCMNoteActivity.Sum(n => n.Minutes);
                    unitTCM = minutesTCM / 15;
                    residuoTCM = minutesTCM % 15;
                    if (residuoTCM > 7)
                    {
                        Unit = unitTCM + 1;
                        amount = (unitTCM + 1) / 5;
                    }
                    else
                    {
                        Unit = unitTCM;
                        amount = (decimal)(unitTCM * 0.2);
                    }

                    billDmsDetailsTemp = new BillDmsDetailsViewModel()
                    {
                        DateService = item.DateOfService,
                        ServiceAgency = ServiceAgency.TCM,
                        Unit = Unit,
                        IdTCMNotes = item.Id,
                        StatusBill = status,
                        NameClient = item.TCMClient.Client.Name,
                        IdCLient = item.TCMClient.Client.Id,
                        Amount = (decimal)(Unit * 0.2)

                    };
                    UnitTotal += Unit;
                    unitTCMEnd += Unit;
                    billDmsDetailsList.Add(billDmsDetailsTemp);
                }
                model = new BillDmsViewModel
                {
                    DateBill = DateTime.Today,
                    DateBillClose = dateBillclose,
                    AmountCMHNotes = workdayClientUnbilled.Count(),
                    AmountTCMNotes = tcmNotesUnbilled.Count(),
                    Amount = (decimal)(UnitTotal * 0.2),
                    Units = UnitTotal,
                    Different = 0,
                    StatusBill = StatusBill.Unbilled,
                    Workday_Clients = workdayClientUnbilled,
                    TCMNotes = tcmNotesUnbilled,
                    BillDmsDetails = billDmsDetailsList,
                    UnitsMH = UnitMH,
                    UnitsTCM = unitTCMEnd

                };
                return model;
            }
            return model;
        }

        [Authorize(Roles = "Manager, Admin")]
        public IActionResult BillDetails(int id = 0)
        {
            BillDmsViewModel model = new BillDmsViewModel();
            BillDmsEntity entity = _context.BillDms
                                           .Include(n => n.BillDmsDetails)
                                           .FirstOrDefault(n => n.Id == id);
            model = _converterHelper.ToBillDMSViewModel(entity);
            
            if (User.IsInRole("Manager") || User.IsInRole("Admin"))
            {
                return View(model);
            }

            return null;
        }
       
        [Authorize(Roles = "Manager, Admin")]
        public async Task<IActionResult> FinishEdition(int idBill = 0)
        {
            if (ModelState.IsValid)
            {
                BillDmsEntity BillDms = await _context.BillDms.FirstOrDefaultAsync(f => f.Id == idBill);
                if (BillDms != null)
                {
                    BillDms.FinishEdition = true;
                   
                    _context.Update(BillDms);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("BillDetails", new { id = idBill });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the Bil with close date: {BillDms.DateBillClose}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    return RedirectToAction("BillDetails", new { id = idBill });
                }
            }
            return RedirectToAction("BillDetails", new { id = idBill });
        }

        [Authorize(Roles = "Manager, Admin")]
        public async Task<IActionResult> ToPending(int id = 0)
        {
            BillDmsDetailsEntity detailEntity = await _context.BillDmsDetails
                                                                     .Include(n => n.Bill)
                                                                     .FirstOrDefaultAsync(n => n.Id == id);
            BillDmsEntity billDmsEntity = await _context.BillDms.FirstOrDefaultAsync(n => n.Id == detailEntity.Bill.Id);

            if (ModelState.IsValid)
            {   
                if (detailEntity != null)
                {
                    detailEntity.StatusBill = StatusBill.Pending;
                    
                    _context.Update(detailEntity);
                    billDmsEntity.Different += (decimal)(detailEntity.Unit * 0.2);
                    billDmsEntity.Amount -= (decimal)(detailEntity.Unit * 0.2);
                    _context.Update(billDmsEntity);

                    Workday_Client workday_Client = _context.Workdays_Clients.FirstOrDefault(n => n.Id == detailEntity.IdWorkddayClient);
                    if (workday_Client != null)
                    {
                        workday_Client.Hold = true;
                        _context.Update(workday_Client);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("BillDetails", new { id = billDmsEntity.Id });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already not exists the Bil");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    return RedirectToAction("BillDetails", new { id = billDmsEntity.Id });
                }
            }
            return RedirectToAction("BillDetails", new { id = billDmsEntity.Id });
        }

        [Authorize(Roles = "Manager, Admin")]
        public async Task<IActionResult> ToBill(int id = 0)
        {
            BillDmsDetailsEntity detailEntity = await _context.BillDmsDetails
                                                                 .Include(n => n.Bill)
                                                                 .FirstOrDefaultAsync(n => n.Id == id);

            BillDmsEntity billDmsEntity = await _context.BillDms.FirstOrDefaultAsync(n => n.Id == detailEntity.Bill.Id);

            if (ModelState.IsValid)
            {
                if (detailEntity != null)
                {
                    detailEntity.StatusBill = StatusBill.Billed;
                    billDmsEntity.Different -= (decimal)(detailEntity.Unit * 0.2);
                    billDmsEntity.Amount += (decimal)(detailEntity.Unit * 0.2);
                    _context.Update(detailEntity);

                    Workday_Client workday_Client = _context.Workdays_Clients.FirstOrDefault(n => n.Id == detailEntity.IdWorkddayClient);
                    if (workday_Client != null)
                    {
                        workday_Client.Hold = false;
                        _context.Update(workday_Client);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("BillDetails", new { id = billDmsEntity.Id });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already not exists the Bil");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    return RedirectToAction("BillDetails", new { id = billDmsEntity.Id });
                }
            }
            return RedirectToAction("BillDetails", new { id = billDmsEntity.Id });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPaid(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            BillDmsEntity billDmsEntity = await _context.BillDms
                                                        .Include(n => n.BillDmsDetails)
                                                        .FirstOrDefaultAsync(f => f.Id == id);
            if (billDmsEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            BillDmsPaidViewModel billDmsPaidViewModel = new BillDmsPaidViewModel();
            if (User.IsInRole("Admin"))
            {
                billDmsPaidViewModel = new BillDmsPaidViewModel()
                { 
                    IdBillDms = billDmsEntity.Id,
                    Amount = billDmsEntity.Amount,
                    OrigiList = _combosHelper.GetComboBillPaid(),
                    Bill = billDmsEntity,
                    DatePaid = DateTime.Today
                    
                };
            }

            return View(billDmsPaidViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPaid(int id, BillDmsPaidViewModel billDmsPaidViewModel)
        {
            if (id != billDmsPaidViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            List<SelectListItem> list = new List<SelectListItem>();

            if (ModelState.IsValid)
            {
                BillDmsPaidEntity billDmsPaidEntity = _converterHelper.ToBillDMSPaidEntity(billDmsPaidViewModel, true);

                _context.Add(billDmsPaidEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    BillDmsEntity billDmsEntity = await _context.BillDms
                                                          .Include(n => n.BillDmsPaids)
                                                          .FirstOrDefaultAsync(n => n.Id == billDmsPaidViewModel.IdBillDms);

                    if (billDmsEntity.BillDmsPaids.Count() > 0)
                    {
                        if (billDmsEntity.BillDmsPaids.Sum(n => n.Amount) == (billDmsEntity.Amount + billDmsEntity.Different))
                        {
                            billDmsEntity.StatusBill = StatusBill.Paid;
                            billDmsEntity.DateBillPayment = billDmsPaidViewModel.DatePaid;
                        }
                        else
                        {
                            if (billDmsEntity.BillDmsPaids.Sum(n => n.Amount) < (billDmsEntity.Amount + billDmsEntity.Different))
                            {
                                billDmsEntity.StatusBill = StatusBill.Billed;
                                billDmsEntity.DateBillPayment = billDmsPaidViewModel.DatePaid;
                            }

                        }
                    }
                    else
                    {
                        billDmsEntity.StatusBill = StatusBill.Pending;
                        billDmsEntity.DateBillPayment = billDmsPaidViewModel.DatePaid;
                    }

                    _context.BillDms.Update(billDmsEntity);

                    try
                    {
                        await _context.SaveChangesAsync();
                        List<BillDmsEntity> billDms_List = await _context.BillDms
                                                                    .Include(n => n.BillDmsDetails)
                                                                    .Include(n => n.BillDmsPaids)
                                                                    .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewBillDms", billDms_List) });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the Bill: {billDmsPaidViewModel.DatePaid}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Bill: {billDmsPaidViewModel.DatePaid}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            billDmsPaidViewModel.OrigiList = _combosHelper.GetComboBillPaid();

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddPaid", billDmsPaidViewModel) });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditPaid(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            BillDmsPaidEntity billDmsPaidEntity = await _context.BillDmsPaid
                                                                .Include(n => n.Bill)
                                                                .FirstOrDefaultAsync(f => f.Id == id);
            if (billDmsPaidEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            BillDmsPaidViewModel billDmsPaidViewModel = new BillDmsPaidViewModel();
            if (User.IsInRole("Admin"))
            {
                billDmsPaidViewModel = _converterHelper.ToBillDMSPaidModel(billDmsPaidEntity);
            }

            return View(billDmsPaidViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPaid(int id, BillDmsPaidViewModel billDmsPaidViewModel)
        {
            if (id != billDmsPaidViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            List<SelectListItem> list = new List<SelectListItem>();

            if (ModelState.IsValid)
            {
                BillDmsPaidEntity billDmsPaidEntity = _converterHelper.ToBillDMSPaidEntity(billDmsPaidViewModel, false);
                              
                _context.BillDmsPaid.Update(billDmsPaidEntity);
               
                try
                {
                    await _context.SaveChangesAsync();

                    BillDmsEntity billDmsEntity = await _context.BillDms
                                                           .Include(n => n.BillDmsPaids)
                                                           .FirstOrDefaultAsync(n => n.Id == billDmsPaidViewModel.IdBillDms);

                    if (billDmsEntity.BillDmsPaids.Count() > 0)
                    {
                        if (billDmsEntity.BillDmsPaids.Sum(n => n.Amount) == (billDmsEntity.Amount + billDmsEntity.Different))
                        {
                            billDmsEntity.StatusBill = StatusBill.Paid;
                            billDmsEntity.DateBillPayment = billDmsPaidViewModel.DatePaid;
                        }
                        else
                        {
                            if (billDmsEntity.BillDmsPaids.Sum(n => n.Amount) < (billDmsEntity.Amount + billDmsEntity.Different))
                            {
                                billDmsEntity.StatusBill = StatusBill.Billed;
                                billDmsEntity.DateBillPayment = billDmsPaidViewModel.DatePaid;
                            }

                        }
                    }
                    else
                    {
                        billDmsEntity.StatusBill = StatusBill.Pending;
                        billDmsEntity.DateBillPayment = billDmsPaidViewModel.DatePaid;
                    }

                    _context.BillDms.Update(billDmsEntity);

                    try
                    {
                        await _context.SaveChangesAsync();
                        List<BillDmsEntity> billDms_List = await _context.BillDms
                                                                    .Include(n => n.BillDmsDetails)
                                                                    .Include(n => n.BillDmsPaids)
                                                                    .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewBillDms", billDms_List) });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the Bill: {billDmsPaidViewModel.DatePaid}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Bill: {billDmsPaidViewModel.DatePaid}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            billDmsPaidViewModel.OrigiList = _combosHelper.GetComboBillPaid();

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditPaid", billDmsPaidViewModel) });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePaid(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            BillDmsPaidEntity billDmsPaidEntity = await _context.BillDmsPaid
                                                                .Include(n => n.Bill)
                                                                .FirstOrDefaultAsync(t => t.Id == id);
            if (billDmsPaidEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }
            int tempBilldms = billDmsPaidEntity.Bill.Id;
            try
            {
                _context.BillDmsPaid.Remove(billDmsPaidEntity);
                await _context.SaveChangesAsync();

                BillDmsEntity billDmsEntity = await _context.BillDms
                                                            .Include(n => n.BillDmsPaids)
                                                            .FirstOrDefaultAsync(n => n.Id == tempBilldms);

                if (billDmsEntity.BillDmsPaids.Count() > 0)
                {
                    if (billDmsEntity.BillDmsPaids.Sum(n => n.Amount) == (billDmsEntity.Amount + billDmsEntity.Different))
                    {
                        billDmsEntity.StatusBill = StatusBill.Paid;
                        billDmsEntity.DateBillPayment = DateTime.Today;
                    }
                    else
                    {
                        if (billDmsEntity.BillDmsPaids.Sum(n => n.Amount) < (billDmsEntity.Amount + billDmsEntity.Different))
                        {
                            billDmsEntity.StatusBill = StatusBill.Billed;
                            billDmsEntity.DateBillPayment = DateTime.Today;
                        }

                    }
                }
                else
                {
                    billDmsEntity.StatusBill = StatusBill.Pending;
                    billDmsEntity.DateBillPayment = DateTime.Today;
                }

                _context.BillDms.Update(billDmsEntity);

                try
                {
                    await _context.SaveChangesAsync();
                   
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Bill: ");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            catch (System.Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager, Admin")]
        public IActionResult EXCEL(int id)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            BillDmsEntity billDms = _context.BillDms
                                            .Include(n => n.BillDmsDetails)
                                            .FirstOrDefault(n => n.Id == id);

            string Periodo = "Invoice until: " + billDms.DateBillClose.ToShortDateString();
            string ReportName = "Invoice "+ user_logged.Clinic.Name + " "+ billDms.DateBillClose.ToShortDateString() + ".xlsx";
            string data = "BILL";
          
            byte[] content = _exportExcelHelper.ExportBillDmsHelper(billDms, Periodo, _context.Clinics.FirstOrDefault().Name, data);

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ReportName);
            
        }

        [Authorize(Roles = "Manager, Admin")]
        public async Task<IActionResult> UnbilledNotesToPendingPayments(int id = 0)
        {
            List<BillDmsDetailsEntity> detailBill_List = await _context.BillDmsDetails
                                                                       .Include(n => n.Bill)
                                                                       .Where(n => n.Bill.Id == id
                                                                                && n.MedicaidBill == false)
                                                                       .ToListAsync();

            List<BillDmsDetailsEntity> salida = new List<BillDmsDetailsEntity>();
            BillDmsEntity billDmsEntity = await _context.BillDms.FirstOrDefaultAsync(n => n.Id == id);

            if (detailBill_List.Count() > 0)
            {
                foreach (var item in detailBill_List)
                {
                    if (item.StatusBill == StatusBill.Billed)
                    {
                        item.StatusBill = StatusBill.Pending;

                        _context.Update(item);
                        billDmsEntity.Different += (decimal)(item.Unit * 0.2);
                        billDmsEntity.Amount -= (decimal)(item.Unit * 0.2);
                        _context.Update(billDmsEntity);

                        Workday_Client workday_Client = _context.Workdays_Clients.FirstOrDefault(n => n.Id == item.IdWorkddayClient);
                        if (workday_Client != null)
                        {
                            workday_Client.Hold = true;
                            _context.Update(workday_Client);
                        }
                    }
                    
                }
               
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("BillDetails", new { id = billDmsEntity.Id });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already not exists the Bil");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            else
            {
                return RedirectToAction("BillDetails", new { id = billDmsEntity.Id });
            }
            return RedirectToAction("BillDetails", new { id = billDmsEntity.Id });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InternalAccount(int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("Admin"))
            {
                List<BillDmsEntity> salida = await _context.BillDms
                                                            .Include(c => c.BillDmsDetails)
                                                            .Include(c => c.BillDmsPaids)
                                                            .ToListAsync();
                return View(salida);
            }

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }
            if (User.IsInRole("Manager"))
            {
                List<BillDmsEntity> salida = await _context.BillDms
                                                           .Include(c => c.BillDmsDetails)
                                                           .Include(c => c.BillDmsPaids)
                                                           .ToListAsync();
                return View(salida);
            }



            return RedirectToAction("NotAuthorized", "Account");
        }

    }
}
