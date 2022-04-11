using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class SentralFamilyDetailsSyncJob : ISentralFamilyDetailsSyncJob, IScopedService
    {
        private readonly IAppDbContext _context;
        private readonly ILogger<ISentralFamilyDetailsSyncJob> _logger;
        private readonly ISentralGateway _gateway;

        public SentralFamilyDetailsSyncJob(IAppDbContext context, ILogger<ISentralFamilyDetailsSyncJob> logger, ISentralGateway gateway)
        {
            _context = context;
            _logger = logger;
            _gateway = gateway;
        }

        public async Task StartJob(bool automated)
        {
            if (automated)
            {
                var jobStatus = await _context.JobActivations.FirstOrDefaultAsync(job => job.JobName == nameof(ISentralFamilyDetailsSyncJob));
                if (jobStatus == null || !jobStatus.IsActive)
                {
                    _logger.LogInformation("Stopped due to job being set inactive.");
                    return;
                }
            }

            _logger.LogInformation("Starting Sentral Family Details Scan.");

            // Get the CSV file from Sentral
            // Convert to temporary objects
            var families = await _gateway.GetFamilyDetailsReport();

            _logger.LogInformation("Found {count} families", families.Count);

            // Check objects against database
            foreach (var family in families)
            {
                _logger.LogInformation(" Checking family: {name} ({id})", family.AddressName, family.FamilyId);

                family.MotherMobile = family.MotherMobile.Replace(" ", "");
                family.FatherMobile = family.FatherMobile.Replace(" ", "");

                // Check family exists in database
                var entry = await _context.StudentFamilies
                    .Include(family => family.Students)
                    .SingleOrDefaultAsync(f => f.Id == family.FamilyId);

                if (entry == null)
                {
                    _logger.LogInformation(" No existing entry. Creating new family.");
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
                            EmailAddress = family.FamilyEmail
                        },
                        Parent2 = new StudentFamily.Parent
                        {
                            Title = family.MotherTitle,
                            FirstName = family.MotherFirstName,
                            LastName = family.MotherLastName,
                            MobileNumber = family.MotherMobile,
                            EmailAddress = family.FamilyEmail
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
                        var student = await _context.Students.FirstOrDefaultAsync(student => student.StudentId == studentId);
                        if (student != null)
                        {
                            _logger.LogInformation("Adding student {name} to family {family} ({id})", student.DisplayName, family.AddressName, family.FamilyId);

                            entry.Students.Add(student);
                        }
                    }

                    _context.Add(entry);

                } else
                {
                    _logger.LogInformation(" Found existing entry. Updating details.");

                    // Existing family... Check details
                    if (entry.Parent1.Title != family.FatherTitle)
                    {
                        _logger.LogInformation(" Updated Parent 1 Title from {old} to {new}", entry.Parent1.Title, family.FatherTitle);
                        
                        entry.Parent1.Title = family.FatherTitle;
                    }

                    if (entry.Parent1.FirstName != family.FatherFirstName)
                    {
                        _logger.LogInformation(" Updated Parent 1 First Name from {old} to {new}", entry.Parent1.FirstName, family.FatherFirstName);

                        entry.Parent1.FirstName = family.FatherFirstName;
                    }

                    if (entry.Parent1.LastName != family.FatherLastName)
                    {
                        _logger.LogInformation(" Updated Parent 1 Last Name from {old} to {new}", entry.Parent1.LastName, family.FatherLastName);
                        
                        entry.Parent1.LastName = family.FatherLastName;
                    }

                    if (entry.Parent1.MobileNumber != family.FatherMobile)
                    {
                        _logger.LogInformation(" Updated Parent 1 Mobile from {old} to {new}", entry.Parent1.MobileNumber, family.FatherMobile);
                        
                        entry.Parent1.MobileNumber = family.FatherMobile;
                    }

                    if (entry.Parent1.EmailAddress != family.FamilyEmail)
                    {
                        if (!string.IsNullOrWhiteSpace(family.FamilyEmail))
                        {
                            _logger.LogInformation(" Updated Parent 1 Email from {old} to {new}", entry.Parent1.EmailAddress, family.FamilyEmail);

                            entry.Parent1.EmailAddress = family.FamilyEmail;
                        }
                    }

                    if (entry.Parent2.Title != family.MotherTitle)
                    {
                        _logger.LogInformation(" Updated Parent 2 Title from {old} to {new}", entry.Parent2.Title, family.MotherTitle);

                        entry.Parent2.Title = family.MotherTitle;
                    }

                    if (entry.Parent2.FirstName != family.MotherFirstName)
                    {
                        _logger.LogInformation(" Updated Parent 2 First Name from {old} to {new}", entry.Parent2.FirstName, family.MotherFirstName);

                        entry.Parent2.FirstName = family.MotherFirstName;
                    }

                    if (entry.Parent2.LastName != family.MotherLastName)
                    {
                        _logger.LogInformation(" Updated Parent 2 Last Name from {old} to {new}", entry.Parent2.LastName, family.MotherLastName);

                        entry.Parent2.LastName = family.MotherLastName;
                    }

                    if (entry.Parent2.MobileNumber != family.MotherMobile)
                    {
                        _logger.LogInformation(" Updated Parent 2 Mobile from {old} to {new}", entry.Parent2.MobileNumber, family.MotherMobile);

                        entry.Parent2.MobileNumber = family.MotherMobile;
                    }

                    if (entry.Parent2.EmailAddress != family.FamilyEmail)
                    {
                        if (!string.IsNullOrWhiteSpace(family.FamilyEmail))
                        {
                            _logger.LogInformation(" Updated Parent 2 Email from {old} to {new}", entry.Parent2.EmailAddress, family.FamilyEmail);

                            entry.Parent2.EmailAddress = family.FamilyEmail;
                        }
                    }

                    if (entry.Address.Title != family.AddressName)
                    {
                        _logger.LogInformation(" Updated Address Name from {old} to {new}", entry.Address.Title, family.AddressName);

                        entry.Address.Title = family.AddressName;
                    }

                    if (entry.Address.Line1 != family.AddressLine1)
                    {
                        _logger.LogInformation(" Updated Address Line 1 from {old} to {new}", entry.Address.Line1, family.AddressLine1);

                        entry.Address.Line1 = family.AddressLine1;
                    }

                    if (entry.Address.Line2 != family.AddressLine2)
                    {
                        _logger.LogInformation(" Updated Address Line 2 from {old} to {new}", entry.Address.Line2, family.AddressLine2);

                        entry.Address.Line2 = family.AddressLine2;
                    }

                    if (entry.Address.Town != family.AddressTown)
                    {
                        _logger.LogInformation(" Updated Address Town from {old} to {new}", entry.Address.Town, family.AddressTown);

                        entry.Address.Town = family.AddressTown;
                    }

                    if (entry.Address.PostCode != family.AddressPostCode)
                    {
                        _logger.LogInformation(" Updated Address PostCode from {old} to {new}", entry.Address.PostCode, family.AddressPostCode);

                        entry.Address.PostCode = family.AddressPostCode;
                    }

                    foreach (var studentId in family.StudentIds)
                    {
                        if (entry.Students.All(student => student.StudentId != studentId))
                        {
                            // Student is not currently linked
                            var student = await _context.Students.FirstOrDefaultAsync(student => student.StudentId == studentId);
                            if (student != null)
                            {
                                _logger.LogInformation("Adding student {name} to family {family} ({id})", student.DisplayName, family.AddressName, family.FamilyId);

                                entry.Students.Add(student);
                            }
                        }
                    }

                    var linkedStudents = entry.Students.ToList();
                    foreach (var student in linkedStudents)
                    {
                        if (!family.StudentIds.Contains(student.StudentId))
                        {
                            _logger.LogInformation("Removing student {name} to family {family} ({id})", student.DisplayName, family.AddressName, family.FamilyId);

                            // Student should not be linked
                            entry.Students.Remove(student);
                        }
                    }
                }

                await _context.SaveChangesAsync(new System.Threading.CancellationToken());
            }
        }
    }
}
