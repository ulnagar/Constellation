namespace Constellation.Application.MissedWork.GetOutstandingNotificationsForTeacher;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.MissedWork.Models;
using System.Collections.Generic;

public sealed record GetOutstandingNotificationsForTeacherQuery(
    string StaffId)
    : IQuery<List<NotificationSummary>>;