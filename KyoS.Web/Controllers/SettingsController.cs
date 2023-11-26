using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;

       
        public SettingsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Settings

                                      .Include(s => s.Clinic)

                                      .OrderBy(s => s.Clinic.Name).ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            SettingViewModel entity = new SettingViewModel()
            {
                IdClinic = 0,
                Clinics = _combosHelper.GetComboClinics()
            };
            return View(entity);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SettingViewModel model)
        {
            if (ModelState.IsValid)
            {
                SettingEntity setting = await _context.Settings
                                                      .FirstOrDefaultAsync(s => s.Clinic.Id == model.IdClinic);
                if (setting == null)
                {
                    UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    setting = await _converterHelper.ToSettingEntity(model, true, user_logged.Id);
                    _context.Add(setting);
                    await _context.SaveChangesAsync();
                }

                List<SettingEntity> settingList = await _context.Settings
                                                                .Include(s => s.Clinic)
                                                                .ToListAsync();

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_SettingList", settingList) });
            }

            SettingViewModel entity = new SettingViewModel()
            {
                IdClinic = model.IdClinic,
                Clinics = _combosHelper.GetComboClinics()
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", entity) });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SettingEntity entity = await _context.Settings
                                                 .Include(c => c.Clinic)
                                                 .FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SettingViewModel model = _converterHelper.ToSettingViewModel(entity);

            ClinicEntity clinic = await _context.Clinics
                                                .FirstOrDefaultAsync(c => c.Id == model.IdClinic);

            List<SelectListItem> list = new List<SelectListItem>();
            list.Insert(0, new SelectListItem
            {
                Text = clinic.Name,
                Value = $"{clinic.Id}"
            });
            model.Clinics = list;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SettingViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                SettingEntity setting = await _converterHelper.ToSettingEntity(model, false, user_logged.Id);
                _context.Update(setting);
                await _context.SaveChangesAsync();

                List<SettingEntity> settingList = await _context.Settings
                                                                .Include(s => s.Clinic)
                                                                .ToListAsync();

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_SettingList", settingList) });
            }

            SettingViewModel entity = new SettingViewModel()
            {
                IdClinic = model.IdClinic,
                Clinics = _combosHelper.GetComboClinics()
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", entity) });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditSettingManager()
        {
            UserEntity user_logged = _context.Users
                                            .Include(u => u.Clinic)
                                            .ThenInclude(u => u.Setting)
                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic.Setting == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SettingEntity entity = await _context.Settings
                                                 .Include(c => c.Clinic)
                                                 .FirstOrDefaultAsync(s => s.Id == user_logged.Clinic.Setting.Id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            SettingManagerViewModel model = new SettingManagerViewModel()
            {
                Address = user_logged.Clinic.Address,
                BillSemanalMH = user_logged.Clinic.Setting.BillSemanalMH,
                CEO = user_logged.Clinic.CEO,
                City = user_logged.Clinic.City,
                ClinicalDirector = user_logged.Clinic.ClinicalDirector,
                CodeBIO = user_logged.Clinic.CodeBIO,
                CodeFARS = user_logged.Clinic.CodeFARS,
                CodeGroupTherapy = user_logged.Clinic.CodeGroupTherapy,
                CodeIndTherapy = user_logged.Clinic.CodeIndTherapy,
                CodeMTP = user_logged.Clinic.CodeMTP,
                CodeMTPR = user_logged.Clinic.CodeMTPR,
                CodePSRTherapy = user_logged.Clinic.CodePSRTherapy,
                CPTCode_TCM = user_logged.Clinic.CPTCode_TCM,
                CreateNotesTCMWithServiceplanInEdition = user_logged.Clinic.Setting.CreateNotesTCMWithServiceplanInEdition,
                DischargeJoinCommission = user_logged.Clinic.Setting.DischargeJoinCommission,
                FaxNo = user_logged.Clinic.FaxNo,
                IdClinic = user_logged.Clinic.Id,
                IndNoteForAppointment = user_logged.Clinic.Setting.IndNoteForAppointment,
                LockTCMNoteForUnits = user_logged.Clinic.Setting.LockTCMNoteForUnits,
                MHClassificationOfGoals = user_logged.Clinic.Setting.MHClassificationOfGoals,
                MHProblems = user_logged.Clinic.Setting.MHProblems,
                Phone = user_logged.Clinic.Phone,
                ProviderMedicaidId = user_logged.Clinic.ProviderMedicaidId,
                ProviderTaxId = user_logged.Clinic.ProviderTaxId,
                SignaturePath = user_logged.Clinic.SignaturePath,
                State = user_logged.Clinic.State,
                SupervisorEdit = user_logged.Clinic.Setting.SupervisorEdit,
                TCMEndTime = user_logged.Clinic.Setting.TCMEndTime,
                TCMInitialTime = user_logged.Clinic.Setting.TCMInitialTime,
                TCMSupervisorEdit = user_logged.Clinic.Setting.TCMSupervisorEdit,
                UnitsForDayForClient = user_logged.Clinic.Setting.UnitsForDayForClient,
                ZipCode = user_logged.Clinic.ZipCode,
                TCMSupervisionTimeWithCaseManager = user_logged.Clinic.Setting.TCMSupervisionTimeWithCaseManager

            };

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSettingManager(SettingManagerViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .ThenInclude(u => u.Setting)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                SettingEntity setting = await _context.Settings.FirstOrDefaultAsync(n => n.Id == user_logged.Clinic.Setting.Id);
                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(n => n.Id == user_logged.Clinic.Id);

                if (clinic != null)
                {
                    clinic.CEO = model.CEO;
                    clinic.ProviderTaxId = model.ProviderTaxId;
                    clinic.ProviderMedicaidId = model.ProviderMedicaidId;
                    clinic.ProviderTaxId = model.ProviderTaxId;
                    clinic.ClinicalDirector = model.ClinicalDirector;
                    clinic.SignaturePath = model.SignaturePath;
                    clinic.Address = model.Address;
                    clinic.City = model.City;
                    clinic.ZipCode = model.ZipCode;
                    clinic.State = model.State;
                    clinic.Phone = model.Phone;
                    clinic.FaxNo = model.FaxNo;
                    clinic.CPTCode_TCM = model.CPTCode_TCM;
                    clinic.CodePSRTherapy = model.CodePSRTherapy;
                    clinic.CodeIndTherapy = model.CodeIndTherapy;
                    clinic.CodeGroupTherapy = model.CodeGroupTherapy;
                    clinic.CodeMTP = model.CodeMTP;
                    clinic.CodeMTPR = model.CodeMTPR;
                    clinic.CodeBIO = model.CodeBIO;
                    clinic.CodeFARS = model.CodeFARS;
                }
                _context.Update(clinic);
                await _context.SaveChangesAsync();

                if (setting != null)
                {
                    setting.TCMInitialTime = model.TCMInitialTime;
                    setting.TCMEndTime = model.TCMEndTime;
                    setting.UnitsForDayForClient = model.UnitsForDayForClient;
                    setting.LockTCMNoteForUnits = model.LockTCMNoteForUnits;
                    setting.TCMSupervisorEdit = model.TCMSupervisorEdit;
                    setting.CreateNotesTCMWithServiceplanInEdition = model.CreateNotesTCMWithServiceplanInEdition;
                    setting.SupervisorEdit = model.SupervisorEdit;
                    setting.TCMSupervisionTimeWithCaseManager = model.TCMSupervisionTimeWithCaseManager;

                }
                _context.Update(setting);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index","Desktop");
            }

            return View(model);
        }
    }
}