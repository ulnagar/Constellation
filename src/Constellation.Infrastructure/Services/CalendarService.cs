using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Text;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC Operations
    public class CalendarService : ICalendarService, IScopedService
    {
        public CalendarService()
        {
        }

        public string CreateInvite(string uid, string attendeeName, string attendeeEmail, string summary, string location, string description, DateTime start, DateTime end, int repeats = 0)
        {
            var sb = new StringBuilder();

            // Start with basic ICS format
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:AuroraCollege//ACOS//1.5");
            sb.AppendLine("METHOD:REQUEST");

            // Add the specific event
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("DTSTAMP:" + DateTime.UtcNow.ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("LAST-MODIFIED:" + DateTime.UtcNow.ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("UID:" + uid + "@aurora.nsw.edu.au");

            sb.AppendLine("DTSTART:" + start.ToUniversalTime().ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("DTEND:" + end.ToUniversalTime().ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("SEQUENCE:0");
            sb.AppendLine("SUMMARY:" + summary + "");
            sb.AppendLine("LOCATION: Microsoft Teams");
            sb.AppendLine("DESCRIPTION:" + summary + "");
            sb.AppendLine("PRIORITY:3");

            sb.AppendLine("ORGANIZER;CN=Aurora College:mailto:auroracoll-h.school@det.nsw.edu.au");
            sb.AppendLine($"ATTENDEE;CN={attendeeName};RSVP=FALSE:mailto:{attendeeEmail}");
            //// https://stackoverflow.com/questions/45076896/send-email-as-calendar-invite-appointment-in-sendgrid-c-sharp

            if (repeats > 0)
            {
                sb.AppendLine("RRULE:FREQ=WEEKLY;INTERVAL=2;WKST=MO;COUNT=" + repeats + "");
            }

            // Add the alarm
            sb.AppendLine("BEGIN:VALARM");
            sb.AppendLine("TRIGGER:-PT5M");
            sb.AppendLine("ACTION:DISPLAY");
            sb.AppendLine("DESCRIPTION:" + summary + "");
            sb.AppendLine("END:VALARM");

            // End the event
            sb.AppendLine("END:VEVENT");

            // End the calendar item
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        public string CreateEvent(string uid, string summary, string location, string description, DateTime start, DateTime end, int repeats = 0)
        {
            var sb = new StringBuilder();

            // Start with basic ICS format
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:AuroraCollege//ACOS//1.5");
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("METHOD:PUBLISH");

            // Add the Sydney timezone info
            sb.AppendLine("BEGIN:VTIMEZONE");
            sb.AppendLine("TZID:Australia/Sydney");
            sb.AppendLine("BEGIN:STANDARD");
            sb.AppendLine("TZOFFSETFROM:+1100");
            sb.AppendLine("TZOFFSETTO:+1000");
            sb.AppendLine("TZNAME:AEST");
            sb.AppendLine("DTSTART:19700405T030000");
            sb.AppendLine("RRULE:FREQ=YEARLY;BYMONTH=4;BYDAY=1SU");
            sb.AppendLine("END:STANDARD");
            sb.AppendLine("BEGIN:DAYLIGHT");
            sb.AppendLine("TZOFFSETFROM:+1000");
            sb.AppendLine("TZOFFSETTO:+1100");
            sb.AppendLine("TZNAME:AEDT");
            sb.AppendLine("DTSTART:19701004T020000");
            sb.AppendLine("RRULE=YEARLY;BYMONTH=10;BYDAY=1SU");
            sb.AppendLine("END:DAYLIGHT");
            sb.AppendLine("END:VTIMEZONE");

            // Add the specific event
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("DTSTAMP:" + DateTime.Now.ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("ORGANIZER;CN=Aurora College:mailto:auroracoll-h.school@det.nsw.edu.au");
            sb.AppendLine("UID:" + uid + "@aurora.nsw.edu.au");
            sb.AppendLine("DTSTART;TZID=Australia/Sydney:" + start.ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("DTEND;TZID=Australia/Sydney:" + end.ToString("yyyyMMddTHHmm00"));
            //sb.AppendLine($"ATTENDEE;CN=\"{0}\";RSVP=TRUE:mailto:{1}", string.Join(",", toUsers), string.Join(",", toUsers)));
            // https://stackoverflow.com/questions/45076896/send-email-as-calendar-invite-appointment-in-sendgrid-c-sharp

            if (repeats > 0)
            {
                sb.AppendLine("RRULE:FREQ=WEEKLY;INTERVAL=2;WKST=MO;COUNT=" + repeats + "");
            }

            sb.AppendLine("SUMMARY:" + summary + "");
            sb.AppendLine("LOCATION:" + location + "");
            sb.AppendLine("DESCRIPTION:" + description + "");
            sb.AppendLine("PRIORITY:3");

            // Add the alarm
            sb.AppendLine("BEGIN:VALARM");
            sb.AppendLine("TRIGGER:-PT5M");
            sb.AppendLine("ACTION:DISPLAY");
            sb.AppendLine("DESCRIPTION:" + summary + "");
            sb.AppendLine("END:VALARM");

            // End the event
            sb.AppendLine("END:VEVENT");

            // End the calendar item
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }

        public string ModifyEvent(string uid, string summary, string location, string description, DateTime start, DateTime end, int repeats = 0)
        {
            throw new NotImplementedException();
        }

        public string CancelEvent(string uid, string summary, string location, string description, DateTime start, DateTime end, int repeats = 0)
        {
            throw new NotImplementedException();
        }

        public string StartCalendarFile()
        {
            var sb = new StringBuilder();

            // Start with basic ICS format
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:AuroraCollege//AOCS//1.5");
            sb.AppendLine("CALSCALE:GREGORIAN");

            // Add the Sydney timezone info
            sb.AppendLine("BEGIN:VTIMEZONE");
            sb.AppendLine("TZID:Australia/Sydney");
            sb.AppendLine("BEGIN:STANDARD");
            sb.AppendLine("TZOFFSETFROM:+1100");
            sb.AppendLine("TZOFFSETTO:+1000");
            sb.AppendLine("TZNAME:AEST");
            sb.AppendLine("DTSTART:19700405T030000");
            sb.AppendLine("RRULE:FREQ=YEARLY;BYMONTH=4;BYDAY=1SU");
            sb.AppendLine("END:STANDARD");
            sb.AppendLine("BEGIN:DAYLIGHT");
            sb.AppendLine("TZOFFSETFROM:+1000");
            sb.AppendLine("TZOFFSETTO:+1100");
            sb.AppendLine("TZNAME:AEDT");
            sb.AppendLine("DTSTART:19701004T020000");
            sb.AppendLine("RRULE=YEARLY;BYMONTH=10;BYDAY=1SU");
            sb.AppendLine("END:DAYLIGHT");
            sb.AppendLine("END:VTIMEZONE");

            return sb.ToString();
        }

        public string AddEventWithAlarm(string uid, string summary, string location, string description, DateTime start, DateTime end, int repeats)
        {
            var sb = new StringBuilder();
            sb.AppendLine("METHOD:PUBLISH");

            // Add the specific event
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("DTSTAMP:" + DateTime.Now.ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("ORGANIZER;CN=Aurora College:mailto:auroracoll-h.school@det.nsw.edu.au");
            sb.AppendLine("UID:" + uid + "@aurora.nsw.edu.au");
            sb.AppendLine("DTSTART;TZID=Australia/Sydney:" + start.ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("DTEND;TZID=Australia/Sydney:" + end.ToString("yyyyMMddTHHmm00"));

            if (repeats > 0)
            {
                sb.AppendLine("RRULE:FREQ=WEEKLY;INTERVAL=2;WKST=MO;COUNT=" + repeats + "");
            }

            sb.AppendLine("SUMMARY:" + summary + "");
            sb.AppendLine("LOCATION:" + location + "");
            sb.AppendLine("DESCRIPTION:" + description + "");
            sb.AppendLine("PRIORITY:3");

            // Add the alarm
            sb.AppendLine("BEGIN:VALARM");
            sb.AppendLine("TRIGGER:-PT5M");
            sb.AppendLine("ACTION:DISPLAY");
            sb.AppendLine("DESCRIPTION:" + summary + "");
            sb.AppendLine("END:VALARM");

            // End the event
            sb.AppendLine("END:VEVENT");

            return sb.ToString();
        }

        public string CancelEvent(string uid)
        {
            var sb = new StringBuilder();
            sb.AppendLine("METHOD:CANCEL");

            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("DTSTAMP:" + DateTime.Now.ToString("yyyyMMddTHHmm00"));
            sb.AppendLine("UID:" + uid + "@aurora.nsw.edu.au");
            sb.AppendLine("SEQUENCE:" + DateTime.Now.SecondsSinceYearStart());
            sb.AppendLine("STATUS:CANCELLED");
            sb.AppendLine("END:VEVENT");

            return sb.ToString();
        }

        public string EndCalendarFile()
        {
            var sb = new StringBuilder();

            // End the calendar item
            sb.AppendLine("END:VCALENDAR");

            return sb.ToString();
        }
    }
}
