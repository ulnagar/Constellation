namespace Constellation.Application.Offerings.GetOfferingsWithCanvasUserRecords;

using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;

public sealed record OfferingSummaryWithUsers(
    OfferingId Id,
    string Name,
    string CourseName,
    DateOnly StartDate,
    DateOnly EndDate,
    Grade Grade,
    bool IsActive,
    List<string> CanvasResourceIds,
    List<OfferingSummaryWithUsers.User> Users)
{
    public sealed record User(
        string Id,
        User.UserType Type,
        User.AccessType AccessLevel)
    {
        public enum UserType
        {
            Unknown,
            Student,
            Teacher
        }

        public enum AccessType
        {
            Unknown,
            Student,
            Teacher
        }
    }
}
