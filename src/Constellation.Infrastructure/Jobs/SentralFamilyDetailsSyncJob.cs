namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Extensions;
using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Students.SendFamilyContactChangesReport;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class SentralFamilyDetailsSyncJob : ISentralFamilyDetailsSyncJob, IScopedService, IHangfireJob
{
    private readonly IAppDbContext _context;
    private readonly ILogger _logger;
    private readonly ISentralGateway _gateway;
    private readonly IMediator _mediator;

    public SentralFamilyDetailsSyncJob(IAppDbContext context, ILogger logger, 
        ISentralGateway gateway, IMediator mediator)
    {
        _context = context;
        _logger = logger.ForContext<ISentralFamilyDetailsSyncJob>();
        _gateway = gateway;
        _mediator = mediator;
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        _logger.Information("{id}: Starting Sentral Family Details Scan.", jobId);

        var changeLog = new List<ParentContactChangeDto>();

        // Get the CSV file from Sentral
        // Convert to temporary objects
        var families = await _gateway.GetFamilyDetailsReport(_logger);

        _logger.Information("{id}: Found {count} families", jobId, families.Count);

        // Check objects against database
        foreach (var family in families)
        {
            if (token.IsCancellationRequested)
                return;

            _logger.Information("{id}: Checking family: {name} ({code})", jobId, family.AddressName, family.FamilyId);

            family.MotherMobile = family.MotherMobile.Replace(" ", "");
            family.FatherMobile = family.FatherMobile.Replace(" ", "");

            var replacedEmails = new List<string>();

            // Check family exists in database
            var entry = await _context.StudentFamilies
                .Include(family => family.Students)
                .SingleOrDefaultAsync(f => f.Id == family.FamilyId, token);

            if (entry == null)
            {
                _logger.Information("{id}: No existing entry for {name} ({code}). Creating new family.", jobId, family.AddressName, family.FamilyId);
                // New Family... Add to database
                entry = new StudentFamily
                {
                    Id = family.FamilyId,
                    Parent1 = new StudentFamily.Parent
                    {
                        Title = family.FatherTitle,
                        FirstName = family.FatherFirstName,
                        LastName = family.FatherLastName,
                        MobileNumber = family.FatherMobile,
                        EmailAddress = family.FatherEmail
                    },
                    Parent2 = new StudentFamily.Parent
                    {
                        Title = family.MotherTitle,
                        FirstName = family.MotherFirstName,
                        LastName = family.MotherLastName,
                        MobileNumber = family.MotherMobile,
                        EmailAddress = family.MotherEmail
                    },
                    Address = new StudentFamily.MailingAddress
                    {
                        Title = family.AddressName,
                        Line1 = family.AddressLine1,
                        Line2 = family.AddressLine2,
                        Town = family.AddressTown,
                        PostCode = family.AddressPostCode
                    }
                };

                foreach (var studentId in family.StudentIds)
                {
                    var student = await _context.Students.FirstOrDefaultAsync(student => student.StudentId == studentId, token);
                    if (student != null)
                    {
                        _logger.Information("{id}: Adding student {name} to family {family} ({code})", jobId, student.DisplayName, family.AddressName, family.FamilyId);

                        entry.Students.Add(student);
                    }
                }

                // Check if email is blank and alert admin
                if (string.IsNullOrEmpty(family.FamilyEmail))
                {
                    changeLog.Add(new ParentContactChangeDto(
                        "Family Email",
                        string.Empty,
                        string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                        "No Email Supplied"));
                } 
                else
                {
                    entry.EmailAddress = family.FamilyEmail;

                    changeLog.Add(new ParentContactChangeDto(
                        "Family Email",
                        string.Empty,
                        family.FamilyEmail,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                        "Family Email Changed"));
                }

                changeLog.Add(new ParentContactChangeDto(
                    $"{entry.Parent1.FirstName.Trim()} {entry.Parent1.LastName.Trim()}",
                    string.Empty,
                    entry.Parent1.EmailAddress,
                    (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                    "New Parent Added"));

                changeLog.Add(new ParentContactChangeDto(
                    $"{entry.Parent2.FirstName.Trim()} {entry.Parent2.LastName.Trim()}",
                    string.Empty,
                    entry.Parent2.EmailAddress,
                    (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                    (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                    (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                    "New Parent Added"));

                _context.Add(entry);

            } else
            {
                _logger.Information("{id}: Found existing entry for {family} ({code}). Updating details.", jobId, family.AddressName, family.FamilyId);

                // Existing family... Check details
                if (entry.Parent1.Title != family.FatherTitle)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Parent 1 Title from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent1.Title, family.FatherTitle);
                    
                    entry.Parent1.Title = family.FatherTitle;
                }

                if (entry.Parent1.FirstName != family.FatherFirstName)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Parent 1 First Name from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent1.FirstName, family.FatherFirstName);

                    entry.Parent1.FirstName = family.FatherFirstName;
                }

                if (entry.Parent1.LastName != family.FatherLastName)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Parent 1 Last Name from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent1.LastName, family.FatherLastName);
                    
                    entry.Parent1.LastName = family.FatherLastName;
                }

                if (entry.Parent1.MobileNumber != family.FatherMobile)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Parent 1 Mobile from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent1.MobileNumber, family.FatherMobile);
                    
                    entry.Parent1.MobileNumber = family.FatherMobile;
                }

                if (entry.Parent1.EmailAddress != family.FatherEmail && !string.IsNullOrWhiteSpace(family.FatherEmail))
                {
                    _logger.Information("{id}: {family} ({code}): Updated Parent 1 Email from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent1.EmailAddress, family.FatherEmail);

                    replacedEmails.Add(entry.Parent1.EmailAddress);

                    changeLog.Add(new ParentContactChangeDto(
                        $"{entry.Parent1.FirstName.Trim()} {entry.Parent1.LastName.Trim()}",
                        entry.Parent1.EmailAddress,
                        family.FatherEmail,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                        "Parent Email Changed"));

                    entry.Parent1.EmailAddress = family.FatherEmail;
                }

                if (string.IsNullOrWhiteSpace(family.FatherEmail) && !string.IsNullOrWhiteSpace(family.FatherFirstName))
                {
                    if (string.IsNullOrWhiteSpace(entry.Parent1.EmailAddress))
                    {
                        changeLog.Add(new ParentContactChangeDto(
                        $"{entry.Parent1.FirstName.Trim()} {entry.Parent1.LastName.Trim()}",
                        string.Empty,
                        string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                        "Parent Email Missing"));
                    }
                    else
                    {
                        changeLog.Add(new ParentContactChangeDto(
                        $"{entry.Parent1.FirstName.Trim()} {entry.Parent1.LastName.Trim()}",
                        entry.Parent1.EmailAddress,
                        string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                        "Parent Email Removed"));
                    }
                }

                if (string.IsNullOrEmpty(entry.Parent1.FirstName))
                {
                    entry.Parent1.EmailAddress = null;
                }

                if (entry.Parent2.Title != family.MotherTitle)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Parent 2 Title from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent2.Title, family.MotherTitle);

                    entry.Parent2.Title = family.MotherTitle;
                }

                if (entry.Parent2.FirstName != family.MotherFirstName)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Parent 2 First Name from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent2.FirstName, family.MotherFirstName);

                    entry.Parent2.FirstName = family.MotherFirstName;
                }

                if (entry.Parent2.LastName != family.MotherLastName)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Parent 2 Last Name from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent2.LastName, family.MotherLastName);

                    entry.Parent2.LastName = family.MotherLastName;
                }

                if (entry.Parent2.MobileNumber != family.MotherMobile)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Parent 2 Mobile from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent2.MobileNumber, family.MotherMobile);

                    entry.Parent2.MobileNumber = family.MotherMobile;
                }

                if (entry.Parent2.EmailAddress != family.MotherEmail && !string.IsNullOrWhiteSpace(family.MotherEmail))
                {
                    _logger.Information("{id}: {family} ({code}): Updated Parent 2 Email from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent2.EmailAddress, family.MotherEmail);

                    replacedEmails.Add(entry.Parent2.EmailAddress);

                    changeLog.Add(new ParentContactChangeDto(
                        $"{entry.Parent2.FirstName.Trim()} {entry.Parent2.LastName.Trim()}",
                        entry.Parent2.EmailAddress,
                        family.MotherEmail,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                        "Parent Email Changed"));

                    entry.Parent2.EmailAddress = family.MotherEmail;
                }

                if (string.IsNullOrWhiteSpace(family.MotherEmail) && !string.IsNullOrWhiteSpace(family.MotherFirstName))
                {
                    if (string.IsNullOrWhiteSpace(entry.Parent2.EmailAddress))
                    {
                        changeLog.Add(new ParentContactChangeDto(
                        $"{entry.Parent2.FirstName.Trim()} {entry.Parent2.LastName.Trim()}",
                        string.Empty,
                        string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                        "Parent Email Missing"));
                    }
                    else
                    {
                        changeLog.Add(new ParentContactChangeDto(
                        $"{entry.Parent2.FirstName.Trim()} {entry.Parent2.LastName.Trim()}",
                        entry.Parent2.EmailAddress,
                        string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                        "Parent Email Removed"));
                    }
                }

                if (string.IsNullOrEmpty(entry.Parent2.FirstName))
                {
                    entry.Parent2.EmailAddress = null;
                }

                if (entry.Address.Title != family.AddressName)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Address Name from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Address.Title, family.AddressName);

                    entry.Address.Title = family.AddressName;
                }

                if (entry.Address.Line1 != family.AddressLine1)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Address Line 1 from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Address.Line1, family.AddressLine1);

                    entry.Address.Line1 = family.AddressLine1;
                }

                if (entry.Address.Line2 != family.AddressLine2)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Address Line 2 from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Address.Line2, family.AddressLine2);

                    entry.Address.Line2 = family.AddressLine2;
                }

                if (entry.Address.Town != family.AddressTown)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Address Town from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Address.Town, family.AddressTown);

                    entry.Address.Town = family.AddressTown;
                }

                if (entry.Address.PostCode != family.AddressPostCode)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Address PostCode from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Address.PostCode, family.AddressPostCode);

                    entry.Address.PostCode = family.AddressPostCode;
                }

                if (string.IsNullOrWhiteSpace(entry.EmailAddress) && string.IsNullOrWhiteSpace(family.FamilyEmail))
                {
                    changeLog.Add(new ParentContactChangeDto(
                        "Family Email",
                        string.Empty,
                        string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                        (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                        "Family Email Missing"));
                }

                if (entry.EmailAddress != family.FamilyEmail)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Family Email Address from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.EmailAddress, family.FamilyEmail);

                    if (string.IsNullOrWhiteSpace(entry.EmailAddress) && !string.IsNullOrWhiteSpace(family.FamilyEmail))
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            string.Empty,
                            family.FamilyEmail,
                            (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                            (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                            (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                            "Family Email Added"));
                    }

                    if (!string.IsNullOrWhiteSpace(entry.EmailAddress) && !string.IsNullOrWhiteSpace(family.FamilyEmail))
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            entry.EmailAddress,
                            family.FamilyEmail,
                            (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                            (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                            (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                            "Family Email Changed"));
                    }

                    if (!string.IsNullOrWhiteSpace(entry.EmailAddress) && string.IsNullOrWhiteSpace(family.FamilyEmail))
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            entry.EmailAddress,
                            string.Empty,
                            (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().FirstName : string.Empty,
                            (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().LastName : string.Empty,
                            (entry.Students.FirstOrDefault() is not null) ? entry.Students.First().CurrentGrade.AsName() : string.Empty,
                            "Family Email Removed"));
                    }

                    entry.EmailAddress = family.FamilyEmail;
                }

                foreach (var studentId in family.StudentIds)
                {
                    if (entry.Students.All(student => student.StudentId != studentId))
                    {
                        // Student is not currently linked
                        var student = await _context
                            .Students
                            .FirstOrDefaultAsync(student => 
                                student.StudentId == studentId,
                                token);

                        if (student != null)
                        {
                            _logger.Information("{id}: Adding student {name} to family {family} ({code})", jobId, student.DisplayName, family.AddressName, family.FamilyId);

                            entry.Students.Add(student);
                        }
                    }
                }

                var linkedStudents = entry.Students.ToList();
                foreach (var student in linkedStudents)
                {
                    if (!family.StudentIds.Contains(student.StudentId))
                    {
                        _logger.Information("{id}: Removing student {name} to family {family} ({code})", jobId, student.DisplayName, family.AddressName, family.FamilyId);

                        // Student should not be linked
                        entry.Students.Remove(student);
                    }
                }
            }

            await _context.SaveChangesAsync(token);

            // Create app users for each parents
            if (!string.IsNullOrWhiteSpace(entry.Parent1.EmailAddress) && !string.IsNullOrWhiteSpace(entry.Parent1.FirstName))
            {
                await _mediator.Send(
                    new RegisterParentContactAsUserCommand
                    {
                        FirstName = entry.Parent1.FirstName,
                        LastName = entry.Parent1.LastName,
                        EmailAddress = entry.Parent1.EmailAddress
                    },
                    token);
            }

            if (!string.IsNullOrWhiteSpace(entry.Parent2.EmailAddress) && !string.IsNullOrWhiteSpace(entry.Parent2.FirstName))
            {
                await _mediator.Send(
                    new RegisterParentContactAsUserCommand
                    {
                        FirstName = entry.Parent2.FirstName,
                        LastName = entry.Parent2.LastName,
                        EmailAddress = entry.Parent2.EmailAddress
                    },
                    token);
            }

            // Remove app users for old and replaced email addresses
            foreach (var email in replacedEmails)
            {
                if (email != entry.Parent1.EmailAddress && email != entry.Parent2.EmailAddress)
                    await _mediator.Send(
                        new RemoveOldParentEmailAddressFromUserCommand { 
                            Email = email 
                        },
                        token);
            }
        }

        if (changeLog.Count > 0)
            await _mediator.Send(new SendFamilyContactChangesReportCommand(changeLog), token);
    }
}
