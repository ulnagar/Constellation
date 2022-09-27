namespace Constellation.Application.Features.Contacts.Queries;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetStudentSupportContactsQuery : IRequest<ICollection<StudentSupportContact>>
{
    public string StudentId { get; set; }
}

public class StudentSupportContact
{
    public StudentSupportContact() { }

    public StudentSupportContact(Staff staffMember)
    {
        FirstName = staffMember.FirstName;
        LastName = staffMember.LastName;
        DisplayName = staffMember.DisplayName;
        EmailAddress = staffMember.EmailAddress;
    }

    public static StudentSupportContact GetDefault()
    {
        var entry = new StudentSupportContact
        {
            DisplayName = "Administration Office",
            EmailAddress = "auroracoll-h.school@det.nsw.edu.au",
            PhoneNumber = "1300 287 629",
            Category = "Aurora College"
        };

        return entry;
    }

    public static StudentSupportContact GetSupport()
    {
        var entry = new StudentSupportContact
        {
            DisplayName = "Technology Support Team",
            EmailAddress = "support@aurora.nsw.edu.au",
            PhoneNumber = "1300 610 733",
            Category = "Aurora College"
        };

        return entry;
    }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
    public string Category { get; set; }
    public string Detail { get; set; }
}

public class GetStudentSupportContactsQueryHandler : IRequestHandler<GetStudentSupportContactsQuery, ICollection<StudentSupportContact>>
{
    private readonly IAppDbContext _context;

    public GetStudentSupportContactsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<StudentSupportContact>> Handle(GetStudentSupportContactsQuery request, CancellationToken cancellationToken)
    {
        var data = new List<StudentSupportContact>();

        // Add Counsellor
        var counsellor = await _context.Staff
            .FirstOrDefaultAsync(staff => staff.StaffId == "1436948", cancellationToken);

        if (counsellor != null)
        {
            var entry = new StudentSupportContact(counsellor);
            entry.Category = "Support";
            entry.Detail = "School Counsellor";

            data.Add(entry);
        }

        // Add Careers Advisor
        var careersAdvisor = await _context.Staff
            .FirstOrDefaultAsync(staff => staff.StaffId == "9222390", cancellationToken);

        if (careersAdvisor != null)
        {
            var entry = new StudentSupportContact(careersAdvisor);
            entry.Category = "Support";
            entry.Detail = "Careers Advisor";

            data.Add(entry);
        }

        // Add Generic School Contact
        var genericContact = StudentSupportContact.GetDefault();
        data.Add(genericContact);

        var techContact = StudentSupportContact.GetSupport();
        data.Add(techContact);

        var student = await _context.Students
            .FirstOrDefaultAsync(student => student.StudentId == request.StudentId, cancellationToken);

        if (student == null)
            return data;

        var offerings = await _context.Enrolments
            .Where(enrolment => !enrolment.IsDeleted &&
                enrolment.StudentId == request.StudentId &&
                enrolment.Offering.EndDate.Date > DateTime.Today &&
                enrolment.Offering.StartDate.Date < DateTime.Today)
            .Select(enrolment => enrolment.Offering)
            .ToListAsync(cancellationToken);

        foreach (var offering in offerings)
        {
            var sessions = await _context.Sessions
                .Where(session => session.OfferingId == offering.Id &&
                    !session.IsDeleted &&
                    session.Period.Type != "Other")
                .Include(session => session.Teacher)
                .ToListAsync(cancellationToken);

            sessions = sessions.DistinctBy(session => session.StaffId).ToList();

            foreach (var session in sessions)
            {
                var entry = new StudentSupportContact(session.Teacher);
                entry.Category = "Teacher";
                entry.Detail = offering.Name;

                data.Add(entry);
            }
        }

        // Add Learning Support Team Contact
        if (student.CurrentGrade == Core.Enums.Grade.Y07 ||
            student.CurrentGrade == Core.Enums.Grade.Y09 ||
            student.CurrentGrade == Core.Enums.Grade.Y11 ||
            student.CurrentGrade == Core.Enums.Grade.Y12)
        {
            var teacher = await _context.Staff
                .FirstOrDefaultAsync(staff => staff.StaffId == "1064078", cancellationToken);

            if (teacher != null)
            {
                var entry = new StudentSupportContact(teacher);
                entry.Category = "Support";
                entry.Detail = "Learning Support";

                data.Add(entry);
            }
        }

        if (student.CurrentGrade == Core.Enums.Grade.Y05 ||
            student.CurrentGrade == Core.Enums.Grade.Y06 ||
            student.CurrentGrade == Core.Enums.Grade.Y08 ||
            student.CurrentGrade == Core.Enums.Grade.Y10)
        {
            var teacher = await _context.Staff
                .FirstOrDefaultAsync(staff => staff.StaffId == "1094923", cancellationToken);

            if (teacher != null)
            {
                var entry = new StudentSupportContact(teacher);
                entry.Category = "Support";
                entry.Detail = "Learning Support";

                data.Add(entry);
            }
        }

        return data;
    }
}
