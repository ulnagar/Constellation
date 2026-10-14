namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.ExportApplicationsList;

using Abstractions.Messaging;
using Application.Interfaces.Services.Excel;
using Core.Extensions;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentParent;
using Core.Models.EnrolmentContext.EnrolmentStudent;
using Core.Models.EnrolmentContext.EnrolmentStudent.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentStudent.Repositories;
using Core.Shared;
using Serilog;

internal sealed class ExportApplicationsListQueryHandler
: IQueryHandler<ExportApplicationsListQuery, byte[]>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentStudentRepository _studentRepository;
    private readonly IExcelWriter _writer;
    private readonly ILogger _logger;

    public ExportApplicationsListQueryHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentStudentRepository studentRepository,
        IExcelWriter writer,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _studentRepository = studentRepository;
        _writer = writer;
        _logger = logger
            .ForContext<ExportApplicationsListQuery>();
    }

    public async Task<Result<byte[]>> Handle(ExportApplicationsListQuery request, CancellationToken cancellationToken)
    {
        List<Application> applications = await _applicationRepository.GetListFromIds(request.ApplicationIds, cancellationToken);

        if (applications.Count == 0)
            return Result.Failure<byte[]>(EnrolmentApplicationErrors.NoneFound);

        int maxParents = applications.Select(a => a.Parents.Count).DefaultIfEmpty(0).Max();

        List<EnrolmentStudentId> studentIds = applications.Select(a => a.StudentId).Distinct().ToList();

        List<EnrolmentStudent> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);
        
        IExcelWorkbook workbook = _writer.CreateWorkbook();
        IExcelWorksheet sheet = _writer.AddWorksheet(workbook, "Sheet 1");

        List<ExcelColumn<Application>> columns =
        [
            new("Application Reference", a => a.ApplicationReference),
            new("SRN", a => StudentFor(a, students)?.StudentReferenceNumber?.Value ?? ""),
            new("Student Family Name", a => StudentFor(a, students)?.Name.LastName),
            new("Student Given Names", a => StudentFor(a, students)?.Name.FirstName),
            new("Student Preferred Name", a => StudentFor(a, students)?.Name.PreferredName),
            new("Full Name", a => StudentFor(a, students)?.Name.DisplayName),
            new("Student Email", a => StudentFor(a, students)?.StudentEmailAddress?.Email ?? ""),
            new("DoB", a => StudentFor(a, students)?.DateOfBirth, ExcelColumnFormat.Date),
            new("Gender", a => StudentFor(a, students)?.StudentGender?.Name ?? "Unknown"),
            new("Cohort", a => a.Grade.AsNumber(), ExcelColumnFormat.Text),
            new("Courses", a => 
                String.Join("; ", a.SelectedCourses
                    .Where(entry => entry.Status <= CourseSelectionStatus.Approved)
                    .Select(entry => $"{entry.Course.Name} ({entry.Line})"))),
            new("Current School Code", a => a.CurrentSchoolCode, ExcelColumnFormat.Text),
            new("Current School", a => a.CurrentSchool),
            new("Destination School Code", a => a.DestinationSchoolCode, ExcelColumnFormat.Text),
            new("Destination School", a => a.DestinationSchool)
        ];

        for (int i = 0; i < maxParents; i++)
        {
            int index = i;
            string prefix = $"Parent {index + 1}";

            columns.Add(new($"{prefix} First Name", a => ParentAt(a, index)?.Name.FirstName));
            columns.Add(new($"{prefix} Last Name", a => ParentAt(a, index)?.Name.LastName));
            columns.Add(new($"{prefix} Street Address", a => ParentAt(a, index)?.MailingAddress?.Street));
            columns.Add(new($"{prefix} Town", a => ParentAt(a, index)?.MailingAddress?.Town));
            columns.Add(new($"{prefix} State", a => ParentAt(a, index)?.MailingAddress?.State));
            columns.Add(new($"{prefix} PostCode", a => ParentAt(a, index)?.MailingAddress?.Postcode, ExcelColumnFormat.Text));
            columns.Add(new($"{prefix} Phone Number", a => ParentAt(a, index)?.PhoneNumber));
            columns.Add(new($"{prefix} Email", a => ParentAt(a, index)?.Email));
        }

        columns.Add(new("Status", a => a.Status.ToString()));

        _writer.WriteRange(sheet, 2, applications, columns.ToArray());

        _writer.ApplyHeaderStyle(sheet, 1);
        _writer.AddAutoFilter(sheet);
        _writer.AutoFitColumns(sheet);

        byte[] file = _writer.GetAsByteArray(workbook);

        return file;
    }

    static EnrolmentParent? ParentAt(Application application, int index) =>
        index < application.Parents.Count ? application.Parents[index] : null;

    static EnrolmentStudent? StudentFor(Application application, List<EnrolmentStudent> students) =>
        students.FirstOrDefault(entry => entry.Id == application.StudentId);
}
