namespace Constellation.Application.Students.SendFamilyContactChangesReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendFamilyContactChangesReportCommandHandler
    : ICommandHandler<SendFamilyContactChangesReportCommand>
{
    private readonly IExcelService _excelService;
    private readonly IEmailService _emailService;

    public SendFamilyContactChangesReportCommandHandler(
        IExcelService excelService,
        IEmailService emailService)
    {
        _excelService = excelService;
        _emailService = emailService;
    }

    public async Task<Result> Handle(SendFamilyContactChangesReportCommand request, CancellationToken cancellationToken)
    {
        var stream = await _excelService.CreateFamilyContactDetailsChangeReport(request.changes, cancellationToken);

        await _emailService.SendParentContactChangeReportEmail(stream, cancellationToken);

        return Result.Success();
    }
}
