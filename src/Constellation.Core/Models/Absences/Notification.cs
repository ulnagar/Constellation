namespace Constellation.Core.Models.Absences;

using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using System;

public class Notification
{
    private Notification(
        AbsenceNotificationId id,
        AbsenceId absenceId,
        NotificationType type,
        string message,
        string recipients)
    {
        Id = id;
        AbsenceId = absenceId;
        Type = type;
        Message = message;
        Recipients = recipients;
    }

    public AbsenceNotificationId Id { get; private set; }
    public AbsenceId AbsenceId { get; private set; }
    public NotificationType Type { get; private set; }
    public string OutgoingId { get; private set; }
    public DateTime SentAt { get; private set; }
    public string Message { get; private set; }
    public string Recipients { get; private set; }
    public bool ConfirmedDelivered { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string DeliveredMessageIds { get; private set; }

    public static Notification Create(
        AbsenceId absenceId,
        NotificationType type,
        string message,
        string recipients)
    {
        var notification = new Notification(
            new AbsenceNotificationId(),
            absenceId,
            type,
            message,
            recipients);

        return notification;
    }

    public void UpdateSentFields(string outgoingId, DateTime sentAt)
    {
        SentAt = sentAt;
        OutgoingId = outgoingId;
    }

    public void UpdateDeliveryFields(DateTime deliveredAt, string deliveryId)
    {
        ConfirmedDelivered = true;
        DeliveredAt = deliveredAt;
        DeliveredMessageIds = deliveryId;
    }
}