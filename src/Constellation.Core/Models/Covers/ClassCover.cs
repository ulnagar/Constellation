namespace Constellation.Core.Models.Covers;

using Constellation.Core.Models.Offerings.Identifiers;
using Enums;
using Events;
using System;

public sealed class ClassCover : Cover
{
    private ClassCover(
        OfferingId offeringId,
        DateOnly startDate,
        DateOnly endDate,
        CoverTeacherType teacherType,
        string teacherId)
    {
        Id = new();
        OfferingId = offeringId;
        StartDate = startDate;
        EndDate = endDate;
        TeacherType = teacherType;
        TeacherId = teacherId;
    }

    public static ClassCover Create(
        OfferingId offeringId,
        DateOnly startDate,
        DateOnly endDate,
        CoverTeacherType teacherType,
        string teacherId)
    {
        ClassCover cover = new(offeringId, startDate, endDate, teacherType, teacherId);

        cover.RaiseDomainEvent(new CoverCreatedDomainEvent(new(), cover.Id));

        return cover;
    }
}