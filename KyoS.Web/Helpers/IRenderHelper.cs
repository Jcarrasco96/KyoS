using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace KyoS.Web.Helpers
{
    public interface IRenderHelper
    {
        string RenderRazorViewToString(Controller controller, string viewName, object model = null, Dictionary<string, object> values = null);
    }
}
