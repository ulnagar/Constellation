using Constellation.Application.Features.Jobs.SentralReportSync.Commands;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.SentralReportSync.Commands
{
    public class ReplaceStudentReportCommandHandler : IRequestHandler<ReplaceStudentReportCommand>
    {
        private readonly IAppDbContext _context;

        public ReplaceStudentReportCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ReplaceStudentReportCommand request, CancellationToken cancellationToken)
        {
            var existingReport = await _context.StudentReports
                .FirstOrDefaultAsync(report => report.PublishId == request.OldPublishId);

            var newReport = new StudentReport
            {
                ReportingPeriod = existingReport.ReportingPeriod,
                StudentId = existingReport.StudentId,
                Year = existingReport.Year,
                PublishId = request.NewPublishId
            };

            _context.StudentReports.Remove(existingReport);
            _context.Add(newReport);
            await _context.SaveChangesAsync(cancellationToken);

            var existingFile = await _context.StoredFiles
                .FirstOrDefaultAsync(file => file.LinkType == StoredFile.StudentReport && file.LinkId == existingReport.Id.ToString());

            var newFile = new StoredFile
            {
                Name = existingFile.Name,
                FileType = "application/pdf",
                FileData = request.File,
                CreatedAt = DateTime.Now,
                LinkType = StoredFile.StudentReport,
                LinkId = existingReport.Id.ToString()
            };

            _context.StoredFiles.Remove(existingFile);
            _context.Add(newFile);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
