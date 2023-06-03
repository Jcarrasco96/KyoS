using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Abstract;

namespace KyoS.Web.Data.Entities
{
    public class ReferredTempEntity : Contact
    {
        public int Id { get; set; }

        public int IdReferred { get; set; }

        public string Title { get; set; }
        
        public string Agency { get; set; }

        public ServiceAgency Service { get; set; }

        public string ReferredNote { get; set; }

        public int IdClient { get; set; }
    }
}
