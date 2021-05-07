using KyoS.Web.Data.Abstract;

namespace KyoS.Web.Data.Entities
{
    public class DocumentDiagnosticEntity : Document
    {
        public int Id { get; set; }
        public DiagnosticEntity Diagnostic { get; set; }
    }
}
