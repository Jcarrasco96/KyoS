using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Mannager, Supervisor, Facilitator")]
    public class IntakesController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly DataContext _context;

        public IntakesController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
        }

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

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                if (User.IsInRole("Mannager"))
                    return View(await _context.IntakeScreenings
                                              .Include(f => f.Client)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    


                    return View(await _context.IntakeScreenings
                                              .Include(f => f.Client)
                                              //.Where(f => )
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Mannager")]
        public IActionResult Create(int id = 0)
        {
            
            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeScreeningViewModel model;

            if (User.IsInRole("Mannager"))
            {
                

                if (user_logged.Clinic != null)
                {
                    
                    model = new IntakeScreeningViewModel
                    {
                        IdClient = id,
                        Client = _context.Clients.Find(id),
                        InformationGatheredBy = user_logged.FullName,
                        DateAdmision = DateTime.Now,
                        ClientIsStatus = IntakeClientIsStatus.Clean,
                        BehaviorIsStatus = IntakeBehaviorIsStatus.Normal,
                        SpeechIsStatus = IntakeSpeechIsStatus.Normal,
                        DoesClientKnowHisName = true,
                        DoesClientKnowTodayDate = true,
                        DoesClientKnowWhereIs = true,
                        DoesClientKnowTimeOfDay = true,
                        DateSignatureClient = DateTime.Now,
                        DateSignatureWitness = DateTime.Now,
                        IdClientIs = 1,
                        ClientIs_Status = _combosHelper.GetComboIntake_ClientIs(),
                        IdBehaviorIs = 1,
                        BehaviorIs_Status = _combosHelper.GetComboIntake_BehaviorIs(),
                        IdSpeechIs = 1,
                        SpeechIs_Status = _combosHelper.GetComboIntake_SpeechIs(),
                        

                    };
                    return View(model);
                }
            }

            model = new IntakeScreeningViewModel
            {
                IdClient = id,
                Client = _context.Clients.Find(id),
                InformationGatheredBy = user_logged.FullName,
                DateAdmision = DateTime.Now,
                ClientIsStatus = IntakeClientIsStatus.Clean,
                BehaviorIsStatus = IntakeBehaviorIsStatus.Normal,
                SpeechIsStatus = IntakeSpeechIsStatus.Normal,
                DoesClientKnowHisName = true,
                DoesClientKnowTodayDate = true,
                DoesClientKnowWhereIs = true,
                DoesClientKnowTimeOfDay = true,
                DateSignatureClient = DateTime.Now,
                DateSignatureWitness = DateTime.Now,
                IdClientIs = 1,
                ClientIs_Status = _combosHelper.GetComboIntake_ClientIs(),
                IdBehaviorIs = 1,
                BehaviorIs_Status = _combosHelper.GetComboIntake_BehaviorIs(),
                IdSpeechIs = 1,
                SpeechIs_Status = _combosHelper.GetComboIntake_SpeechIs(),
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> Create(IntakeScreeningViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeScreeningEntity IntakeEntity = _context.IntakeScreenings.Find(IntakeViewModel.Id);
                if (IntakeEntity == null)
                {
                    IntakeEntity = await _converterHelper.ToIntakeEntity(IntakeViewModel, true);
                    _context.IntakeScreenings.Add(IntakeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                       
                        return RedirectToAction("Index", "Intakes");
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM service.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", IntakeViewModel) });
                }
            }
            IntakeScreeningViewModel model;
            model = new IntakeScreeningViewModel
            {
                IdClient = IntakeViewModel.IdClient,
                Client = _context.Clients.Find(IntakeViewModel.IdClient),
                InformationGatheredBy = user_logged.FullName,
                DateAdmision = DateTime.Now,
                ClientIsStatus = IntakeClientIsStatus.Clean,
                BehaviorIsStatus = IntakeBehaviorIsStatus.Normal,
                SpeechIsStatus = IntakeSpeechIsStatus.Normal,
                DoesClientKnowHisName = true,
                DoesClientKnowTodayDate = true,
                DoesClientKnowWhereIs = true,
                DoesClientKnowTimeOfDay = true,
                DateSignatureClient = DateTime.Now,
                DateSignatureWitness = DateTime.Now,
                IdClientIs = IntakeViewModel.IdClientIs,
                ClientIs_Status = _combosHelper.GetComboIntake_ClientIs(),
                IdBehaviorIs = IntakeViewModel.IdBehaviorIs,
                BehaviorIs_Status = _combosHelper.GetComboIntake_BehaviorIs(),
                IdSpeechIs = IntakeViewModel.IdSpeechIs,
                SpeechIs_Status = _combosHelper.GetComboIntake_SpeechIs(),
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", IntakeViewModel) });
        }

        [Authorize(Roles = "Mannager")]
        public IActionResult Edit(int id = 0)
        {
            IntakeScreeningViewModel model;

            if (User.IsInRole("Mannager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    IntakeScreeningEntity Intake = _context.IntakeScreenings
                                                                 .Include(m => m.Client)
                                                                 .FirstOrDefault(m => m.Id == id);
                    if (Intake == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToIntakeViewModel(Intake);

                        return View(model);
                    }

                }
            }

            model = new IntakeScreeningViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> Edit(IntakeScreeningViewModel intakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeScreeningEntity intakeEntity = await _converterHelper.ToIntakeEntity(intakeViewModel, false);
                _context.IntakeScreenings.Update(intakeEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Intakes");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", intakeViewModel) });
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> IntakeCandidates(int idError = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null /*|| !user_logged.Clinic.Setting.TCMClinic*/)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            List<ClientEntity> ClientList = await _context.Clients
                                                          .Include(n => n.IntakeScreening)
                                                          .Where(n => n.IntakeScreening == null)
                                                          .ToListAsync();

           /* List<IntakeScreeningEntity> IntakeList = await _context.IntakeScreenings
                                                                      .Include(f => f.Client)
                                                                      .ToListAsync();
            TCMDischargeEntity tcmDiscaharge = new TCMDischargeEntity();
            for (int i = 0; i < tcmDischargeList.Count(); i++)
            {
                tcmDiscaharge.TcmServicePlan = tcmServicePlanList.FirstOrDefault(g => g.Id == tcmDischargeList[i].TcmServicePlan.Id);
                if (tcmDiscaharge != null)
                {
                    tcmServicePlanList.Remove(tcmDiscaharge.TcmServicePlan);
                }

            }*/

            return View(ClientList);

        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            IntakeScreeningEntity intakeEntity = await _context.IntakeScreenings.FirstOrDefaultAsync(s => s.Id == id);
            if (intakeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.IntakeScreenings.Remove(intakeEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
