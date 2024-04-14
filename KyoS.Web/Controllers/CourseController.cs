using KyoS.Common.Enums;
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
using System.Net.Sockets;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class CourseController : Controller
    {
        private readonly DataContext _context;
        private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        

        public CourseController(DataContext context, IImageHelper imageHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _imageHelper = imageHelper;
            _converterHelper = converterHelper;
            _combosHelper = combosHelper;
            _renderHelper = renderHelper;
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            return View(await _context.Courses.OrderBy(t => t.Name).ToListAsync());
        }

        [Authorize(Roles = "Manager")]
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

            CourseViewModel model = new CourseViewModel
            {
                Roles = _combosHelper.GetComboRoles()               
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Manager") )
            {
                if (ModelState.IsValid)
                {
                    CourseEntity course = _converterHelper.ToCourseEntity(model, true, user_logged.UserName);
                    course.Clinic = user_logged.Clinic;
                    _context.Add(course);
                    try
                    {
                        await _context.SaveChangesAsync();
                        List<CourseEntity> courses = await _context.Courses
                                                                    .OrderBy(g => g.Role)
                                                                    .ThenBy(g => g.Name)
                                                                    .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewCourses", courses) });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }

                }
                else
                {
                    model.Roles = _combosHelper.GetComboRoles();
                
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
                }
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            model.Roles = _combosHelper.GetComboRoles();
          
            return View(model);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CourseEntity course = await _context.Courses
                                                .FirstOrDefaultAsync(s => s.Id == id);
            if (course == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("Index", "Course");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int? id, int error = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CourseEntity courseEntity = await _context.Courses
                                                      .Include(g => g.Clinic)
                                                      .FirstOrDefaultAsync(g => g.Id == id);
            if (courseEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CourseViewModel coursViewModel = _converterHelper.ToCourseViewModel(courseEntity);
           
            return View(coursViewModel);

        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourseViewModel model)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                CourseEntity course = _converterHelper.ToCourseEntity(model, false, user_logged.UserName);
                course.Clinic = user_logged.Clinic;

                _context.Courses.Update(course);
                try
                {
                    await _context.SaveChangesAsync();
                    List<CourseEntity> courses = await _context.Courses
                                                                   .Include(g => g.Clinic)
                                                                   .OrderBy(g => g.Role)
                                                                   .ThenBy(g => g.Name)
                                                                   .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewCourses", courses) });

                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            model.Roles = _combosHelper.GetComboRoles();
          
            return View(model);
        }

    }
}
