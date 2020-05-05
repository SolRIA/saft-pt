using System.Threading.Tasks;

namespace Solria.SAFT.Desktop.Services
{
    public interface IXmlSerializer
    {
        Task<T> Deserialize<T>(string xmlFileName);
    }
}
