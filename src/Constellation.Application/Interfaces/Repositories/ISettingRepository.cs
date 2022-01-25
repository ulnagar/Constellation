using Constellation.Application.Models;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface ISettingRepository
    {
        Task<AppSettings> Get();
        Task<AppSettings.AbsencesModule> GetAbsenceAppSettings();
    }
}