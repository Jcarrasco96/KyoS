﻿using KyoS.Web.Data.Abstract;
using System.Collections.Generic;

namespace KyoS.Web.Data.Entities
{
    public class DoctorEntity : Contact
    {
        public int Id { get; set; }
        public virtual ICollection<ClientEntity> Clients { get; set; }
    }
}
