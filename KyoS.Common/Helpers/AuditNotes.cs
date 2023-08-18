﻿using KyoS.Common.Enums;
using System;
using System.Collections.Generic;

namespace KyoS.Common.Helpers
{
    public class AuditNotes
    {
        public string NameClient { get; set; }
        public string NoteDate { get; set; }
        public string Description { get; set; }        
        public int Active { get; set; }
    }
}
