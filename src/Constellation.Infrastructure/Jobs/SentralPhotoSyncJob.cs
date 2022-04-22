﻿using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class SentralPhotoSyncJob : ISentralPhotoSyncJob, IScopedService
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

        public async Task StartJob(bool automated)
        {
            if (automated)
            {
                var jobStatus = await _context.JobActivations.FirstOrDefaultAsync(job => job.JobName == nameof(ISentralPhotoSyncJob));
                if (jobStatus == null || !jobStatus.IsActive)
                {
                    _logger.LogInformation("Stopped due to job being set inactive.");
                    return;
                }
            }

            var students = await _context.Students.Where(student => !student.IsDeleted).ToListAsync();

            foreach (var student in students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName).ThenBy(student => student.FirstName))
            {
                _logger.LogInformation("Checking student {student} ({grade}) for photo", student.DisplayName, student.CurrentGrade.AsName());

                var photo = await _gateway.GetSentralStudentPhoto(student.StudentId);

                if (student.Photo != photo)
                {
                    _logger.LogInformation("Found new photo for {student} ({grade})", student.DisplayName, student.CurrentGrade.AsName());

                    student.Photo = photo;
                    await _context.SaveChangesAsync(new System.Threading.CancellationToken());
                }
            }
        }
    }
}
