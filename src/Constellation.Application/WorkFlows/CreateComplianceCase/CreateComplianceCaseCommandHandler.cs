namespace Constellation.Application.WorkFlows.CreateComplianceCase;

using Abstractions.Messaging;
using Constellation.Application.WorkFlows.CreateAttendanceCase;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Repositories;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students.Repositories;
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
        Student student = await _studentRepository.GetBySRN(request.Incident.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(CreateComplianceCaseCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.Incident.StudentId), true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(StudentErrors.NotFound(request.Incident.StudentId));
        }

        School school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(CreateComplianceCaseCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(student.SchoolCode), true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(DomainErrors.Partners.School.NotFound(student.SchoolCode));
        }

        string[] nameFragments = request.Incident.Teacher.Split(',');
        string firstName = nameFragments[1].Remove(nameFragments[1].IndexOf('(')).Trim();
        string lastName = nameFragments[0].Trim();
        string staffName = $"{firstName} {lastName}";

        Staff teacher = await _staffRepository.GetFromName(staffName);

        if (teacher is null)
        {
            _logger
                .ForContext(nameof(CreateComplianceCaseCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(string.Empty), true)
                .Warning("Failed to create WorkFlow Case");

            return Result.Failure(DomainErrors.Partners.Staff.NotFound(string.Empty));
        }

        Result<Case> caseResult = await _caseService.CreateComplianceCase(
            student.StudentId, 
            teacher.StaffId, 
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
