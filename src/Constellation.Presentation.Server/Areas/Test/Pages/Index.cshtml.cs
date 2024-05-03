namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.DTOs.Canvas;
using Application.Interfaces.Gateways;
using Application.Interfaces.Jobs;
using Application.Offerings.GetOfferingsWithCanvasUserRecords;
using BaseModels;
using Core.Abstractions.Clock;
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
        bool useGroups = false;

        List<CourseListEntry> coursesInCanvas = await _gateway.GetAllCourses(_dateTime.CurrentYear.ToString());
        
        Result<List<OfferingSummaryWithUsers>> offeringRequest = await _mediator.Send(new GetOfferingsWithCanvasUserRecordsQuery());

        if (offeringRequest.IsFailure)
            return;

        List<OfferingSummaryWithUsers> offerings = offeringRequest.Value;
        
        foreach (CourseListEntry canvasCourse in coursesInCanvas)
        {
            List<CourseEnrolmentEntry> canvasEnrolments = await _gateway.GetEnrolmentsForCourse(canvasCourse.CourseCode);

            List<OfferingSummaryWithUsers> offeringList = offerings
                .Where(entry => entry.CanvasResourceIds.Contains(canvasCourse.CourseCode))
                .ToList();

            List<OfferingSummaryWithUsers.User> offeringUsers = offeringList.SelectMany(entry => entry.Users).ToList();

            List<OfferingSummaryWithUsers.User> missingCanvasEnrolments = offeringUsers
                .Where(entry => 
                    canvasEnrolments.All(user => user.UserId != entry.Id))
                .ToList();

            foreach (OfferingSummaryWithUsers.User missingEnrolment in missingCanvasEnrolments)
            {
                _logger.Information("Adding {user} to {course}", missingEnrolment.Id, canvasCourse.CourseCode);

                bool enrolAttempt = await _gateway.EnrolUser(missingEnrolment.Id, canvasCourse.CourseCode, missingEnrolment.AccessLevel.ToString());

                if (!useGroups)
                    continue;

                string year = canvasCourse.CourseCode.Split('-')[1];
                string canvasGroupCode = $"{year}-{offeringList.First(entry => entry.Users.Contains(missingEnrolment)).Name}";

                bool addToGroupAttempt = await _gateway.AddUserToGroup(missingEnrolment.Id, canvasGroupCode);
            }

            List<CourseEnrolmentEntry> extraCanvasEnrolments = canvasEnrolments
                .Where(entry => 
                    offeringUsers.All(user => user.Id != entry.UserId))
                .ToList();

            foreach (CourseEnrolmentEntry canvasEnrolment in extraCanvasEnrolments)
            {
                _logger.Information("Removing {user} from {course}", canvasEnrolment.UserId, canvasCourse.CourseCode);
                
                bool unenrolAttempt = await _gateway.UnenrolUser(canvasEnrolment.UserId, canvasEnrolment.CourseCode);
            }

            if (!useGroups)
                continue;

            foreach (OfferingSummaryWithUsers offering in offeringList)
            {
                string year = canvasCourse.CourseCode.Split('-')[1];
                string canvasGroupCode = $"{year}-{offering.Name}";

                List<string> usersInGroup = await _gateway.GetGroupMembers(canvasGroupCode);

                List<OfferingSummaryWithUsers.User> missingGroupMembers = offering.Users.Where(entry => !usersInGroup.Contains(entry.Id)).ToList();

                foreach (OfferingSummaryWithUsers.User missingGroupMember in missingGroupMembers)
                {
                    bool addToGroupAttempt = await _gateway.AddUserToGroup(missingGroupMember.Id, canvasGroupCode);
                }

                List<string> extraGroupMembers = usersInGroup.Where(entry => offering.Users.All(user => user.Id != entry)).ToList();

                foreach (string extraGroupMember in extraGroupMembers)
                {
                    bool removeFromGroupAttempt = await _gateway.RemoveUserFromGroup(extraGroupMember, canvasGroupCode);
                }
            }
        }
    }
}