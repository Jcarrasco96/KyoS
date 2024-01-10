using KyoS.Web.Data.Abstract;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class ReferredEntity : Contact
    {
        public int Id{ get; set; }

        public string Title { get; set; }

        public string Agency { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZidCode { get; set; }
    }
}
