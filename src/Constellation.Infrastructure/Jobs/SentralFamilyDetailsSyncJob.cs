namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Features.Jobs.SentralFamilyDetailsSync.Notifications;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class SentralFamilyDetailsSyncJob : ISentralFamilyDetailsSyncJob, IScopedService, IHangfireJob
{
    private readonly IAppDbContext _context;
    private readonly ILogger<ISentralFamilyDetailsSyncJob> _logger;
    private readonly ISentralGateway _gateway;
    private readonly IMediator _mediator;

    public SentralFamilyDetailsSyncJob(IAppDbContext context, ILogger<ISentralFamilyDetailsSyncJob> logger, 
        ISentralGateway gateway, IMediator mediator)
    {
        _context = context;
        _logger = logger;
        _gateway = gateway;
        _mediator = mediator;
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        _logger.LogInformation("{id}: Starting Sentral Family Details Scan.", jobId);

        // Get the CSV file from Sentral
        // Convert to temporary objects
        var families = await _gateway.GetFamilyDetailsReport(_logger);

        _logger.LogInformation("{id}: Found {count} families", jobId, families.Count);

        // Check objects against database
        foreach (var family in families)
        {
            if (token.IsCancellationRequested)
                return;

            _logger.LogInformation("{id}: Checking family: {name} ({code})", jobId, family.AddressName, family.FamilyId);

            family.MotherMobile = family.MotherMobile.Replace(" ", "");
            family.FatherMobile = family.FatherMobile.Replace(" ", "");

            // Check family exists in database
            var entry = await _context.StudentFamilies
                .Include(family => family.Students)
                .SingleOrDefaultAsync(f => f.Id == family.FamilyId, token);

            if (entry == null)
            {
                _logger.LogInformation("{id}: No existing entry for {name} ({code}). Creating new family.", jobId, family.AddressName, family.FamilyId);
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

                // Check if email is blank and alert admin
                if (string.IsNullOrEmpty(family.FamilyEmail))
                {
                    await _mediator.Publish(new FamilyWithoutValidEmailAddressFoundNotification { FamilyId = family.FamilyId });
                }

                foreach (var studentId in family.StudentIds)
                {
                    var student = await _context.Students.FirstOrDefaultAsync(student => student.StudentId == studentId, token);
                    if (student != null)
                    {
                        _logger.LogInformation("{id}: Adding student {name} to family {family} ({code})", jobId, student.DisplayName, family.AddressName, family.FamilyId);

                        entry.Students.Add(student);
                    }
                }

                _context.Add(entry);

            } else
            {
                _logger.LogInformation("{id}: Found existing entry for {family} ({code}). Updating details.", jobId, family.AddressName, family.FamilyId);

                // Existing family... Check details
                if (entry.Parent1.Title != family.FatherTitle)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Parent 1 Title from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent1.Title, family.FatherTitle);
                    
                    entry.Parent1.Title = family.FatherTitle;
                }

                if (entry.Parent1.FirstName != family.FatherFirstName)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Parent 1 First Name from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent1.FirstName, family.FatherFirstName);

                    entry.Parent1.FirstName = family.FatherFirstName;
                }

                if (entry.Parent1.LastName != family.FatherLastName)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Parent 1 Last Name from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent1.LastName, family.FatherLastName);
                    
                    entry.Parent1.LastName = family.FatherLastName;
                }

                if (entry.Parent1.MobileNumber != family.FatherMobile)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Parent 1 Mobile from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent1.MobileNumber, family.FatherMobile);
                    
                    entry.Parent1.MobileNumber = family.FatherMobile;
                }

                if (entry.Parent1.EmailAddress != family.FatherEmail && !string.IsNullOrWhiteSpace(family.FatherEmail))
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Parent 1 Email from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent1.EmailAddress, family.FatherEmail);

                    entry.Parent1.EmailAddress = family.FatherEmail;
                }

                if (string.IsNullOrEmpty(entry.Parent1.FirstName))
                {
                    entry.Parent1.EmailAddress = null;
                }

                if (entry.Parent2.Title != family.MotherTitle)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Parent 2 Title from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent2.Title, family.MotherTitle);

                    entry.Parent2.Title = family.MotherTitle;
                }

                if (entry.Parent2.FirstName != family.MotherFirstName)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Parent 2 First Name from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent2.FirstName, family.MotherFirstName);

                    entry.Parent2.FirstName = family.MotherFirstName;
                }

                if (entry.Parent2.LastName != family.MotherLastName)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Parent 2 Last Name from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent2.LastName, family.MotherLastName);

                    entry.Parent2.LastName = family.MotherLastName;
                }

                if (entry.Parent2.MobileNumber != family.MotherMobile)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Parent 2 Mobile from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent2.MobileNumber, family.MotherMobile);

                    entry.Parent2.MobileNumber = family.MotherMobile;
                }

                if (entry.Parent2.EmailAddress != family.MotherEmail && !string.IsNullOrWhiteSpace(family.MotherEmail))
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Parent 2 Email from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Parent2.EmailAddress, family.MotherEmail);

                    entry.Parent2.EmailAddress = family.MotherEmail;
                }

                if (string.IsNullOrEmpty(entry.Parent1.FirstName))
                {
                    entry.Parent1.EmailAddress = null;
                }

                if (entry.Address.Title != family.AddressName)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Address Name from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Address.Title, family.AddressName);

                    entry.Address.Title = family.AddressName;
                }

                if (entry.Address.Line1 != family.AddressLine1)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Address Line 1 from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Address.Line1, family.AddressLine1);

                    entry.Address.Line1 = family.AddressLine1;
                }

                if (entry.Address.Line2 != family.AddressLine2)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Address Line 2 from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Address.Line2, family.AddressLine2);

                    entry.Address.Line2 = family.AddressLine2;
                }

                if (entry.Address.Town != family.AddressTown)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Address Town from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Address.Town, family.AddressTown);

                    entry.Address.Town = family.AddressTown;
                }

                if (entry.Address.PostCode != family.AddressPostCode)
                {
                    _logger.LogInformation("{id}: {family} ({code}): Updated Address PostCode from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.Address.PostCode, family.AddressPostCode);

                    entry.Address.PostCode = family.AddressPostCode;
                }

                // Check if email is blank and alert admin
                if (string.IsNullOrEmpty(family.FatherEmail) && string.IsNullOrEmpty(family.MotherEmail))
                {
                    await _mediator.Publish(new FamilyWithoutValidEmailAddressFoundNotification { FamilyId = family.FamilyId });
                }

                foreach (var studentId in family.StudentIds)
                {
                    if (entry.Students.All(student => student.StudentId != studentId))
                    {
                        // Student is not currently linked
                        var student = await _context.Students.FirstOrDefaultAsync(student => student.StudentId == studentId, token);
                        if (student != null)
                        {
                            _logger.LogInformation("{id}: Adding student {name} to family {family} ({code})", jobId, student.DisplayName, family.AddressName, family.FamilyId);

                            entry.Students.Add(student);
                        }
                    }
                }

                var linkedStudents = entry.Students.ToList();
                foreach (var student in linkedStudents)
                {
                    if (!family.StudentIds.Contains(student.StudentId))
                    {
                        _logger.LogInformation("{id}: Removing student {name} to family {family} ({code})", jobId, student.DisplayName, family.AddressName, family.FamilyId);

                        // Student should not be linked
                        entry.Students.Remove(student);
                    }
                }
            }

            await _context.SaveChangesAsync(token);


            // Create app users for each parents
            if (!string.IsNullOrWhiteSpace(entry.Parent1.EmailAddress) && !string.IsNullOrWhiteSpace(entry.Parent1.FirstName))
            {
                await _mediator.Send(new RegisterParentContactAsUserCommand { FirstName = entry.Parent1.FirstName, LastName = entry.Parent1.LastName, EmailAddress = entry.Parent1.EmailAddress });
            }

            if (!string.IsNullOrWhiteSpace(entry.Parent2.EmailAddress) && !string.IsNullOrWhiteSpace(entry.Parent2.FirstName))
            {
                await _mediator.Send(new RegisterParentContactAsUserCommand { FirstName = entry.Parent2.FirstName, LastName = entry.Parent2.LastName, EmailAddress = entry.Parent2.EmailAddress });
            }
        }
    }
}
