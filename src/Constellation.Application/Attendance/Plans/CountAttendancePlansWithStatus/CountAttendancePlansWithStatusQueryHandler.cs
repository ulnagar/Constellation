namespace Constellation.Application.Attendance.Plans.CountAttendancePlansWithStatus;

using Abstractions.Messaging;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountAttendancePlansWithStatusQueryHandler
: IQueryHandler<CountAttendancePlansWithStatusQuery, (int Pending, int Processing)>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly ILogger _logger;

    public CountAttendancePlansWithStatusQueryHandler(
        IAttendancePlanRepository planRepository,
        ILogger logger)
    {
        _planRepository = planRepository;
        _logger = logger
            .ForContext<CountAttendancePlansWithStatusQuery>();
    }

    public async Task<Result<(int Pending, int Processing)>> Handle(CountAttendancePlansWithStatusQuery request, CancellationToken cancellationToken)
    {
        int pending = await _planRepository.GetCountOfPending(cancellationToken);

        int processing = await _planRepository.GetCountOfProcessing(cancellationToken);

        return (pending, processing);
    }
}