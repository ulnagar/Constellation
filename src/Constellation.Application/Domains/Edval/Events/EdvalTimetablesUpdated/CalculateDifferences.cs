﻿namespace Constellation.Application.Domains.Edval.Events.EdvalTimetablesUpdated;

using Abstractions.Messaging;
using Constellation.Core.Models.Edval.Enums;
using Core.Models;
using Core.Models.Edval;
using Core.Models.Edval.Events;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.Repositories;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CalculateDifferences : IIntegrationEventHandler<EdvalTimetablesUpdatedIntegrationEvent>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CalculateDifferences(
        IEdvalRepository edvalRepository,
        IOfferingRepository offeringRepository,
        IPeriodRepository periodRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<EdvalTimetablesUpdatedIntegrationEvent>();
    }

    public async Task Handle(EdvalTimetablesUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        List<Period> periods = await _periodRepository.GetCurrent(cancellationToken);
        List<Offering> offerings = await _offeringRepository.GetAllActive(cancellationToken);
        List<StaffMember> staff = await _staffRepository.GetAllActive(cancellationToken);

        List<EdvalTeacher> teachers = await _edvalRepository.GetTeachers(cancellationToken);
        List<EdvalTimetable> timetables = await _edvalRepository.GetTimetables(cancellationToken);
        List<EdvalIgnore> ignoredTimetables = await _edvalRepository.GetIgnoreRecords(EdvalDifferenceType.EdvalTimetable, cancellationToken);

        foreach (EdvalTimetable timetable in timetables)
        {
            bool ignored = ignoredTimetables
                .Where(ignore => ignore.System == EdvalDifferenceSystem.EdvalDifference)
                .Any(ignore => ignore.Identifier == timetable.Identifier);

            EdvalTeacher teacher = teachers.FirstOrDefault(teacher => teacher.TeacherId == timetable.TeacherId);

            if (teacher is null)
            {
                _logger
                    .ForContext(nameof(EdvalTimetable), timetable, true)
                    .Warning("Failed to match Teacher with Timetable in Edval data");

                continue;
            }

            StaffMember staffMember = staff.FirstOrDefault(member =>
                member.Name.FirstName.Equals(teacher.FirstName, StringComparison.OrdinalIgnoreCase) &&
                member.Name.LastName.Equals(teacher.LastName, StringComparison.OrdinalIgnoreCase));

            if (staffMember is null)
            {
                _logger
                    .ForContext(nameof(EdvalTimetable), timetable, true)
                    .Warning("Unable to find matching staff member by Name");

                continue;
            }

            Offering offering = offerings.FirstOrDefault(offering => offering.Name.Value == timetable.OfferingName);

            if (offering is null)
            {
                _logger
                    .ForContext(nameof(EdvalTimetable), timetable, true)
                    .Warning("Unable to find matching offering by Name");

                continue;
            }

            if (!offering.Teachers.Any(assignment =>
                    assignment.StaffId == staffMember.Id && 
                    assignment.Type == AssignmentType.ClassroomTeacher &&
                    !assignment.IsDeleted))
            {
                _edvalRepository.Insert(new Difference(
                    EdvalDifferenceType.EdvalTimetable,
                    EdvalDifferenceSystem.EdvalDifference,
                    timetable.Identifier,
                    $"{staffMember.Name} is not a Classroom Teacher for {offering.Name} in Constellation",
                    ignored));
            }

            Period period = periods.SingleOrDefault(period =>
                period.DayNumber == timetable.DayNumber &&
                period.SentralPeriodName() == timetable.Period);

            if (period is null)
            {
                _logger
                    .ForContext(nameof(EdvalTimetable), timetable, true)
                    .Warning("Unable to find matching period by DayNumber and SentralPeriodName");

                continue;
            }

            if (!offering.Sessions.Any(session =>
                    session.PeriodId == period.Id &&
                    !session.IsDeleted))
            {
                _edvalRepository.Insert(new Difference(
                    EdvalDifferenceType.EdvalTimetable,
                    EdvalDifferenceSystem.EdvalDifference,
                    timetable.Identifier,
                    $"{offering.Name} does not have a session at {period} in Constellation",
                    ignored));
            }
        }

        foreach (Offering offering in offerings)
        {
            List<StaffId> classTeacherIds = offering.Teachers
                .Where(assignment =>
                    assignment.Type == AssignmentType.ClassroomTeacher &&
                    !assignment.IsDeleted)
                .Select(assignment => assignment.StaffId)
                .Distinct()
                .ToList();

            List<StaffMember> classTeachers = staff
                .Where(staffMember => classTeacherIds.Contains(staffMember.Id))
                .ToList();

            foreach (StaffMember classTeacher in classTeachers)
            {
                bool ignored = ignoredTimetables
                    .Where(ignore => ignore.System == EdvalDifferenceSystem.ConstellationDifference)
                    .Any(ignore => ignore.Identifier == $"{offering.Id}--{classTeacher.Id}");

                EdvalTeacher edvalTeacher = teachers.FirstOrDefault(teacher =>
                    teacher.FirstName.Equals(classTeacher.Name.FirstName, StringComparison.OrdinalIgnoreCase) &&
                    teacher.LastName.Equals(classTeacher.Name.LastName, StringComparison.OrdinalIgnoreCase));

                if (edvalTeacher is null)
                {
                    _logger
                        .ForContext(nameof(StaffMember), classTeacher, true)
                        .Warning("Unable to find matching Edval Teacher by Name");

                    continue;
                }

                if (!timetables.Any(timetable =>
                        timetable.OfferingName == offering.Name.Value &&
                        timetable.TeacherId == edvalTeacher.TeacherId))
                {
                    _edvalRepository.Insert(new Difference(
                        EdvalDifferenceType.EdvalTimetable,
                        EdvalDifferenceSystem.ConstellationDifference,
                        $"{offering.Id}--{classTeacher.Id}",
                        $"{classTeacher.Name} is not a Classroom Teacher for {offering.Name} in Edval",
                        ignored));
                }
            }

            List<PeriodId> classPeriodIds = offering.Sessions
                .Where(session => !session.IsDeleted)
                .Select(session => session.PeriodId)
                .ToList();

            List<Period> classPeriods = periods
                .Where(period => classPeriodIds.Contains(period.Id))
                .ToList();

            foreach (Period period in classPeriods)
            {
                bool ignored = ignoredTimetables
                    .Where(ignore => ignore.System == EdvalDifferenceSystem.ConstellationDifference)
                    .Any(ignore => ignore.Identifier == $"{offering.Id}--{period.Id}");

                if (!timetables.Any(timetable =>
                        timetable.DayNumber == period.DayNumber &&
                        timetable.Period == period.SentralPeriodName() &&
                        timetable.OfferingName == offering.Name.Value))
                {
                    _edvalRepository.Insert(new Difference(
                        EdvalDifferenceType.EdvalTimetable,
                        EdvalDifferenceSystem.ConstellationDifference,
                        $"{offering.Id}--{period.Id}",
                        $"{offering.Name} does not have a session at {period} in Edval",
                        ignored));
                }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}