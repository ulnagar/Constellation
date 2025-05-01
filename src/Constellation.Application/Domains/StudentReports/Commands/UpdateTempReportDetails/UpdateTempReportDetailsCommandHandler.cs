namespace Constellation.Application.Domains.StudentReports.Commands.UpdateTempReportDetails;

using Abstractions.Messaging;
using Core.Models.Reports;
using Core.Models.Reports.Errors;
using Core.Models.Reports.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateTempReportDetailsCommandHandler
: ICommandHandler<UpdateTempReportDetailsCommand>
{
    private readonly IReportRepository _reportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateTempReportDetailsCommandHandler(
        IReportRepository reportRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _reportRepository = reportRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTempReportDetailsCommand request, CancellationToken cancellationToken)
    {
        TempExternalReport report = await _reportRepository.GetTempExternalReportById(request.ReportId, cancellationToken);

        if (report is null)
        {
            _logger
                .ForContext(nameof(UpdateTempReportDetailsCommand), request, true)
                .ForContext(nameof(Error), ExternalReportErrors.TempReportNotFound(request.ReportId), true)
                .Warning("Failed to retrieve Temporary External Report");

            return Result.Failure(ExternalReportErrors.TempReportNotFound(request.ReportId));
        }

        report.UpdateStudentId(request.StudentId);
        report.UpdateReportType(request.ReportType);
        report.UpdateIssuedDate(request.IssuedDate);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
