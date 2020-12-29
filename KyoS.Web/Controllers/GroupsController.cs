using AspNetCore.Reporting;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GroupsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        
        //private readonly IWebHostEnvironment _webHostEnvironment;

        public GroupsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            //_webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Groups.Include(g => g.Facilitator).Include(g => g.Clients).OrderBy(g => g.Facilitator.Name).ToListAsync());
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

            GroupViewModel model = new GroupViewModel
            {
                Facilitators = _combosHelper.GetComboFacilitators()
            };

            MultiSelectList client_list = new MultiSelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["clients"] = client_list;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel model, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                switch (form["Meridian"])
                {
                    case "Am":
                        {
                            model.Am = true;
                            model.Pm = false;
                            break;
                        }
                    case "Pm":
                        {
                            model.Am = false;
                            model.Pm = true;
                            break;
                        }
                    default:
                        break;
                }

                GroupEntity group = await _converterHelper.ToGroupEntity(model, true);
                _context.Add(group);

                if (!string.IsNullOrEmpty(form["clients"]))
                {
                    string[] clients = form["clients"].ToString().Split(',');
                    ClientEntity client;
                    foreach (string value in clients)
                    {
                        client = await _context.Clients.FindAsync(Convert.ToInt32(value));
                        if (client != null)
                        {
                            client.Group = group;
                            _context.Update(client);
                        }
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Create", new { id = 1 });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the objective");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Facilitators = _combosHelper.GetComboFacilitators();

            MultiSelectList client_list = new MultiSelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["clients"] = client_list;

            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GroupEntity groupEntity = await _context.Groups.Include(g => g.Facilitator)
                                                           .Include(g => g.Clients).FirstOrDefaultAsync(g => g.Id == id);
            if (groupEntity == null)
            {
                return NotFound();
            }

            GroupViewModel groupViewModel = _converterHelper.ToGroupViewModel(groupEntity);
            ViewData["am"] = groupViewModel.Am ? "true" : "false";            

            MultiSelectList client_list = new MultiSelectList(await _context.Clients.ToListAsync(), "Id", "Name", groupViewModel.Clients.Select(c=>c.Id));
            ViewData["clients"] = client_list;

            return View(groupViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupViewModel model, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                switch (form["Meridian"])
                {
                    case "Am":
                        {
                            model.Am = true;
                            model.Pm = false;
                            break;
                        }
                    case "Pm":
                        {
                            model.Am = false;
                            model.Pm = true;
                            break;
                        }
                    default:
                        break;
                }

                GroupEntity group = await _converterHelper.ToGroupEntity(model, false);
                _context.Update(group);

                GroupEntity original_group = await _context.Groups.Include(g => g.Clients)
                                                                  .FirstOrDefaultAsync(g => g.Id == model.Id);

                foreach (ClientEntity value in original_group.Clients)
                {
                    value.Group = null;
                    _context.Update(value);
                }

                if (!string.IsNullOrEmpty(form["clients"]))
                {
                    string[] clients = form["clients"].ToString().Split(',');
                    ClientEntity client;
                    foreach (string value in clients)
                    {
                        client = await _context.Clients.FindAsync(Convert.ToInt32(value));
                        if (client != null)
                        {
                            client.Group = group;
                            _context.Update(client);
                        }
                    }
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
                        ModelState.AddModelError(string.Empty, "Already exists the group");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            GroupEntity groupEntity = await _context.Groups.Include(g => g.Facilitator)
                                                           .Include(g => g.Clients).FirstOrDefaultAsync(g => g.Id == model.Id);
            GroupViewModel groupViewModel = _converterHelper.ToGroupViewModel(groupEntity);
            ViewData["am"] = groupViewModel.Am ? "true" : "false";

            //List<ClientEntity> list = await (from cl in _context.Clients
            //                                 join g in groupViewModel.Clients on cl.Id equals g.Id
            //                                 select cl).ToListAsync();

            MultiSelectList client_list = new MultiSelectList(await _context.Clients.ToListAsync(), "Id", "Name", groupViewModel.Clients.Select(c => c.Id));
            ViewData["clients"] = client_list;

            return View(groupViewModel);
        }

        public IActionResult Print(int? id)
        {
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Groups\\{1}.rdlc", fileDirPath, "rptGroup");
            Dictionary<string, string> parameters = new Dictionary<string, string>();            
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            System.Text.Encoding.GetEncoding("windows-1252");
            LocalReport report = new LocalReport(rdlcFilePath);
            
            GroupEntity groupEntity = _context.Groups.Include(f => f.Facilitator).
                                              ThenInclude(c => c.Clinic).FirstOrDefault(g => g.Id == id);
            
            List<ClinicEntity> clinics = new List <ClinicEntity> { groupEntity.Facilitator.Clinic };
            List<GroupEntity> groups = new List<GroupEntity> { groupEntity };
            List<ClientEntity> clients = _context.Clients.Where(c => c.Group.Id == groupEntity.Id).ToList();

            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { groupEntity.Facilitator };

            report.AddDataSource("dsGroups", groups);
            report.AddDataSource("dsClinics", clinics);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsClients", clients);

            var sesion = (groupEntity.Am) ? "Session: AM" : "Session: PM";
            parameters.Add("sesion", sesion);

            //var logopath = Url.Content(groupEntity.Facilitator.Clinic.LogoPath);
            //var logopath = new Uri("C:\\logo.jpg");
            //var logopath = "05fad053-f1c1-4ce8-a816-778ae3387173.jpg";
            //var logopath = "wwwroot\\images\\Clinics\\05fad053-f1c1-4ce8-a816-778ae3387173.jpg";
            var logopath = "";
            parameters.Add("logopath", logopath);

            //string paramValue = "";
            //using (var b = new Bitmap(@"YOUR IMAGE"))
            //{
            //    using (var ms = new System.IO.MemoryStream())
            //    {
            //        b.Save(ms, ImageFormat.Bmp);
            //        paramValue = Convert.ToBase64String(ms.ToArray());
            //    }
            //}

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, System.Net.Mime.MediaTypeNames.Application.Octet, 
                        $"Group_{groupEntity.Facilitator.Name}_{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}.pdf");
        }
    }
}