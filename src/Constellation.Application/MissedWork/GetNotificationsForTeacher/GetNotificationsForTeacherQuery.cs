namespace Constellation.Application.MissedWork.GetNotificationsForTeacher;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.MissedWork.Models;
using System.Collections.Generic;

public sealed record GetNotificationsForTeacherQuery(
    string StaffId)
    : IQuery<List<NotificationSummary>>;
