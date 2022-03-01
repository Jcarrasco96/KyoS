using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class TCMClientEntity
    {
        public int Id { get; set; }
       
        public CaseMannagerEntity Casemanager { get; set; }
        public List<ClientEntity> Clients { get; set; }
       
    }
}
