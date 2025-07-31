namespace Constellation.Core.Models.Absences;

using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using System;

public class Notification
{
    private Notification(
        AbsenceId absenceId,
        NotificationType type,
        string message,
        string recipients)
    {
        Id = new();
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
        Notification notification = new(
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