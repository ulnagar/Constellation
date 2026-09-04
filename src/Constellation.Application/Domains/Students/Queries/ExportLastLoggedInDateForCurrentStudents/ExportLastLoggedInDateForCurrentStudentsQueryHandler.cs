namespace Constellation.Application.Domains.Students.Queries.ExportLastLoggedInDateForCurrentStudents;

using Abstractions.Messaging;
using Application.Models.Identity.Repositories;
using Constellation.Application.Domains.Students.Models;
using Core.Enums;
using Core.Extensions;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using GetLastLoggedInDateForCurrentStudents;
using Interfaces.Services.Excel;
using Models;
using Serilog;

internal sealed class ExportLastLoggedInDateForCurrentStudentsQueryHandler
    : IQueryHandler<ExportLastLoggedInDateForCurrentStudentsQuery, byte[]>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IIdentityRepository _identityRepository;
    private readonly IExcelWriter _writer;
    private readonly ILogger _logger;

    public ExportLastLoggedInDateForCurrentStudentsQueryHandler(
        IStudentRepository studentRepository,
        IIdentityRepository identityRepository,
        IExcelWriter writer,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _identityRepository = identityRepository;
        _writer = writer;
        _logger = logger
            .ForContext<ExportLastLoggedInDateForCurrentStudentsQuery>();
    }

    public async Task<Result<byte[]>> Handle(ExportLastLoggedInDateForCurrentStudentsQuery request, CancellationToken cancellationToken)
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

        IExcelWorkbook workbook = _writer.CreateWorkbook();
        IExcelWorksheet sheet = _writer.AddWorksheet(workbook, "Sheet 1");

        _writer.WriteRange(sheet, 2, loginData,
            new("SRN", a => a.StudentReferenceNumber?.Value ?? string.Empty),
            new("First Name", a => a.Student.FirstName),
            new("Preferred Name", a => a.Student.PreferredName),
            new("Last Name", a => a.Student.LastName),
            new("Email Address", a => a.EmailAddress.Email),
            new("Grade", a => a.Grade.AsNumber(), ExcelColumnFormat.Text),
            new("School", a => a.SchoolName),
            new("Last Logged In", a => a.LastLoginTime?.ToLocalTime(), ExcelColumnFormat.DateTime));

        _writer.ApplyHeaderStyle(sheet, 1);
        _writer.AddAutoFilter(sheet);
        _writer.AutoFitColumns(sheet);

        byte[] file = _writer.GetAsByteArray(workbook);

        return file;
    }
}