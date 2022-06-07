using System;

namespace Constellation.Application.Interfaces.Services
{
    public interface ICalendarService
    {
        string CreateInvite(string uid, string attendeeName, string attendeeEmail, string summary, string location, string description, DateTime start, DateTime end, int repeats = 0);
        string CreateEvent(string uid, string summary, string location, string description, DateTime start, DateTime end, int repeats);
        string ModifyEvent(string uid, string summary, string location, string description, DateTime start, DateTime end, int repeats);
        string CancelEvent(string uid, string summary, string location, string description, DateTime start, DateTime end, int repeats);

        string StartCalendarFile();
        string AddEventWithAlarm(string uid, string summary, string location, string description, DateTime start, DateTime end, int repeats);
        string CancelEvent(string uid);
        string EndCalendarFile();
    }
}
