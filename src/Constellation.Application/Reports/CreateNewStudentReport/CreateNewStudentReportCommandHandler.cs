namespace Constellation.Application.Reports.CreateNewStudentReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Reports;
using Constellation.Core.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

public class CreateNewStudentReportCommandHandler 
    : ICommandHandler<CreateNewStudentReportCommand>
{
    private readonly IAcademicReportRepository _reportRepository;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNewStudentReportCommandHandler(
        IAcademicReportRepository reportRepository,
        IStoredFileRepository storedFileRepository,
        IUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _storedFileRepository = storedFileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateNewStudentReportCommand request, CancellationToken cancellationToken)
    {
        var report = AcademicReport.Create(
            new AcademicReportId(),
            request.StudentId,
            request.PublishId,
            request.Year,
            request.ReportingPeriod);

        _reportRepository.Insert(report);

        var file = new StoredFile
        {
            Name = request.FileName,
            FileType = "application/pdf",
            FileData = request.FileData,
            CreatedAt = DateTime.Now,
            LinkType = StoredFile.StudentReport,
            LinkId = report.Id.ToString()
        };

        _storedFileRepository.Insert(file);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
