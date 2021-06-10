using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class DiagnosticTempEntity
    {
        public int Id { get; set; }

        public string Code { get; set; }
        
        public string Description { get; set; }

        public bool Principal { get; set; }
    }
}
