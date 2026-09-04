namespace Constellation.Application.Domains.Students.Queries.GetLastLoggedInDateForCurrentStudents;

using Abstractions.Messaging;
using Application.Models.Identity.Repositories;
using Constellation.Application.Domains.Students.Models;
using Core.Enums;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;

internal sealed class GetLastLoggedInDateForCurrentStudentsQueryHandler
: IQueryHandler<GetLastLoggedInDateForCurrentStudentsQuery, List<StudentLoginData>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IIdentityRepository _identityRepository;
    private readonly ILogger _logger;

    public GetLastLoggedInDateForCurrentStudentsQueryHandler(
        IStudentRepository studentRepository,
        IIdentityRepository identityRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _identityRepository = identityRepository;
        _logger = logger
            .ForContext<GetLastLoggedInDateForCurrentStudentsQuery>();
    }

    public async Task<Result<List<StudentLoginData>>> Handle(GetLastLoggedInDateForCurrentStudentsQuery request, CancellationToken cancellationToken)
    {
        List<StudentLoginData> loginData = [];

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        List<AppUser> users = await _identityRepository.GetUsers(cancellationToken);

        foreach (Student student in students)
        {
            AppUser? user = users.FirstOrDefault(entry => 
                entry.IsStudent 
                && entry.Links.Any(link =>
                    !link.IsDeleted 
                    && link.Type == LinkType.Student 
                    && link.LinkId == student.Id.Value));

            AppUserLoginAttempt? loginTime = user?.Logins
                .Where(entry => entry.Status == LoginStatus.SingleSignOn || entry.Status == LoginStatus.Success)
                .MaxBy(entry => entry.LoginDateTime);

            loginData.Add(new(
                student.StudentReferenceNumber,
                student.Name,
                student.CurrentEnrolment?.Grade ?? Grade.SpecialProgram,
                student.EmailAddress,
                student.CurrentEnrolment?.SchoolName ?? string.Empty,
                loginTime?.LoginDateTime));
        }

        return loginData;
    }
}
