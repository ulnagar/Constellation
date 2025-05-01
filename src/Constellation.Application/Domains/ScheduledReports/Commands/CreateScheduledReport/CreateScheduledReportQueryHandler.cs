namespace Constellation.Application.Domains.ScheduledReports.Commands.CreateScheduledReport;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Shared;
using DTOs;
using Interfaces.Repositories;
using Models;
using Repositories;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateScheduledReportCommandHandler<T>
: ICommandHandler<CreateScheduledReportCommand<T>> where T : IQuery<FileDto>
{
    private readonly IScheduledReportRepository _reportRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public CreateScheduledReportCommandHandler(
        IScheduledReportRepository reportRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateScheduledReportCommand<T> request, CancellationToken cancellationToken)
    {
        ScheduledReport report = ScheduledReport.Create(_dateTime, request.ReportDefinition, request.Recipient);

        _reportRepository.Insert(report);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
