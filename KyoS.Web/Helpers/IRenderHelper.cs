using Microsoft.AspNetCore.Mvc;

namespace KyoS.Web.Helpers
{
    public interface IRenderHelper
    {
        string RenderRazorViewToString(Controller controller, string viewName, object model = null);
    }
}
