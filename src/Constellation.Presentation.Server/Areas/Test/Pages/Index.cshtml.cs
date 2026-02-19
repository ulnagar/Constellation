namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.AppSettings.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.Absences.Enums;
using Core.Models.AppSettings.Enums;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.StaffMembers.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IAppSettingsService appSettingsService,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _appSettingsService = appSettingsService;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task OnGet()
    {
        await MigrateAbsences();
        await MigrateCanvas();
        await MigrateContacts();
        await MigrateCovers();
        await MigrateLessons();
        await MigrateMandatoryTraining();
        await MigrateSentral();
        await MigrateTeams();
        await MigrateTutorials();
        await MigrateWorkFlows();
    }

    private async Task MigrateCovers()
    {
        StaffMember? evan = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1226239"));
        StaffMember? karen = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1112830"));

        Dictionary<StaffMember, List<Grade>> members = new();
        List<Grade> grades = [Grade.Y05, Grade.Y06, Grade.Y07, Grade.Y08, Grade.Y09, Grade.Y10, Grade.Y11, Grade.Y12];

        members.Add(evan, grades);
        members.Add(karen, grades);

        CoversConfiguration configuration = new(
            evan.Name.DisplayName,
            "Casual Coordinator",
            "0412 225 129",
            members);
        
        await _appSettingsService.Covers(configuration);
        await _unitOfWork.CompleteAsync();
    }

    private async Task MigrateLessons()
    {
        StaffMember? silvia = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1096783"));
        StaffMember? fiona = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1047595"));

        Dictionary<StaffMember, List<Grade>> members = new();
        List<Grade> grades = Enum.GetValues<Grade>().ToList();
        grades.Remove(Grade.SpecialProgram);

        members.Add(silvia, grades);

        LessonsConfiguration configuration = new(
            fiona.Name.DisplayName,
            "Science Practical Coordinator",
            fiona.EmailAddress.Email,
            members);

        await _appSettingsService.Lessons(configuration);
        await _unitOfWork.CompleteAsync();
    }

    private async Task MigrateMandatoryTraining()
    {
        StaffMember? thara = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1320079"));

        Dictionary<StaffMember, List<Grade>> members = new();
        List<Grade> grades = Enum.GetValues<Grade>().ToList();
        grades.Remove(Grade.SpecialProgram);

        members.Add(thara, grades);

        MandatoryTrainingConfiguration configuration = new(members);

        await _appSettingsService.MandatoryTraining(configuration);
        await _unitOfWork.CompleteAsync();
    }

    private async Task MigrateWorkFlows()
    {
        List<Grade> grades = Enum.GetValues<Grade>().ToList();
        grades.Remove(Grade.SpecialProgram);

        StaffMember? julie = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1077017"));
        StaffMember? carolyn = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1055721"));
        StaffMember? thara = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1320079"));

        Dictionary<StaffMember, List<Grade>> attendance = new();
        Dictionary<StaffMember, List<Grade>> compliance = new();
        Dictionary<StaffMember, List<Grade>> training = new();

        attendance.Add(julie, grades);
        compliance.Add(carolyn, grades);
        training.Add(thara, grades);

        WorkflowConfiguration attendanceConfiguration = new(WorkflowArea.Attendance, attendance);
        WorkflowConfiguration complianceConfiguration = new(WorkflowArea.Compliance, compliance);
        WorkflowConfiguration trainingConfiguration = new(WorkflowArea.Training, training);

        await _appSettingsService.Workflow(attendanceConfiguration);
        await _appSettingsService.Workflow(complianceConfiguration);
        await _appSettingsService.Workflow(trainingConfiguration);

        await _unitOfWork.CompleteAsync();
    }

    private async Task MigrateTutorials()
    {
        List<Grade> grades = Enum.GetValues<Grade>().ToList();
        grades.Remove(Grade.SpecialProgram);

        StaffMember? julie = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1077017"));
        StaffMember? tegan = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1198543"));

        Dictionary<StaffMember, List<Grade>> approver = new();
        Dictionary<StaffMember, List<Grade>> scheduler = new();

        approver.Add(julie, grades);
        scheduler.Add(tegan, grades);

        TutorialsConfiguration approverConfiguration = new(TutorialPosition.Approver, approver);
        TutorialsConfiguration schedulerConfiguration = new(TutorialPosition.Scheduler, scheduler);

        await _appSettingsService.Tutorials(approverConfiguration);
        await _appSettingsService.Tutorials(schedulerConfiguration);

        await _unitOfWork.CompleteAsync();
    }

    private async Task MigrateAbsences()
    {
        List<Grade> grades = Enum.GetValues<Grade>().ToList();
        grades.Remove(Grade.SpecialProgram);

        StaffMember? tegan = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1198543"));
        StaffMember? julie = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1077017"));

        Dictionary<StaffMember, List<Grade>> contacts = new();
        contacts.Add(tegan, grades);
        contacts.Add(julie, grades);

        List<AbsenceReason> partialReasons =
        [
            AbsenceReason.Flexible,
            AbsenceReason.Leave,
            AbsenceReason.SchoolBusiness,
            AbsenceReason.SharedEnrolment,
            AbsenceReason.Sick,
            AbsenceReason.Suspended
        ];

        List<AbsenceReason> wholeReasons =
        [
            AbsenceReason.Flexible,
            AbsenceReason.Leave,
            AbsenceReason.SchoolBusiness,
            AbsenceReason.Sick,
            AbsenceReason.Suspended
        ];

        AbsencesConfiguration configuration = new(
            10,
            "Aurora College",
            string.Empty,
            "auroracoll-h.school@det.nsw.edu.au",
            wholeReasons,
            partialReasons,
            contacts);

        await _appSettingsService.Absences(configuration);
        await _unitOfWork.CompleteAsync();
    }

    private async Task MigrateContacts()
    {
        List<Grade> allGrades = Enum.GetValues<Grade>().ToList();
        allGrades.Remove(Grade.SpecialProgram);

        StaffMember? amanda = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1017411"));
        StaffMember? kim = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("9222390"));
        StaffMember? virginia = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1188033"));
        StaffMember? hayley = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1167831"));
        StaffMember? benetta = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1126568"));
        StaffMember? dishanka = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1253229"));
        StaffMember? angela = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1348015"));
        StaffMember? walt = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1158684"));
        StaffMember? beth = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1102472"));
        StaffMember? carolyn = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1055721"));
        StaffMember? chris = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("8850192"));

        Dictionary<StaffMember, List<Grade>> members = [];
        members.Add(amanda, allGrades);

        ContactsConfiguration counsellorConfiguration = new(ContactPosition.Counsellor, members);
        await _appSettingsService.Contacts(counsellorConfiguration);

        members = [];
        members.Add(kim, allGrades);

        ContactsConfiguration careersAdvisorConfiguration = new(ContactPosition.CareersAdvisor, members);
        await _appSettingsService.Contacts(careersAdvisorConfiguration);

        members = [];
        members.Add(virginia, allGrades);

        ContactsConfiguration instructionalLeaderConfiguration = new(ContactPosition.InstructionalLeader, members);
        await _appSettingsService.Contacts(instructionalLeaderConfiguration);

        members = [];
        members.Add(hayley, allGrades);

        ContactsConfiguration librarianConfiguration = new(ContactPosition.Librarian, members);
        await _appSettingsService.Contacts(librarianConfiguration);

        members = [];
        members.Add(benetta, [ Grade.Y05, Grade.Y06 ]);
        members.Add(dishanka, [Grade.Y05, Grade.Y06]);
        members.Add(angela, [ Grade.Y07, Grade.Y08]);
        members.Add(walt, [ Grade.Y09, Grade.Y10]);
        members.Add(kim, [ Grade.Y11, Grade.Y12]);

        ContactsConfiguration lastConfiguration = new(ContactPosition.LearningSupport, members);
        await _appSettingsService.Contacts(lastConfiguration);

        members = [];
        members.Add(beth, [Grade.Y05, Grade.Y06, Grade.Y07, Grade.Y08, Grade.Y09]);
        members.Add(carolyn, [Grade.Y07, Grade.Y08, Grade.Y09, Grade.Y10, Grade.Y11, Grade.Y12]);

        ContactsConfiguration deputyConfiguration = new(ContactPosition.DeputyPrincipal, members);
        await _appSettingsService.Contacts(deputyConfiguration);

        members = [];
        members.Add(chris, allGrades);

        ContactsConfiguration principalConfiguration = new(ContactPosition.Principal, members);
        await _appSettingsService.Contacts(principalConfiguration);

        await _unitOfWork.CompleteAsync();
    }

    private async Task MigrateCanvas()
    {
        List<Grade> allGrades = Enum.GetValues<Grade>().ToList();
        allGrades.Remove(Grade.SpecialProgram);

        StaffMember? tegan = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1198543"));

        Dictionary<StaffMember, List<Grade>> members = [];
        members.Add(tegan, allGrades);

        CanvasConfiguration configuration = new(
            true,
            true,
            members);

        await _appSettingsService.Canvas(configuration);
        await _unitOfWork.CompleteAsync();
    }

    private async Task MigrateTeams()
    {
        List<Grade> allGrades = Enum.GetValues<Grade>().ToList();
        allGrades.Remove(Grade.SpecialProgram);

        StaffMember? ben = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1206070"));
        StaffMember? michael = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1415806"));
        StaffMember? chrisR = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("8850192"));
        StaffMember? carolyn = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1055721"));
        StaffMember? beth = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1102472"));
        StaffMember? virginia = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1188033"));
        StaffMember? julie = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1077017"));
        StaffMember? cassandra = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1278134"));
        StaffMember? silvia = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1096783"));
        StaffMember? karen = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1112830"));
        StaffMember? lisa = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1263351"));
        StaffMember? tegan = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1198543"));
        StaffMember? chrisH = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1119949"));
        StaffMember? walt = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1158684"));
        StaffMember? hayley = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("1167831"));
        StaffMember? kim = await _staffRepository.GetByEmployeeId(EmployeeId.FromValue("9222390"));

        Dictionary<StaffMember, List<Grade>> mandatoryOwners = new();
        Dictionary<StaffMember, List<Grade>> studentTeamOwners = new();
        Dictionary<StaffMember, List<Grade>> studentChannelOwners = new();

        mandatoryOwners.Add(virginia, allGrades);
        mandatoryOwners.Add(ben, allGrades);
        mandatoryOwners.Add(michael, allGrades);
        mandatoryOwners.Add(chrisR, allGrades);
        mandatoryOwners.Add(julie, allGrades);
        mandatoryOwners.Add(carolyn, allGrades);
        mandatoryOwners.Add(tegan, allGrades);
        mandatoryOwners.Add(beth, allGrades);

        studentTeamOwners.Add(ben, allGrades);
        studentTeamOwners.Add(michael, allGrades);
        studentTeamOwners.Add(chrisR, allGrades);
        studentTeamOwners.Add(carolyn, allGrades);
        studentTeamOwners.Add(beth, allGrades);
        studentTeamOwners.Add(virginia, allGrades);
        studentTeamOwners.Add(julie, allGrades);
        studentTeamOwners.Add(cassandra, allGrades);
        studentTeamOwners.Add(silvia, allGrades);
        studentTeamOwners.Add(karen, allGrades);
        studentTeamOwners.Add(lisa, allGrades);
        studentTeamOwners.Add(chrisH, allGrades);
        studentTeamOwners.Add(walt, allGrades);
        studentTeamOwners.Add(hayley, allGrades);
        studentTeamOwners.Add(kim, allGrades);

        studentChannelOwners.Add(ben, allGrades);
        studentChannelOwners.Add(virginia, allGrades);
        studentChannelOwners.Add(carolyn, allGrades);
        studentChannelOwners.Add(chrisR, allGrades);
        studentChannelOwners.Add(beth, allGrades);
        studentChannelOwners.Add(julie, allGrades);
        studentChannelOwners.Add(kim, [ Grade.Y11, Grade.Y12 ]);

        TeamsConfiguration configuration = new(mandatoryOwners, studentTeamOwners, studentChannelOwners);
        await _appSettingsService.Teams(configuration);
        await _unitOfWork.CompleteAsync();
    }

    private async Task MigrateSentral()
    {
        SentralConfiguration familyEmail = new(SentralPath.FamilyEmail, "//*[@id='expander-content-1']/table/tr/td[1]/table/tr[4]/td");
        SentralConfiguration absenceTable = new(SentralPath.AbsenceTable, "//*[@id='layout-2col-content']/div/div[3]/div[2]/table/tbody");
        SentralConfiguration studentTable = new(SentralPath.StudentTable, "//*[@id='layout-2col-content']/div/div[1]/div/div[2]/table/tbody");
        SentralConfiguration partialAbsenceTable = new(SentralPath.PartialAbsenceTable, "//*[@id='student-absences-list']/table/tbody");
        SentralConfiguration calendarTable = new(SentralPath.CalendarTable, "//*[@id='layout-2col-content']/div/div/div[2]/div/table[1]");
        SentralConfiguration termCalendarTable = new(SentralPath.TermCalendarTable, "//*[@id='layout-2col-content']/div/div/div[2]/div/div/table");
        SentralConfiguration studentAwardList = new(SentralPath.WellbeingStudentAwardsList, "//*[@id='layout-2col-content']/div/div/div[2]/table/tbody");
        SentralConfiguration incidentDate = new(SentralPath.IncidentCreatedDate, "//*[@id='layout-3col-content']/div/div[1]/div[1]/div[1]/div[1]/div");
        SentralConfiguration indigenousStatus = new(SentralPath.IndigenousStatus,"//*[@id=\"expander-content-1\"]/table/tr/td[1]/table/tr[7]/td");
        SentralConfiguration srnTable = new(SentralPath.StudentSRNTable, "/html/body/div[8]/div/div[2]/div[3]/div/div/div/div[2]/table");
        SentralConfiguration enrolmentDates = new(SentralPath.StudentEnrolmentDates, "//*[contains(@class, 'pxp-roll')]");

        await _appSettingsService.Sentral(familyEmail);
        await _appSettingsService.Sentral(absenceTable);
        await _appSettingsService.Sentral(studentTable);
        await _appSettingsService.Sentral(partialAbsenceTable);
        await _appSettingsService.Sentral(calendarTable);
        await _appSettingsService.Sentral(termCalendarTable);
        await _appSettingsService.Sentral(studentAwardList);
        await _appSettingsService.Sentral(incidentDate);
        await _appSettingsService.Sentral(indigenousStatus);
        await _appSettingsService.Sentral(srnTable);
        await _appSettingsService.Sentral(enrolmentDates);

        await _unitOfWork.CompleteAsync();
    }
}