namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


internal sealed class SentralPhotoSyncJob : ISentralPhotoSyncJob
{
    private readonly IAppDbContext _context;
    private readonly ISentralGateway _gateway;
    private readonly ILogger _logger;

    public SentralPhotoSyncJob(
        IAppDbContext context, 
        ISentralGateway gateway, 
        ILogger logger)
    {
        _context = context;
        _gateway = gateway;
        _logger = logger.ForContext<ISentralPhotoSyncJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        var students = await _context.Students.Where(student => !student.IsDeleted).ToListAsync(token);

        foreach (var student in students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName).ThenBy(student => student.FirstName))
        {
            if (token.IsCancellationRequested)
                return;

            _logger.Information("{id}: Checking student {student} ({grade}) for photo", jobId, student.DisplayName, student.CurrentGrade.AsName());

            var photo = await _gateway.GetSentralStudentPhoto(student.StudentId);

            try
            {
                if (student.Photo == null || !student.Photo.SequenceEqual(photo))
                {
                    _logger.Information("{id}: Found new photo for {student} ({grade})", jobId, student.DisplayName, student.CurrentGrade.AsName());

                    student.Photo = photo;
                    await _context.SaveChangesAsync(token);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("{id}: Failed to check student photo for {student} ({grade}) due to error {error}", jobId, student.DisplayName, student.CurrentGrade.AsName(), ex.Message);
            }
        }
    }
}