﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface ITranslateHelper
    {
        string TranslateText(string fromLanguage, string toLanguage, string textToTranslate);
    }
}
