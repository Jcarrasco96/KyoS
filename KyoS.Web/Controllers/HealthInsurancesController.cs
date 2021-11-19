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
    [Authorize(Roles = "Mannager")]
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
                return NotFound();
            }

            return View(await _context.HealthInsurances
                                      .Where(hi => hi.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderBy(c => c.Name).ToListAsync());
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

            HealthInsuranceViewModel entity = new HealthInsuranceViewModel()
            {
                SignedDate = DateTime.Now,
                DurationTime = 12,
                Active = true
            };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HealthInsuranceViewModel healthInsuranceViewModel)
        {
            if (ModelState.IsValid)
            {
                HealthInsuranceEntity insurance = await _context.HealthInsurances.FirstOrDefaultAsync(c => c.Name == healthInsuranceViewModel.Name);
                if (insurance == null)
                {
                    string documentPath = string.Empty;
                    if (healthInsuranceViewModel.DocumentFile != null)
                    {
                        documentPath = await _imageHelper.UploadFileAsync(healthInsuranceViewModel.DocumentFile, "Insurances");
                    }

                    UserEntity user_logged = _context.Users
                                                     .Include(u => u.Clinic)
                                                     .FirstOrDefault(u => u.UserName == User.Identity.Name);

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
                return NotFound();
            }

            HealthInsuranceEntity entity = await _context.HealthInsurances
                                                         .Include(hi => hi.Clinic)
                                                         .FirstOrDefaultAsync(hi => hi.Id == id);
            if (entity == null)
            {
                return NotFound();
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
                return NotFound();
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
                return NotFound();
            }

            HealthInsuranceEntity healthInsurancesEntity = await _context.HealthInsurances.FirstOrDefaultAsync(hi => hi.Id == id);
            if (healthInsurancesEntity == null)
            {
                return NotFound();
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
                return NotFound();
            }
            string mimeType = _mimeType.GetMimeType(entity.DocumentPath);
            return File(entity.DocumentPath, mimeType);
        }

        public async Task<IActionResult> UnitsAvailability(int idError = 0)
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
                return NotFound();
            }

            List<Client_HealthInsurance> list = await _context.Clients_HealthInsurances

                                                              .Include(wc => wc.Client)

                                                              .Include(wc => wc.HealthInsurance)
                                                              .ThenInclude(h => h.Clinic)

                                                              .Where(ch => (ch.HealthInsurance.Clinic.Id == user_logged.Clinic.Id))
                                                              .ToListAsync();

            List<UnitsAvailabilityViewModel> unitsList = new List<UnitsAvailabilityViewModel>();
            foreach (var item in list)
            {
                unitsList.Add(new UnitsAvailabilityViewModel { 
                    Id = item.Id, 
                    ApprovedDate = item.ApprovedDate,
                    DurationTime = item.DurationTime,
                    Units = item.Units,
                    Active = item.Active,
                    Client = item.Client,
                    HealthInsurance = item.HealthInsurance,
                    Expired = (item.ApprovedDate.AddMonths(item.DurationTime) < DateTime.Now) ? true : false,
                    UsedUnits = await this.UsedUnitsPerClient(item.Id, item.Client.Id, item.HealthInsurance.Id)
                });
            }

            return View(unitsList);
        }

        public async Task<IActionResult> CreateUnits()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            UnitsAvailabilityViewModel entity = new UnitsAvailabilityViewModel()
            {
                ApprovedDate = DateTime.Now,
                DurationTime = 12,
                IdHealthInsurance = 0,
                HealthInsurances = _combosHelper.GetComboActiveInsurancesByClinic(user_logged.Clinic.Id),
                IdClient = 0,
                Clients = _combosHelper.GetComboActiveClientsByClinic(user_logged.Clinic.Id)
            };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUnits(UnitsAvailabilityViewModel model)
        {
            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                Client_HealthInsurance entity = await _converterHelper.ToClientHealthInsuranceEntity(model, true, user_logged.Id);
                _context.Add(entity);

                await _context.SaveChangesAsync();

                List<Client_HealthInsurance> list = await _context.Clients_HealthInsurances

                                                                    .Include(wc => wc.Client)

                                                                    .Include(wc => wc.HealthInsurance)
                                                                    .ThenInclude(h => h.Clinic)

                                                                    .Where(ch => (ch.HealthInsurance.Clinic.Id == user_logged.Clinic.Id))
                                                                    .ToListAsync();

                List<UnitsAvailabilityViewModel> unitsList = new List<UnitsAvailabilityViewModel>();
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
                        UsedUnits = await this.UsedUnitsPerClient(item.Id, item.Client.Id, item.HealthInsurance.Id)
                    });
                }

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_UnitsAvailability", unitsList) });       
            }

            UnitsAvailabilityViewModel originalEntity = new UnitsAvailabilityViewModel()
            {
                ApprovedDate = model.ApprovedDate,
                DurationTime = model.DurationTime,
                IdHealthInsurance = model.IdHealthInsurance,
                HealthInsurances = _combosHelper.GetComboActiveInsurancesByClinic(user_logged.Clinic.Id),
                IdClient = model.IdClient,
                Clients = _combosHelper.GetComboActiveClientsByClinic(user_logged.Clinic.Id)
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateUnits", originalEntity) });
        }

        public async Task<IActionResult> EditUnits(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Client_HealthInsurance entity = await _context.Clients_HealthInsurances

                                                          .Include(c => c.Client)

                                                          .Include(c => c.HealthInsurance)

                                                          .FirstOrDefaultAsync(hi => hi.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            UnitsAvailabilityViewModel model = _converterHelper.ToClientHealthInsuranceViewModel(entity, user_logged.Clinic.Id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUnits(int id, UnitsAvailabilityViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }
            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                Client_HealthInsurance entity = await _converterHelper.ToClientHealthInsuranceEntity(model, false, user_logged.Id);
                _context.Update(entity);
                await _context.SaveChangesAsync();

                List<Client_HealthInsurance> list = await _context.Clients_HealthInsurances

                                                                    .Include(wc => wc.Client)

                                                                    .Include(wc => wc.HealthInsurance)
                                                                    .ThenInclude(h => h.Clinic)

                                                                    .Where(ch => (ch.HealthInsurance.Clinic.Id == user_logged.Clinic.Id))
                                                                    .ToListAsync();

                List<UnitsAvailabilityViewModel> unitsList = new List<UnitsAvailabilityViewModel>();
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
                        UsedUnits = await this.UsedUnitsPerClient(item.Id, item.Client.Id, item.HealthInsurance.Id)
                    });
                }

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_UnitsAvailability", unitsList) });
            }

            UnitsAvailabilityViewModel originalEntity = new UnitsAvailabilityViewModel()
            {
                ApprovedDate = model.ApprovedDate,
                DurationTime = model.DurationTime,
                IdHealthInsurance = model.IdHealthInsurance,
                HealthInsurances = _combosHelper.GetComboActiveInsurancesByClinic(user_logged.Clinic.Id),
                IdClient = model.IdClient,
                Clients = _combosHelper.GetComboActiveClientsByClinic(user_logged.Clinic.Id)
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditUnits", originalEntity) });
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
            int schema4Count = 0;
            int notNotesCount = 0;
            int indNotesCount = 0;

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

                    schema4Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema4
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                                                     && n.Workday_Cient.Client.Id == idClient))
                                          .Count();
                    usedUnits = usedUnits + (schema4Count * 12);

                    notNotesCount = _context.Workdays_Clients
                                            .Where(wc => (wc.Note == null
                                                       && wc.Workday.Date >= entity.ApprovedDate
                                                       && wc.Client.Id == idClient))
                                            .Count();
                    int value = (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema1) ? 16 :
                                    (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema2) ? 16 :
                                        (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema4) ? 12 : 0;
                    usedUnits = usedUnits + (notNotesCount * value);

                    indNotesCount = _context.IndividualNotes
                                            .Where(n => (n.Workday_Cient.Workday.Date >= entity.ApprovedDate
                                                      && n.Workday_Cient.Client.Id == idClient))
                                            .Count();
                    usedUnits = usedUnits + (indNotesCount * 4);
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

                    schema4Count = _context.Notes
                                           .Where(n => (n.Schema == Common.Enums.SchemaType.Schema4
                                                     && n.Workday_Cient.Workday.Date >= entity.ApprovedDate && n.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                     && n.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                                                     && n.Workday_Cient.Client.Id == idClient))
                                           .Count();
                    usedUnits = usedUnits + (schema4Count * 12);

                    notNotesCount = _context.Workdays_Clients
                                            .Where(wc => (wc.Note == null
                                                       && wc.Workday.Date >= entity.ApprovedDate && wc.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                       && wc.Workday.Date < nextEntity.ApprovedDate
                                                       && wc.Client.Id == idClient))
                                            .Count();
                    int value = (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema1) ? 16 :
                                    (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema2) ? 16 :
                                        (user_logged.Clinic.Schema == Common.Enums.SchemaType.Schema4) ? 12 : 0;
                    usedUnits = usedUnits + (notNotesCount * value);

                    indNotesCount = _context.IndividualNotes
                                            .Where(i => (i.Workday_Cient.Workday.Date >= entity.ApprovedDate && i.Workday_Cient.Workday.Date <= entity.ApprovedDate.AddMonths(entity.DurationTime)
                                                      && i.Workday_Cient.Workday.Date < nextEntity.ApprovedDate
                                                      && i.Workday_Cient.Client.Id == idClient))
                                            .Count();
                    usedUnits = usedUnits + (indNotesCount * 4);
                }
            }

            return usedUnits;
        }
        #endregion
    }
}