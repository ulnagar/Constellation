using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface ISentralService
    {
        Task<string> UpdateSentralStudentId(string studentId);
    }
}
