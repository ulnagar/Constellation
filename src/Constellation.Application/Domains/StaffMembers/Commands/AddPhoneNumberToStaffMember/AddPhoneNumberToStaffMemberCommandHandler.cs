namespace Constellation.Application.Domains.StaffMember.Commands.AddPhoneNumberToStaffMember;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Application.Interfaces.Repositories;
using Serilog;
using System.Threading.Tasks;
using Constellation.Core.Shared;
using System.Threading;
using Constellation.Core.Models.StaffMembers.Errors;

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
        StaffMember staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));
        }

        staffMember.AddPhoneNumber(request.PhoneNumber);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

}