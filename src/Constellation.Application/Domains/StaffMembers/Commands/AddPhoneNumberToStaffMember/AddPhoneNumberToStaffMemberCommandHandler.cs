namespace Constellation.Application.Domains.StaffMember.Commands.AddPhoneNumberToStaffMember

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
        
}