namespace Constellation.Infrastructure.Jobs;

using Application.Canvas.GetCourseMembershipByCourseCode;
using Application.DTOs.Canvas;
using Application.Features.API.Operations.Commands;
using Application.Interfaces.Gateways;
using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Jobs;
using Core.Abstractions.Clock;
using Core.Models.Canvas.Models;
using Core.Models.Operations;
using Core.Shared;
using ExternalServices.Canvas;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CanvasAccessAuditJob : ICanvasAccessAuditJob
{
    private readonly CanvasGatewayConfiguration _configuration;
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly ICanvasGateway _gateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly ISender _mediator;
    private readonly ILogger _logger;

    public CanvasAccessAuditJob(
        IOptions<CanvasGatewayConfiguration> configuration,
        ICanvasOperationsRepository operationsRepository,
        ICanvasGateway gateway,
        IDateTimeProvider dateTime,
        ISender mediator,
        ILogger logger)
    {
        _configuration = configuration.Value;
        _operationsRepository = operationsRepository;
        _gateway = gateway;
        _dateTime = dateTime;
        _mediator = mediator;
        _logger = logger.ForContext<ICanvasAccessAuditJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.ForContext("JobId", jobId);

        _logger.Information("Starting script run");

        List<CanvasOperation> operations = await _operationsRepository.AllToProcess(cancellationToken);
        
        _logger.Information("Found {count} operations to process.", operations.Count);
        
        foreach (CanvasOperation operation in operations)
            await _mediator.Send(new ProcessCanvasOperationCommand(operation.Id, _configuration.UseSections), cancellationToken);

        _logger.Information("Auditing Canvas Enrolments");

        List<CourseListEntry> coursesInCanvas = await _gateway.GetAllCourses(_dateTime.CurrentYear.ToString(), cancellationToken);

        foreach (CourseListEntry canvasCourse in coursesInCanvas.OrderBy(entry => entry.CourseCode))
        {
            _logger.Information("Processing course {canvasCourse}", canvasCourse.Name);

            List<CourseEnrolmentEntry> canvasEnrolments =
                await _gateway.GetEnrolmentsForCourse(canvasCourse.CourseCode, cancellationToken);

            Result<List<CanvasCourseMembership>> calculatedMembers =
                await _mediator.Send(new GetCourseMembershipByCourseCodeQuery(canvasCourse.CourseCode), cancellationToken);

            if (calculatedMembers.IsFailure)
            {
                _logger
                    .ForContext(nameof(CanvasCourseCode), canvasCourse.CourseCode)
                    .Warning("Error getting expected members of Canvas Course");

                continue;
            }

            List<CanvasCourseMembership> missingCanvasEnrolments = calculatedMembers.Value
                .Where(entry =>
                    canvasEnrolments.All(user => user.UserId != entry.UserId))
                .ToList();

            foreach (CanvasCourseMembership missingEnrolment in missingCanvasEnrolments)
            {
                _logger.Information("Adding {user} to {course} in section {section}", missingEnrolment.UserId, canvasCourse.CourseCode, missingEnrolment.SectionId);

                bool enrolAttempt = _configuration.UseSections switch
                {
                    true when missingEnrolment.PermissionLevel == CanvasPermissionLevel.Student =>
                        await _gateway.EnrolToSection(missingEnrolment.UserId, missingEnrolment.SectionId, missingEnrolment.PermissionLevel, cancellationToken),
                    true =>
                        await _gateway.EnrolToCourse(missingEnrolment.UserId, canvasCourse.CourseCode, missingEnrolment.PermissionLevel, cancellationToken),
                    false =>
                        await _gateway.EnrolToCourse(missingEnrolment.UserId, canvasCourse.CourseCode, missingEnrolment.PermissionLevel, cancellationToken),
                };

                if (!enrolAttempt)
                {
                    _logger
                        .ForContext(nameof(CanvasCourseCode), canvasCourse.CourseCode)
                        .ForContext(nameof(CanvasCourseMembership), missingEnrolment, true)
                        .Warning("Failed to enrol missing member into Canvas Course");

                    continue;
                }

                if (!_configuration.UseGroups || missingEnrolment.PermissionLevel != CanvasPermissionLevel.Student) continue;

                _logger.Information("Adding {user} to group {group} in course {course}", missingEnrolment.UserId, missingEnrolment.SectionId, canvasCourse.CourseCode);
                
                bool addToGroupAttempt = await _gateway.AddUserToGroup(missingEnrolment.UserId, missingEnrolment.SectionId, cancellationToken);

                if (!addToGroupAttempt)
                {
                    _logger
                        .ForContext(nameof(CanvasCourseCode), canvasCourse.CourseCode)
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

                bool unenrolAttempt = await _gateway.UnenrolUser(canvasEnrolment.UserId, canvasEnrolment.CourseCode, cancellationToken);

                if (!unenrolAttempt)
                {
                    _logger
                        .ForContext(nameof(CanvasCourseCode), canvasCourse.CourseCode)
                        .ForContext(nameof(CourseEnrolmentEntry), canvasEnrolment, true)
                        .Warning("Failed to remove extra member from Canvas Course");
                }
            }

            if (_configuration.UseGroups)
            {
                foreach (var group in calculatedMembers.Value.GroupBy(entry => entry.SectionId))
                {
                    if (group.Key == CanvasSectionCode.Empty)
                        continue;

                    List<string> usersInGroup = await _gateway.GetGroupMembers(group.Key, cancellationToken);

                    List<CanvasCourseMembership> missingGroupMembers = group
                        .Where(entry => !usersInGroup.Contains(entry.UserId))
                        .ToList();

                    foreach (CanvasCourseMembership missingGroupMember in missingGroupMembers)
                    {
                        _logger.Information("Adding {user} to group {group} in course {course}", missingGroupMember.UserId, group.Key, canvasCourse.CourseCode);
                        
                        bool addToGroupAttempt = await _gateway.AddUserToGroup(missingGroupMember.UserId, group.Key, cancellationToken);

                        if (!addToGroupAttempt)
                            _logger
                                .ForContext(nameof(CanvasCourseCode), canvasCourse.CourseCode)
                                .ForContext(nameof(CanvasCourseMembership), missingGroupMember, true)
                                .ForContext("GroupCode", group.Key)
                                .Warning("Failed to add missing member to Course Group");
                    }

                    List<string> extraGroupMembers = usersInGroup
                        .Where(entry => group.All(user => user.UserId != entry))
                        .ToList();

                    foreach (string extraGroupMember in extraGroupMembers)
                    {
                        _logger.Information("Removing {user} from group {group} in course {course}", extraGroupMember, group.Key, canvasCourse.CourseCode);
                        
                        bool removeFromGroupAttempt = await _gateway.RemoveUserFromGroup(extraGroupMember, group.Key, cancellationToken);

                        if (!removeFromGroupAttempt)
                            _logger
                                .ForContext(nameof(CanvasCourseCode), canvasCourse.CourseCode)
                                .ForContext(nameof(extraGroupMember), extraGroupMember)
                                .ForContext("GroupCode", group.Key)
                                .Warning("Failed to remove extra member from Course Group");
                    }
                }
            }

            if (!_configuration.UseSections) continue;

            foreach (var calculatedMember in calculatedMembers.Value.GroupBy(entry => entry.UserId))
            {
                List<CourseEnrolmentEntry> matchingCanvasEnrolments = canvasEnrolments
                    .Where(entry => entry.UserId == calculatedMember.First().UserId)
                    .ToList();

                if (matchingCanvasEnrolments.Count == 0)
                    continue;

                foreach (CourseEnrolmentEntry matchingCanvasEnrolment in matchingCanvasEnrolments)
                {
                    if (calculatedMember.Any(entry => entry.SectionId == matchingCanvasEnrolment.SectionCode)) continue;

                    _logger.Information("Removing {user} from section {section} in course {course}", calculatedMember.First().UserId, matchingCanvasEnrolment.SectionCode, matchingCanvasEnrolment.CourseCode);
                    
                    bool removedFromSection = await _gateway.UnenrolUser(
                        matchingCanvasEnrolment.EnrollmentId,
                        matchingCanvasEnrolment.CourseCode,
                        cancellationToken);

                    if (!removedFromSection)
                    {
                        _logger
                            .ForContext(nameof(CanvasSectionCode), matchingCanvasEnrolment.SectionCode)
                            .ForContext(nameof(CourseEnrolmentEntry), matchingCanvasEnrolment, true)
                            .Warning("Failed to remove member from Course Section");
                    }
                }

                foreach (CanvasCourseMembership calculatedMemberRecord in calculatedMember)
                {
                    if (matchingCanvasEnrolments.Any(entry => entry.SectionCode == calculatedMemberRecord.SectionId)) continue;

                    _logger.Information("Adding {user} to section {section} in course {course}", calculatedMemberRecord.UserId, calculatedMemberRecord.SectionId, calculatedMemberRecord.CanvasCourseCode);

                    bool addedToSection = await _gateway.EnrolToSection(
                        calculatedMemberRecord.UserId,
                        calculatedMemberRecord.SectionId,
                        calculatedMemberRecord.PermissionLevel,
                        cancellationToken);

                    if (!addedToSection)
                    {
                        _logger
                            .ForContext(nameof(CanvasSectionCode), calculatedMemberRecord.SectionId)
                            .ForContext(nameof(CanvasCourseMembership), calculatedMember, true)
                            .Warning("Failed to add user to Course Section");
                    }
                }
            }
        }

        _logger.Information("Completed script run");
    }
}