using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface IClassificationHelper
    {
        Task CheckClassificationAsync(string classification);
    }
}
