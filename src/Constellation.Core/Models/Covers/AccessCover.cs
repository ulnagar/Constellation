namespace Constellation.Core.Models.Covers;

using Constellation.Core.Models.Covers.Enums;
using Events;
using Offerings.Identifiers;
using System;

public sealed class AccessCover : Cover
{
    private AccessCover(

        OfferingId offeringId,
        DateOnly startDate,
        DateOnly endDate,
        CoverTeacherType teacherType,
        string teacherId,
        string note)
    {
        Id = new();
        OfferingId = offeringId;
        StartDate = startDate;
        EndDate = endDate;
        TeacherType = teacherType;
        TeacherId = teacherId;
        Note = note;
    }

    public string Note { get; private set; }

    public static AccessCover Create(
        OfferingId offeringId,
        DateOnly startDate,
        DateOnly endDate,
        CoverTeacherType teacherType,
        string teacherId,
        string note)
    {
        AccessCover cover = new(offeringId, startDate, endDate, teacherType, teacherId, note);

        cover.RaiseDomainEvent(new CoverCreatedDomainEvent(new(), cover.Id));

        return cover;
    }
}