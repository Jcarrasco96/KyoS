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
using KyoS.Common.Enums;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Manager, TCMSupervisor, CaseManager, Frontdesk")]
    public class HealthInsurancesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IMimeType _mimeType;

        public HealthInsurancesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _imageHelper = imageHelper;
            _mimeType = mimeType;
        }

        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View(await _context.HealthInsurances
                                      .Include(n => n.Client_HealthInsurances)
                                      .ThenInclude(n => n.Client)
                                      .Where(hi => hi.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderBy(c => c.Name).ToListAsync());
        }

        public async Task<IActionResult> Create(int id = 0)
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

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            HealthInsuranceViewModel entity = new HealthInsuranceViewModel()
            {
                SignedDate = DateTime.Now,
                DurationTime = 12,
                Active = true,
                NeedAuthorization = false
            };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HealthInsuranceViewModel healthInsuranceViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                HealthInsuranceEntity insurance = await _context.HealthInsurances
                                                                .FirstOrDefaultAsync(c => (c.Name == healthInsuranceViewModel.Name && c.Clinic.Id == user_logged.Clinic.Id));
                if (insurance == null)
                {
                    string documentPath = string.Empty;
                    if (healthInsuranceViewModel.DocumentFile != null)
                    {
                        documentPath = await _imageHelper.UploadFileAsync(healthInsuranceViewModel.DocumentFile, "Insurances");
                    }                    

                    healthInsuranceViewModel.IdClinic = user_logged.Clinic.Id;

                    HealthInsuranceEntity healthEnsuranceEntity = await _converterHelper.ToHealthInsuranceEntity(healthInsuranceViewModel, true, user_logged.Id, documentPath);
                    _context.Add(healthEnsuranceEntity);

                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the health insurance: {healthEnsuranceEntity.Name}");
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
            return View(healthInsuranceViewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            HealthInsuranceEntity entity = await _context.HealthInsurances
                                                         .Include(hi => hi.Clinic)
                                                         .FirstOrDefaultAsync(hi => hi.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            HealthInsuranceViewModel healthInsuranceViewModel = _converterHelper.ToHealthInsuranceViewModel(entity);            
            return View(healthInsuranceViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HealthInsuranceViewModel healthInsuranceViewModel)
        {
            if (id != healthInsuranceViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                string path = healthInsuranceViewModel.DocumentPath;

                if (healthInsuranceViewModel.DocumentFile != null)
                {
                    path = await _imageHelper.UploadFileAsync(healthInsuranceViewModel.DocumentFile, "Insurances");
                }

                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                HealthInsuranceEntity healthInsuranceEntity = await _converterHelper.ToHealthInsuranceEntity(healthInsuranceViewModel, false, user_logged.Id, path);
                _context.Update(healthInsuranceEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the health insurance: {healthInsuranceEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(healthInsuranceViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            HealthInsuranceEntity healthInsurancesEntity = await _context.HealthInsurances.FirstOrDefaultAsync(hi => hi.Id == id);
            if (healthInsurancesEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.HealthInsurances.Remove(healthInsurancesEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> OpenDocument(int id)
        {
            HealthInsuranceEntity entity = await _context.HealthInsurances
                                                         .FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }
            string mimeType = _mimeType.GetMimeType(entity.DocumentPath);
            return File(entity.DocumentPath, mimeType);
        }

        public async Task<IActionResult> UnitsAvailability(int idError = 0, int agency = 0)
        {
            if (idError == 1) //Imposible to delete 
            {
                ViewBag.Delete = "N";
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return RedirectToAction("Home/Error404");
            }

            List<UnitsAvailabilityViewModel> unitsList = new List<UnitsAvailabilityViewModel>();
            if (agency == 0)
            {
                List<Client_HealthInsurance> list = await _context.Clients_HealthInsurances

                                                              .Include(wc => wc.Client)

                                                              .Include(wc => wc.HealthInsurance)
                                                              .ThenInclude(h => h.Clinic)
                                                              .AsSplitQuery()
                                                              .Where(ch => (ch.HealthInsurance.Clinic.Id == user_logged.Clinic.Id
                                                                         && ch.Agency == ServiceAgencyUtils.GetServiceAgencyByIndex(agency)
                                                                         && ch.Active == true
                                                                         && ch.Client.Status == StatusType.Open))
                                                              .ToListAsync();
                foreach (var item in list)
                {
                    unitsList.Add(new UnitsAvailabilityViewModel
                    {
                        Id = item.Id,
                        ApprovedDate = item.ApprovedDate,
                        DurationTime = item.DurationTime,
                        Units = item.Units,
                        Active = item.Active,
                        Client = item.Client,
                        HealthInsurance = item.HealthInsurance,
                        Expired = (item.ApprovedDate.AddMonths(item.DurationTime) < DateTime.Now) ? true : false,
                        UsedUnits = await this.UsedUnitsPerClient(item.Id, item.Client.Id, item.HealthInsurance.Id),
                        Agency = item.Agency
                    });
                }
            }
            else
            {
                List<TCMClientEntity> list = new List<TCMClientEntity>();
                if (User.IsInRole("TCMSupervisor"))
                {
                    list = await _context.TCMClient
                                         .Include(wc => wc.Client)
                                         .ThenInclude(wc => wc.Clients_HealthInsurances)
                                         .ThenInclude(wc => wc.HealthInsurance)
                                         .Where(ch => (ch.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && ch.Status == StatusType.Open
                                                    && ch.Client.Clients_HealthInsurances.Any(n => n.Active == true
                                                                                        && n.Agency == ServiceAgency.TCM)
                                                    && ch.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName))
                                         .ToListAsync();
                }
                if (User.IsInRole("CaseManager"))
                {
                    list = await _context.TCMClient
                                         .Include(wc => wc.Client)
                                         .ThenInclude(wc => wc.Clients_HealthInsurances)
                                         .ThenInclude(wc => wc.HealthInsurance)
                                         .Where(ch => (ch.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && ch.Status == StatusType.Open
                                                    && ch.Client.Clients_HealthInsurances.Any(n => n.Active == true
                                                                                        && n.Agency == ServiceAgency.TCM)
                                                    && ch.Casemanager.LinkedUser == user_logged.UserName))
                                         .ToListAsync();
                }
                if (User.IsInRole("Manager") || User.IsInRole("Frontdesk"))
                {
                    list = await _context.TCMClient
                                         .Include(wc => wc.Client)
                                         .ThenInclude(wc => wc.Clients_HealthInsurances)
                                         .ThenInclude(wc => wc.HealthInsurance)
                                         .Where(ch => (ch.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && ch.Client.Clients_HealthInsurances.Any(n => n.Active == true
                                                                                           && n.Agency == ServiceAgency.TCM)
                                                    && ch.Status == StatusType.Open))
                                         .ToListAsync();
                }
                foreach (var item in list)
                {
                    Client_HealthInsurance client_health = item.Client.Clients_HealthInsurances
                                                                      .FirstOrDefault(n => n.Active == true
                                                                                        && n.Agency == ServiceAgency.TCM);
                   
                    if (client_health == null)
                    {
                        client_health = new Client_HealthInsurance();
                    }
                   
                    unitsList.Add(new UnitsAvailabilityViewModel
                    {
                        Id = item.Id,
                        ApprovedDate = client_health.ApprovedDate,
                        DurationTime = client_health.DurationTime,
                        Units = client_health.Units,
                        Active = client_health.Active,
                        Client = item.Client,
                        HealthInsurance = client_health.HealthInsurance,
                        Expired = (client_health.ApprovedDate.AddMonths(client_health.DurationTime) < DateTime.Now) ? true : false,
                        UsedUnits = await this.UsedUnitsPerClientTCM(client_health.ApprovedDate, item.Client.Id),
                        Agency = client_health.Agency
                    });
                }
            }

            ViewData["agency"] = agency;
            return View(unitsList);
        }      
       
        public async Task<IActionResult> UnitsPerClientsInsurances()
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return RedirectToAction("Home/Error404");
            }

            List<ClientEntity> clients = await _context.Clients
                                                       .Where(c => (c.Clinic.Id == user_logged.Clinic.Id && c.Status == Common.Enums.StatusType.Open
                                                                 && c.Service == Common.Enums.ServiceType.PSR))
                                                       .ToListAsync();

            List<HealthInsuranceEntity> insurances = await _context.HealthInsurances
                                                                   .Where(hi => hi.Active == true)
                                                                   .ToListAsync();

            List<UnitsPerClientsInsurancesViewModel> list = new List<UnitsPerClientsInsurancesViewModel>();
            List<Client_HealthInsurance> chi_list = new List<Client_HealthInsurance>();
            int approvedUnits = 0;
            int usedUnits = 0;
            int diference = 0;
            bool create;

            foreach (var item in clients)
            {
                create = false;
                foreach (var value in insurances)
                {
                    approvedUnits = 0;
                    usedUnits = 0;
                    diference = 0;

                    chi_list = await _context.Clients_HealthInsurances
                                             .Where(c => (c.HealthInsurance.Id == value.Id && c.Client.Id == item.Id))
                                             .ToListAsync();
                    if (chi_list.Count() > 0)
                    {
                        foreach (var element in chi_list)
                        {
                            approvedUnits = approvedUnits + element.Units;
                            usedUnits = usedUnits + await this.UsedUnitsPerClient(element.Id, item.Id, value.Id);
                        }
                        diference = approvedUnits - usedUnits;
                        list.Add(new UnitsPerClientsInsurancesViewModel
                                    {
                                        ClientName = item.Name,
                                        HealthInsuranceName = value.Name,
                                        ClientCode = item.Code,
                                        ApprovedUnits = approvedUnits,
                                        UsedUnits = usedUnits,
                                        AvailableUnits = diference
                                    }
                        );
                        create = true;
                    }                    
                }
                if (!create)
                {
                    list.Add(new UnitsPerClientsInsurancesViewModel
                                {
                                    ClientName = item.Name,
                                    HealthInsuranceName = "-",
                                    ClientCode = item.Code,
                                    ApprovedUnits = 0,
                                    UsedUnits = 0,
                                    AvailableUnits = 0
                                }
                    );
                }
            }

            return View(list.OrderBy(u => u.AvailableUnits));
        }

        #region Utils Functions
        private async Task<int> UsedUnitsPerClient(int idClientHealthInsurance, int idClient, int idHealthInsurance)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            int usedUnits = 0;
            List<Client_HealthInsurance> list = await _context.Clients_HealthInsurances
                                                              .Where(c => (c.Client.Id == idClient && c.HealthInsurance.Id == idHealthInsurance))
                                                              .ToListAsync();

            int schema1Count = 0;
            int schema2Count = 0;
            List<NotePEntity> list_of_notesP;            
            int schema4Count = 0;
            int notNotesCount = 0;
            //int indNotesCount = 0;

            Client_HealthInsurance entity = await _context.Clients_HealthInsurances
                                                          .FirstOrDefaultAsync(c => (c.Id == idClientHealthInsurance));

            if(entity != null)
            { 
                //ultima asignacion de unidades de ese cliente a ese seguro
                if (list.Last().Id == idClientHealthInsurance)
                {
                    schema1Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema1 
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = schema1Count * 16;

                    schema2Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema2
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = usedUnits + (schema2Count * 16);

                    list_of_notesP = await _context.NotesP
                                                   .Where(n => (n.Schema == Common.Enums.SchemaType.Schema3
                                                             && n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                                                             && n.Workday_Cient.Client.Id == idClient))
                                           .ToListAsync();
                    foreach (var item in list_of_notesP)
                    {
                        usedUnits = usedUnits + item.RealUnits;
                    }                   

                    schema4Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema4
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = usedUnits + (schema4Count * 12);

                    notNotesCount = _context.Workdays_Clients
                                            .Where(wc => (wc.Note == null
                                                       && wc.NoteP == null
                                                       && wc.IndividualNote == null
                                                       && wc.GroupNote == null
                                                       && wc.Workday.Date >= entity.ApprovedDate
                                                       && wc.Client.Id == idClient))
                                            .Count();
                    int value = (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema1) ? 16 :
                                    (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema2) ? 16 :
                                        (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema3) ? 16 :
                                            (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema4) ? 12 : 0;
                    usedUnits = usedUnits + (notNotesCount * value);

                    //indNotesCount = _context.IndividualNotes
                    //                        .Where(n => (n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                    //                                  && n.Workday_Cient.Client.Id == idClient))
                    //                        .Count();
                    //usedUnits = usedUnits + (indNotesCount * 4);
                }
                else 
                {
                    int index = list.IndexOf(entity);
                    Client_HealthInsurance nextEntity = list.ElementAt(index + 1);
                    
                    schema1Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema1
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate && n.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                     && n.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = schema1Count * 16;

                    schema2Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema2
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate && n.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                     && n.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = usedUnits + (schema2Count * 16);

                    list_of_notesP = await _context.NotesP
                                                   .Where(n => (n.Schema == Common.Enums.SchemaType.Schema3
                                                       && n.Workday_Cient.Workday.Date >= entity.ApprovedDate && n.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                       && n.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                                                       && n.Workday_Cient.Client.Id == idClient))
                                                   .ToListAsync();
                    foreach (var item in list_of_notesP)
                    {
                        usedUnits = usedUnits + item.RealUnits;
                    }

                    schema4Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema4
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate && n.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                     && n.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = usedUnits + (schema4Count * 12);

                    notNotesCount = _context.Workdays_Clients
                                            .Where(wc => (wc.Note == null
                                                       && wc.NoteP == null
                                                       && wc.IndividualNote == null
                                                       && wc.GroupNote == null
                                                       && wc.Workday.Date >= entity.ApprovedDate && wc.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                       && wc.Workday.Date < nextEntity.ApprovedDate
                                                       && wc.Client.Id == idClient))
                                            .Count();
                    int value = (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema1) ? 16 :
                                    (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema2) ? 16 :
                                        (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema3) ? 16 :
                                            (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema4) ? 12 : 0;
                    usedUnits = usedUnits + (notNotesCount * value);

                    //indNotesCount = _context.IndividualNotes
                    //                        .Where(i => (i.Workday_Cient.Workday.Date >= entity.ApprovedDate && i.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                    //                                  && i.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                    //                                  && i.Workday_Cient.Client.Id == idClient))
                    //                        .Count();
                    //usedUnits = usedUnits + (indNotesCount * 4);
                }
            }

            return usedUnits;
        }

        private async Task<int> UsedUnitsPerClient(int idClientHealthInsurance, int idClient, int idHealthInsurance, DateTime approvedDateNextEntity)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            int usedUnits = 0;
            List<Client_HealthInsurance> list = await _context.Clients_HealthInsurances
                                                              .Where(c => (c.Client.Id == idClient && c.HealthInsurance.Id == idHealthInsurance))
                                                              .ToListAsync();

            int schema1Count = 0;
            int schema2Count = 0;
            List<NotePEntity> list_of_notesP;
            int schema4Count = 0;
            int notNotesCount = 0;
            //int indNotesCount = 0;

            Client_HealthInsurance entity = await _context.Clients_HealthInsurances
                                                          .FirstOrDefaultAsync(c => (c.Id == idClientHealthInsurance));

            if (entity != null)
            {
                //ultima asignacion de unidades de ese cliente a ese seguro
                if (list.Last().Id == idClientHealthInsurance)
                {
                    schema1Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema1
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                                                     && n.Workday_Cient.Workday.Date < approvedDateNextEntity
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = schema1Count * 16;

                    schema2Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema2
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                                                     && n.Workday_Cient.Workday.Date < approvedDateNextEntity
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = usedUnits + (schema2Count * 16);

                    list_of_notesP = await _context.NotesP
                                                   .Where(n => (n.Schema == Common.Enums.SchemaType.Schema3
                                                       && n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                                                       && n.Workday_Cient.Workday.Date < approvedDateNextEntity
                                                       && n.Workday_Cient.Client.Id == idClient))
                                                   .ToListAsync();
                    foreach (var item in list_of_notesP)
                    {
                        usedUnits = usedUnits + item.RealUnits;
                    }                   

                    schema4Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema4
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                                                     && n.Workday_Cient.Workday.Date < approvedDateNextEntity
                                                     && n.Workday_Cient.Client.Id == idClient))
                                          .Count();
                    usedUnits = usedUnits + (schema4Count * 12);

                    notNotesCount = _context.Workdays_Clients
                                            .Where(wc => (wc.Note == null
                                                       && wc.NoteP == null
                                                       && wc.IndividualNote == null
                                                       && wc.GroupNote == null
                                                       && wc.Workday.Date >= entity.ApprovedDate
                                                       && wc.Workday.Date < approvedDateNextEntity
                                                       && wc.Client.Id == idClient))
                                            .Count();
                    int value = (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema1) ? 16 :
                                    (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema2) ? 16 :
                                        (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema3) ? 16 :
                                            (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema4) ? 12 : 0;
                    usedUnits = usedUnits + (notNotesCount * value);

                    //indNotesCount = _context.IndividualNotes
                    //                        .Where(n => (n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                    //                                  && n.Workday_Cient.Client.Id == idClient))
                    //                        .Count();
                    //usedUnits = usedUnits + (indNotesCount * 4);
                }
                else
                {
                    int index = list.IndexOf(entity);
                    Client_HealthInsurance nextEntity = list.ElementAt(index + 1);

                    schema1Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema1
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate && n.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                     && n.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = schema1Count * 16;

                    schema2Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema2
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate && n.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                     && n.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = usedUnits + (schema2Count * 16);

                    list_of_notesP = await _context.NotesP
                                                   .Where(n => (n.Schema == Common.Enums.SchemaType.Schema3
                                                       && n.Workday_Cient.Workday.Date >= entity.ApprovedDate && n.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                       && n.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                                                       && n.Workday_Cient.Client.Id == idClient))
                                                   .ToListAsync();
                    foreach (var item in list_of_notesP)
                    {
                        usedUnits = usedUnits + item.RealUnits;
                    }

                    schema4Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema4
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate && n.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                     && n.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = usedUnits + (schema4Count * 12);

                    notNotesCount = _context.Workdays_Clients
                                            .Where(wc => (wc.Note == null
                                                       && wc.NoteP == null
                                                       && wc.IndividualNote == null
                                                       && wc.GroupNote == null
                                                       && wc.Workday.Date >= entity.ApprovedDate && wc.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                       && wc.Workday.Date < nextEntity.ApprovedDate
                                                       && wc.Client.Id == idClient))
                                            .Count();
                    int value = (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema1) ? 16 :
                                    (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema2) ? 16 :
                                        (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema3) ? 16 :
                                            (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema4) ? 12 : 0;
                    usedUnits = usedUnits + (notNotesCount * value);

                    //indNotesCount = _context.IndividualNotes
                    //                        .Where(i => (i.Workday_Cient.Workday.Date >= entity.ApprovedDate && i.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                    //                                  && i.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                    //                                  && i.Workday_Cient.Client.Id == idClient))
                    //                        .Count();
                    //usedUnits = usedUnits + (indNotesCount * 4);
                }
            }

            return usedUnits;
        }

        private async Task<int> AvailableUnitsPerClientInsurance(int idClient, int idHealthInsurance, DateTime approvedDateLimit)
        {
            int availableUnits = 0;
            int usedUnits = 0;
            int diference = 0;

            List<Client_HealthInsurance> list = await _context.Clients_HealthInsurances
                                                              .Where(c => (c.Client.Id == idClient && c.HealthInsurance.Id == idHealthInsurance))
                                                              .ToListAsync();

            Client_HealthInsurance entity;
            if (list.Count > 0)
            {
                entity = list.Last();
                usedUnits = await this.UsedUnitsPerClient(entity.Id, idClient, idHealthInsurance, approvedDateLimit);
                diference = entity.Units - usedUnits;
                if (diference >= 0)
                   return diference;                
                else
                    return 0;
            }
            return availableUnits;
        }

        private async Task<int> UsedUnitsPerClientTCM(DateTime dateApproved, int idClient)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            TCMClientEntity tcmClient = await _context.TCMClient
                                                      .Include(n => n.TCMNote)
                                                      .ThenInclude(n => n.TCMNoteActivity)
                                                      .FirstOrDefaultAsync(n => n.Client.Id == idClient);

            if (tcmClient != null)
            {
                List<TCMNoteEntity> notes = tcmClient.TCMNote.Where(n => n.DateOfService.Date >= dateApproved).ToList();
                int count = 0;
                foreach (var item in notes)
                {
                    int factor = 15;
                    int unit = item.TCMNoteActivity.Sum(n => n.Minutes) / factor;
                    double residuo = item.TCMNoteActivity.Sum(n => n.Minutes) % factor;
                    if (residuo >= 8)
                        unit++;
                    count += unit;
                }
               
                return count;
            }
            else
            {
                return 0;
            }

        }

        #endregion

        public async Task<IActionResult> CreateModal(int id = 0)
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

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            HealthInsuranceViewModel entity = new HealthInsuranceViewModel()
            {
                SignedDate = DateTime.Now,
                DurationTime = 12,
                Active = true,
                NeedAuthorization = false
            };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(HealthInsuranceViewModel healthInsuranceViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                HealthInsuranceEntity insurance = await _context.HealthInsurances
                                                                .FirstOrDefaultAsync(c => (c.Name == healthInsuranceViewModel.Name && c.Clinic.Id == user_logged.Clinic.Id));
                if (insurance == null)
                {
                    string documentPath = string.Empty;
                    if (healthInsuranceViewModel.DocumentFile != null)
                    {
                        documentPath = await _imageHelper.UploadFileAsync(healthInsuranceViewModel.DocumentFile, "Insurances");
                    }

                    healthInsuranceViewModel.IdClinic = user_logged.Clinic.Id;

                    HealthInsuranceEntity healthEnsuranceEntity = await _converterHelper.ToHealthInsuranceEntity(healthInsuranceViewModel, true, user_logged.Id, documentPath);
                    _context.Add(healthEnsuranceEntity);

                    try
                    {
                        await _context.SaveChangesAsync();
                        List<HealthInsuranceEntity> healthInsurance_List = await _context.HealthInsurances
                                                                                         .Include(n => n.Client_HealthInsurances)
                                                                                         .ThenInclude(n => n.Client)
                                                                                         .Where(hi => (hi.Clinic.Id == user_logged.Clinic.Id))
                                                                                         .OrderBy(c => c.Name)
                                                                                         .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurances", healthInsurance_List) });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the health insurance: {healthEnsuranceEntity.Name}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", healthInsuranceViewModel) });
                }
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", healthInsuranceViewModel) });
        }

        public async Task<IActionResult> EditModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            HealthInsuranceEntity entity = await _context.HealthInsurances
                                                         .Include(hi => hi.Clinic)
                                                         .FirstOrDefaultAsync(hi => hi.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            HealthInsuranceViewModel healthInsuranceViewModel = _converterHelper.ToHealthInsuranceViewModel(entity);
            return View(healthInsuranceViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(int id, HealthInsuranceViewModel healthInsuranceViewModel)
        {
            if (id != healthInsuranceViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                string path = healthInsuranceViewModel.DocumentPath;

                if (healthInsuranceViewModel.DocumentFile != null)
                {
                    path = await _imageHelper.UploadFileAsync(healthInsuranceViewModel.DocumentFile, "Insurances");
                }

                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                HealthInsuranceEntity healthInsuranceEntity = await _converterHelper.ToHealthInsuranceEntity(healthInsuranceViewModel, false, user_logged.Id, path);
                _context.Update(healthInsuranceEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    List<HealthInsuranceEntity> healthInsurance_List = await _context.HealthInsurances
                                                                                     .Include(n => n.Client_HealthInsurances)
                                                                                     .ThenInclude(n => n.Client)
                                                                                     .Where(hi => (hi.Clinic.Id == user_logged.Clinic.Id))
                                                                                     .OrderBy(c => c.Name)
                                                                                     .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurances", healthInsurance_List) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the health insurance: {healthInsuranceEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", healthInsuranceViewModel) });
        }


        public async Task<IActionResult> ViewDetails(int id = 0, StatusType status = StatusType.Open)
        {
            List<ClientEntity> clients = new List<ClientEntity>();
            if (id > 0)
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .ThenInclude(c => c.Setting)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                clients = await _context.Clients
                                        .Include(n => n.Clients_HealthInsurances)
                                        .ThenInclude(n => n.HealthInsurance)
                                        .Where(n => n.Clients_HealthInsurances.Where(m => m.Active == true && m.HealthInsurance.Id == id).Count() > 0
                                                 && n.Status == status)
                                        .OrderBy(d => d.Name)
                                        .ToListAsync();
            }


            return View(clients);
        }

    }
}