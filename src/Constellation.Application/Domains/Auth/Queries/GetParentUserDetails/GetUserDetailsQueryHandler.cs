namespace Constellation.Application.Domains.Auth.Queries.GetParentUserDetails;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.Families;
using Core.Models.Identifiers;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Models.Identity.Errors;
using Models.Identity.Repositories;
using Serilog;
using System.Threading.Tasks;

internal sealed class GetParentUserDetailsQueryHandler
: IQueryHandler<GetParentUserDetailsQuery, ParentUserResponse>
{
    private readonly IIdentityRepository _identityRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetParentUserDetailsQueryHandler(
        IIdentityRepository identityRepository,
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _identityRepository = identityRepository;
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetParentUserDetailsQuery>();
    }

    public async Task<Result<ParentUserResponse>> Handle(GetParentUserDetailsQuery request, CancellationToken cancellationToken)
    {
        AppUser? user = await _identityRepository.GetUser(request.Id, cancellationToken);

        if (user is null)
        {
            _logger
                .ForContext(nameof(GetParentUserDetailsQuery), request, true)
                .ForContext(nameof(Error), AuthErrors.UserNotFound(request.Id), true)
                .Warning("Failed to retrieve User details");

            return Result.Failure<ParentUserResponse>(AuthErrors.UserNotFound(request.Id));
        }

        List<PhoneNumber> phoneNumbers = [];

        foreach (var link in user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Parent))
        {
            ParentId parentId = ParentId.FromValue(link.LinkId);

            if (parentId == ParentId.Empty)
                continue;

            Parent? parent = await _familyRepository.GetParentById(parentId, cancellationToken);

            if (parent is null)
                continue;

            if (!phoneNumbers.Contains(parent.MobileNumber))
                phoneNumbers.Add(parent.MobileNumber);
        }
        
        List<ParentUserResponse.Student> students = [];

        Dictionary<StudentId, bool> studentIds = await _familyRepository.GetStudentIdsFromFamilyWithEmail(user.Email, cancellationToken);

        foreach (var studentLink in studentIds)
        {
            Student? student = await _studentRepository.GetById(studentLink.Key, cancellationToken);

            if (student is null)
                continue;

            students.Add(new(
                student.Name,
                student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                student.CurrentEnrolment?.SchoolName ?? string.Empty));
        }
        
        List<ParentUserResponse.Passkey> passkeys = [];

        foreach (AppUserPasskey passkey in user.PasskeyCredentials)
        {
            passkeys.Add(new(
                passkey.Name,
                passkey.CreatedAt,
                passkey.CredentialId));
        }

        ParentUserResponse response = new(
            user.Id,
            user.Name,
            user.Email,
            phoneNumbers, 
            students,
            passkeys);

        return response;
    }
}
