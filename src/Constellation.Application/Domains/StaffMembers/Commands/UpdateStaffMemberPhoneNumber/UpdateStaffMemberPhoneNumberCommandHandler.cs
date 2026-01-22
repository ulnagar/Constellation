namespace Constellation.Application.Domains.StaffMembers.Commands.UpdateStaffMemberPhoneNumber;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class UpdateStaffMemberPhoneNumberCommandHandler
    : ICommandHandler<UpdateStaffMemberPhoneNumberCommand>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;


    public UpdateStaffMemberPhoneNumberCommandHandler(
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateStaffMemberPhoneNumberCommand>();     
    }

    public async Task<Result> Handle(UpdateStaffMemberPhoneNumberCommand request, CancellationToken cancellationToken)
    {
        StaffMember? staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
            return Result.Failure(StaffMemberErrors.NotFound(request.StaffId));

        staffMember.AddPhoneNumber(request.PhoneNumber);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

}