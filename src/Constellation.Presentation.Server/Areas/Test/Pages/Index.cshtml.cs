namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Canvas.GetCourseMembershipByCourseCode;
using Application.DTOs.Canvas;
using Application.Interfaces.Gateways;
using Application.Interfaces.Jobs;
using BaseModels;
using Core.Abstractions.Clock;
using Core.Models.Canvas.Models;
using Core.Shared;
using MediatR;
using Serilog;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICanvasGateway _gateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICanvasGateway gateway,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _mediator = mediator;
        _gateway = gateway;
        _dateTime = dateTime;
        _logger = logger.ForContext<IPermissionUpdateJob>();
    }

    public void OnGet() { }

    public async Task OnGetRunCode()
    {
        bool useGroups = true;
        bool useSections = true;

        List<CourseListEntry> coursesInCanvas = await _gateway.GetAllCourses(_dateTime.CurrentYear.ToString());
        
        foreach (CourseListEntry canvasCourse in coursesInCanvas)
        {
            List<CourseEnrolmentEntry> canvasEnrolments = await _gateway.GetEnrolmentsForCourse(canvasCourse.CourseCode);

            CanvasCourseCode courseCode = new(canvasCourse.CourseCode);

            Result<List<CanvasCourseMembership>> calculatedMembers = await _mediator.Send(new GetCourseMembershipByCourseCodeQuery(courseCode));

            if (calculatedMembers.IsFailure)
            {
                _logger
                    .ForContext(nameof(CanvasCourseCode), courseCode.Value)
                    .Warning("Error getting expected members of Canvas Course");

                continue;
            }

            List<CanvasCourseMembership> missingCanvasEnrolments = calculatedMembers.Value
                .Where(entry => 
                    canvasEnrolments.All(user => user.UserId != entry.UserId))
                .ToList();

            foreach (CanvasCourseMembership missingEnrolment in missingCanvasEnrolments)
            {
                _logger.Information("Adding {user} to {course}", missingEnrolment.UserId, canvasCourse.CourseCode);

                bool enrolAttempt = useSections switch
                {
                    true when missingEnrolment.PermissionLevel == CanvasPermissionLevel.Student =>
                        await _gateway.EnrolToSection(missingEnrolment.UserId, missingEnrolment.SectionId, missingEnrolment.PermissionLevel.ToString()),
                    true =>
                        await _gateway.EnrolToCourse(missingEnrolment.UserId, canvasCourse.CourseCode, missingEnrolment.PermissionLevel.ToString()),
                    false =>
                        await _gateway.EnrolToCourse(missingEnrolment.UserId, canvasCourse.CourseCode, missingEnrolment.PermissionLevel.ToString()),
                };

                if (!enrolAttempt)
                {
                    _logger
                        .ForContext(nameof(CanvasCourseCode), courseCode.Value)
                        .ForContext(nameof(CanvasCourseMembership), missingEnrolment, true)
                        .Warning("Failed to enrol missing member into Canvas Course");

                    continue;
                }

                if (!useGroups || missingEnrolment.PermissionLevel != CanvasPermissionLevel.Student) continue;

                bool addToGroupAttempt = await _gateway.AddUserToGroup(missingEnrolment.UserId, missingEnrolment.SectionId);

                if (!addToGroupAttempt)
                {
                    _logger
                        .ForContext(nameof(CanvasCourseCode), courseCode.Value)
                        .ForContext(nameof(CanvasCourseMembership), missingEnrolment, true)
                        .ForContext("GroupCode", missingEnrolment.SectionId)
                        .Warning("Failed to add missing member to Course Group");
                }
            }

            List<CourseEnrolmentEntry> extraCanvasEnrolments = canvasEnrolments
                .Where(entry => 
                    calculatedMembers.Value.All(user => user.UserId != entry.UserId))
                .ToList();

            foreach (CourseEnrolmentEntry canvasEnrolment in extraCanvasEnrolments)
            {
                _logger.Information("Removing {user} from {course}", canvasEnrolment.UserId, canvasCourse.CourseCode);
                
                bool unenrolAttempt = await _gateway.UnenrolUser(canvasEnrolment.UserId, canvasEnrolment.CourseCode);

                if (!unenrolAttempt)
                {
                    _logger
                        .ForContext(nameof(CanvasCourseCode), courseCode.Value)
                        .ForContext(nameof(CourseEnrolmentEntry), canvasEnrolment, true)
                        .Warning("Failed to remove extra member from Canvas Course");
                }
            }
            
            if (useGroups)
            {
                foreach (var group in calculatedMembers.Value.GroupBy(entry => entry.SectionId))
                {
                    if (string.IsNullOrWhiteSpace(group.Key))
                        continue;

                    List<string> usersInGroup = await _gateway.GetGroupMembers(group.Key);

                    List<CanvasCourseMembership> missingGroupMembers = group.Where(entry => !usersInGroup.Contains(entry.UserId)).ToList();

                    foreach (CanvasCourseMembership missingGroupMember in missingGroupMembers)
                    {
                        bool addToGroupAttempt = await _gateway.AddUserToGroup(missingGroupMember.UserId, group.Key);

                        if (!addToGroupAttempt)
                            _logger
                                .ForContext(nameof(CanvasCourseCode), courseCode.Value)
                                .ForContext(nameof(CanvasCourseMembership), missingGroupMember, true)
                                .ForContext("GroupCode", group.Key)
                                .Warning("Failed to add missing member to Course Group");
                    }

                    List<string> extraGroupMembers = usersInGroup.Where(entry => group.All(user => user.UserId != entry)).ToList();

                    foreach (string extraGroupMember in extraGroupMembers)
                    {
                        bool removeFromGroupAttempt = await _gateway.RemoveUserFromGroup(extraGroupMember, group.Key);

                        if (!removeFromGroupAttempt)
                            _logger
                                .ForContext(nameof(CanvasCourseCode), courseCode.Value)
                                .ForContext(nameof(extraGroupMember), extraGroupMember)
                                .ForContext("GroupCode", group.Key)
                                .Warning("Failed to remove extra member from Course Group");
                    }
                }
            }

            if (useSections)
            {
                foreach (CanvasCourseMembership calculatedMember in calculatedMembers.Value)
                {
                    CourseEnrolmentEntry matchingCanvasEnrolment = canvasEnrolments.FirstOrDefault(entry => entry.UserId == calculatedMember.UserId);

                    if (matchingCanvasEnrolment is null)
                        continue;

                    if (calculatedMember.SectionId != matchingCanvasEnrolment.SectionCode)
                    {
                        await _gateway.UnenrolUser(calculatedMember.UserId, )


                        await _gateway.EnrolToSection(calculatedMember.UserId, calculatedMember.SectionId,
                            calculatedMember.PermissionLevel.ToString());
                    }

                }
            }
        }
    }
}