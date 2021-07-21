using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data;
using KyoS.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Mannager")]
    public class TemplatesDOCController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IMimeType _mimeType;

        public TemplatesDOCController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _imageHelper = imageHelper;
            _mimeType = mimeType;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}