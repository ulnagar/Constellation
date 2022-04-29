using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class SentralPhotoSyncJob : ISentralPhotoSyncJob, IScopedService, IHangfireJob
    {
        private readonly IAppDbContext _context;
        private readonly ISentralGateway _gateway;
        private readonly ILogger<ISentralPhotoSyncJob> _logger;

        public SentralPhotoSyncJob(IAppDbContext context, ISentralGateway gateway, ILogger<ISentralPhotoSyncJob> logger)
        {
            _context = context;
            _gateway = gateway;
            _logger = logger;
        }

        public async Task StartJob(Guid jobId, CancellationToken token)
        {
            var students = await _context.Students.Where(student => !student.IsDeleted).ToListAsync(token);

            foreach (var student in students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName).ThenBy(student => student.FirstName))
            {
                if (token.IsCancellationRequested)
                    return;

                _logger.LogInformation("{id}: Checking student {student} ({grade}) for photo", jobId, student.DisplayName, student.CurrentGrade.AsName());

                var photo = await _gateway.GetSentralStudentPhoto(student.StudentId);

                if (student.Photo != photo)
                {
                    _logger.LogInformation("{id}: Found new photo for {student} ({grade})", jobId, student.DisplayName, student.CurrentGrade.AsName());

                    student.Photo = photo;
                    await _context.SaveChangesAsync(token);
                }
            }
        }
    }
}
