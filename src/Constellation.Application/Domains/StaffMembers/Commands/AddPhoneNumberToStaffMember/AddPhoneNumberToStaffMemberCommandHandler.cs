namespace Constellation.Application.Domains.StaffMembers.Commands.AddPhoneNumberToStaffMember;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Errors;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddPhoneNumberToStaffMemberCommandHandler
    : ICommandHandler<AddPhoneNumberToStaffMemberCommand>
{
    private IStaffRepository _staffRepository {get;set;}
    private IUnitOfWork _unitOfWork {get;set;}
    private ILogger _logger {get;set;}
    
    public AddPhoneNumberToStaffMemberCommandHandler(
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddPhoneNumberToStaffMemberCommand>();     
    }

    public async Task<Result> Handle(AddPhoneNumberToStaffMemberCommand request, CancellationToken cancellationToken)
    {
        StaffMember? staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));

        staffMember.AddPhoneNumber(request.PhoneNumber);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

}