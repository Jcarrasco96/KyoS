using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface IRenderHelper
    {
        string RenderRazorViewToString(Controller controller, string viewName, object model = null);
    }
}
