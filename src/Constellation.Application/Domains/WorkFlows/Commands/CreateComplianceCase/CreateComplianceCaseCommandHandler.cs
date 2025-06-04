namespace Constellation.Application.Domains.WorkFlows.Commands.CreateComplianceCase;

using Abstractions.Messaging;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Repositories;
using Core.Models.WorkFlow.Services;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateComplianceCaseCommandHandler
: ICommandHandler<CreateComplianceCaseCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ICaseRepository _caseRepository;
    private readonly ICaseService _caseService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateComplianceCaseCommandHandler(
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IStaffRepository staffRepository,
        ICaseRepository caseRepository,
        ICaseService caseService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _staffRepository = staffRepository;
        _caseRepository = caseRepository;
        _caseService = caseService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateComplianceCaseCommand>();
    }

    public async Task<Result> Handle(CreateComplianceCaseCommand request, CancellationToken cancellationToken)
    {
        Result<StudentReferenceNumber> srn = StudentReferenceNumber.Create(request.Incident.StudentReferenceNumber);

        if (srn.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateComplianceCaseCommand), request, true)
                .ForContext(nameof(Error), srn.Error, true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(srn.Error);
        }

        Student student = await _studentRepository.GetBySRN(srn.Value, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(CreateComplianceCaseCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFoundBySRN(srn.Value), true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(StudentErrors.NotFoundBySRN(srn.Value));
        }

        string[] nameFragments = request.Incident.Teacher.Split(',');
        string firstName = nameFragments[1].Remove(nameFragments[1].IndexOf('(')).Trim();
        string lastName = nameFragments[0].Trim();
        string staffName = $"{firstName} {lastName}";

        StaffMember teacher = await _staffRepository.GetFromName(staffName, cancellationToken);

        if (teacher is null)
        {
            _logger
                .ForContext(nameof(CreateComplianceCaseCommand), request, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(StaffId.Empty), true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(StaffMemberErrors.NotFound(StaffId.Empty));
        }

        Result<Case> caseResult = await _caseService.CreateComplianceCase(
            student.Id,
            teacher.Id, 
            request.Incident.IncidentId, 
            request.Incident.Type,
            request.Incident.Subject,
            request.Incident.DateCreated,
            cancellationToken);

        if (caseResult.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateComplianceCaseCommand), request, true)
                .ForContext(nameof(Error), caseResult.Error, true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(caseResult.Error);
        }

        _caseRepository.Insert(caseResult.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
