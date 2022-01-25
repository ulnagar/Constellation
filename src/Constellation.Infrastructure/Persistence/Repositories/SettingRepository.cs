using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly AppDbContext _context;

        public SettingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AppSettings> Get()
        {
            var settings = await _context.AppSettings
                .OrderByDescending(settings => settings.Id)
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new AppSettings();
                _context.Add(settings);
            }
            
            return settings;
        }

        public async Task<AppSettings.AbsencesModule> GetAbsenceAppSettings()
        {
            var settings = await Get();

            return settings.Absences;
        }
    }
}
