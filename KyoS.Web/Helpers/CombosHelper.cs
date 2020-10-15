﻿using KyoS.Common.Enums;
using KyoS.Web.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public class CombosHelper : ICombosHelper
    {
        private readonly DataContext _context;

        public CombosHelper(DataContext context)
        {
            _context = context;
        }

        public IEnumerable<SelectListItem> GetComboRoles()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = UserType.Facilitator.ToString(), Value = "1"},
                                  new SelectListItem { Text = UserType.Operator.ToString(), Value = "2"},
                                  new SelectListItem { Text = UserType.Admin.ToString(), Value = "3"}};
            
            list.Insert(0, new SelectListItem
            {
                Text = "[Select a rol...]",
                Value = "0"
            });

            return list;
        }
    }
}
