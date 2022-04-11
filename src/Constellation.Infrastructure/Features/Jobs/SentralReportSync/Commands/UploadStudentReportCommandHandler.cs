using Constellation.Application.Features.Jobs.SentralReportSync.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.SentralReportSync.Commands
{
    public class UploadStudentReportCommandHandler : IRequestHandler<UploadStudentReportCommand>
    {
        private readonly IAppDbContext _context;

        public UploadStudentReportCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(UploadStudentReportCommand request, CancellationToken cancellationToken)
        {
            var report = new StudentReport
            {
                StudentId = request.StudentId,
                PublishId = request.PublishId,
                Year = request.Year,
                ReportingPeriod = request.ReportingPeriod
            };

            _context.Add(report);
            await _context.SaveChangesAsync(cancellationToken);

            var storedFile = new StoredFile
            {
                Name = request.FileName,
                FileType = "application/pdf",
                FileData = request.File,
                CreatedAt = DateTime.Now,
                LinkType = StoredFile.StudentReport,
                LinkId = report.Id.ToString()
            };

            _context.Add(storedFile);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
