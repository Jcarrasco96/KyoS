using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GroupsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private IReportService _reportService;
        public GroupsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IReportService reportService)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _reportService = reportService;
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
            
            List<ClientEntity> list = await (from cl in _context.Clients
                                                     join g in groupViewModel.Clients on cl.Id equals g.Id
                                                     select cl).ToListAsync();

            MultiSelectList client_list = new MultiSelectList(await _context.Clients.ToListAsync(), "Id", "Name", list.Select(c => c.Id));
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

            List<ClientEntity> list = await (from cl in _context.Clients
                                             join g in groupViewModel.Clients on cl.Id equals g.Id
                                             select cl).ToListAsync();

            MultiSelectList client_list = new MultiSelectList(await _context.Clients.ToListAsync(), "Id", "Name", list.Select(c => c.Id));
            ViewData["clients"] = client_list;

            return View(groupViewModel);
        }

        public ActionResult Print(int? id)
        {
            var reportName = "Group.rdlc";
            var returnString = _reportService.GenerateReportAsync(reportName);
            return File(returnString, System.Net.Mime.MediaTypeNames.Application.Octet, reportName + ".pdf");
        }


    }
}