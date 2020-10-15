using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace KyoS.Web.Helpers
{
    public interface ICombosHelper
    {
        IEnumerable<SelectListItem> GetComboRoles();
        
    }
}
