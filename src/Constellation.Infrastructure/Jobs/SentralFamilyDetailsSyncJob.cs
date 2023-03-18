namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Students.SendFamilyContactChangesReport;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.Families;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class SentralFamilyDetailsSyncJob : ISentralFamilyDetailsSyncJob, IHangfireJob
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISentralGateway _gateway;
    private readonly IMediator _mediator;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;

    public SentralFamilyDetailsSyncJob(
        IUnitOfWork unitOfWork,
        ILogger logger,
        ISentralGateway gateway,
        IMediator mediator,
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository)
    {
        _logger = logger.ForContext<ISentralFamilyDetailsSyncJob>();
        _unitOfWork = unitOfWork;
        _gateway = gateway;
        _mediator = mediator;
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
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

            // Check family exists in database
            var entry = await _familyRepository.GetFamilyBySentralId(family.FamilyId, token);

            if (entry == null)
            {
                _logger.Information("{id}: No existing entry for {name} ({code}). Creating new family.", jobId, family.AddressName, family.FamilyId);
                // New Family... Add to database
                entry = Family.Create(Guid.NewGuid(), family.AddressName);
                entry.LinkFamilyToSentralDetails(family.FamilyId);
                entry.UpdateFamilyAddress(
                    family.AddressLine1,
                    family.AddressLine2,
                    family.AddressTown,
                    family.AddressPostCode);

                if (!string.IsNullOrWhiteSpace(family.FatherFirstName))
                {
                    entry.AddParent(
                        family.FatherTitle,
                        family.FatherFirstName,
                        family.FatherLastName,
                        family.FatherMobile,
                        family.FatherEmail,
                        Parent.SentralReference.Father);
                }

                if (!string.IsNullOrWhiteSpace(family.MotherFirstName))
                {
                    entry.AddParent(
                        family.MotherTitle,
                        family.MotherFirstName,
                        family.MotherLastName,
                        family.MotherMobile,
                        family.MotherEmail,
                        Parent.SentralReference.Mother);
                }

                var familyStudents = await _studentRepository.GetListFromIds(family.StudentIds, token);

                foreach (var student in familyStudents)
                {
                    _logger.Information("{id}: Adding student {name} to family {family} ({code})", jobId, student.DisplayName, family.AddressName, family.FamilyId);

                    entry.AddStudent(student.StudentId, true);
                }

                // Check if email is blank and alert admin
                if (string.IsNullOrEmpty(family.FamilyEmail))
                {
                    changeLog.Add(new ParentContactChangeDto(
                        "Family Email",
                        string.Empty,
                        string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                        "No Email Supplied"));
                } 
                else
                {
                    entry.UpdateFamilyEmail(family.FamilyEmail);

                    changeLog.Add(new ParentContactChangeDto(
                        "Family Email",
                        string.Empty,
                        family.FamilyEmail,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                        "Family Email Changed"));
                }

                foreach (var parent in entry.Parents)
                {
                    changeLog.Add(new ParentContactChangeDto(
                        $"{parent.FirstName.Trim()} {parent.LastName.Trim()}",
                        string.Empty,
                        parent.EmailAddress,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                        "New Parent Added"));
                }

                _familyRepository.Insert(entry);

            } else
            {
                _logger.Information("{id}: Found existing entry for {family} ({code}). Updating details.", jobId, family.AddressName, family.FamilyId);

                entry.UpdateFamilyAddress(
                    family.AddressLine1,
                    family.AddressLine2,
                    family.AddressTown,
                    family.AddressPostCode);

                var familyStudents = await _studentRepository.GetListFromIds(family.StudentIds, token);

                foreach (var student in familyStudents)
                {
                    if (entry.Students.All(student => student.StudentId != student.StudentId))
                    {
                        // Student is not currently linked
                        _logger.Information("{id}: Adding student {name} to family {family} ({code})", jobId, student.DisplayName, family.AddressName, family.FamilyId);

                        entry.AddStudent(student.StudentId, true);
                    }
                }

                var linkedStudents = entry.Students.ToList();
                foreach (var student in linkedStudents)
                {
                    if (!family.StudentIds.Contains(student.StudentId))
                    {
                        _logger.Information("{id}: Removing student {name} from family {family} ({code})", jobId, student.StudentId, family.AddressName, family.FamilyId);

                        // Student should not be linked
                        entry.RemoveStudent(student.StudentId);
                    }
                }

                var fatherEntry = entry.Parents.FirstOrDefault(parent => parent.SentralLink == Parent.SentralReference.Father);

                if (fatherEntry is null)
                {
                    entry.AddParent(
                        family.FatherTitle,
                        family.FatherFirstName,
                        family.FatherLastName,
                        family.FatherMobile,
                        family.FatherEmail,
                        Parent.SentralReference.Father);

                    changeLog.Add(new ParentContactChangeDto(
                        $"{family.FatherFirstName.Trim()} {family.FatherLastName.Trim()}",
                        string.Empty,
                        family.FatherEmail,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                        "New Parent Added"));
                } 
                else if (fatherEntry.Title.ToLower() != family.FatherTitle.ToLower() ||
                        fatherEntry.FirstName.ToLower() != family.FatherFirstName.ToLower() ||
                        fatherEntry.LastName.ToLower() != family.FatherLastName.ToLower() ||
                        fatherEntry.MobileNumber.ToLower() != family.FatherMobile.ToLower() ||
                        fatherEntry.EmailAddress.ToLower() != family.FatherEmail.ToLower())
                {
                    if (fatherEntry.EmailAddress.ToLower() != family.FatherEmail.ToLower())
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            $"{family.FatherFirstName.Trim()} {family.FatherLastName.Trim()}",
                            fatherEntry.EmailAddress,
                            family.FatherEmail,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                            "Parent Email Changed"));
                    }

                    entry.AddParent(
                        family.FatherTitle,
                        family.FatherFirstName,
                        family.FatherLastName,
                        family.FatherMobile,
                        family.FatherEmail,
                        Parent.SentralReference.Father);
                }

                var motherEntry = entry.Parents.FirstOrDefault(parent => parent.SentralLink == Parent.SentralReference.Mother);

                if (motherEntry is null)
                {
                    entry.AddParent(
                        family.MotherTitle,
                        family.MotherFirstName,
                        family.MotherLastName,
                        family.MotherMobile,
                        family.MotherEmail,
                        Parent.SentralReference.Mother);

                    changeLog.Add(new ParentContactChangeDto(
                        $"{family.MotherFirstName.Trim()} {family.MotherLastName.Trim()}",
                        string.Empty,
                        family.MotherEmail,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                        "New Parent Added"));
                }
                else if (motherEntry.Title.ToLower() != family.MotherTitle.ToLower() ||
                        motherEntry.FirstName.ToLower() != family.MotherFirstName.ToLower() ||
                        motherEntry.LastName.ToLower() != family.MotherLastName.ToLower() ||
                        motherEntry.MobileNumber.ToLower() != family.MotherMobile.ToLower() ||
                        motherEntry.EmailAddress.ToLower() != family.MotherEmail.ToLower())
                {
                    if (motherEntry.EmailAddress.ToLower() != family.MotherEmail.ToLower())
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            $"{family.FatherFirstName.Trim()} {family.FatherLastName.Trim()}",
                            motherEntry.EmailAddress,
                            family.MotherEmail,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                            "Parent Email Changed"));
                    }

                    entry.AddParent(
                        family.MotherTitle,
                        family.MotherFirstName,
                        family.MotherLastName,
                        family.MotherMobile,
                        family.MotherEmail,
                        Parent.SentralReference.Mother);
                }

                if (entry.FamilyEmail != family.FamilyEmail)
                {
                    _logger.Information("{id}: {family} ({code}): Updated Family Email Address from {old} to {new}", jobId, family.AddressName, family.FamilyId, entry.FamilyEmail, family.FamilyEmail);

                    if (string.IsNullOrWhiteSpace(entry.FamilyEmail) && !string.IsNullOrWhiteSpace(family.FamilyEmail))
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            string.Empty,
                            family.FamilyEmail,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                            "Family Email Added"));
                    }

                    if (!string.IsNullOrWhiteSpace(entry.FamilyEmail) && !string.IsNullOrWhiteSpace(family.FamilyEmail))
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            entry.FamilyEmail,
                            family.FamilyEmail,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                            "Family Email Changed"));
                    }

                    if (!string.IsNullOrWhiteSpace(entry.FamilyEmail) && string.IsNullOrWhiteSpace(family.FamilyEmail))
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            entry.FamilyEmail,
                            string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                            "Family Email Removed"));
                    }

                    entry.UpdateFamilyEmail(family.FamilyEmail);
                }
            }

            await _unitOfWork.CompleteAsync(token);
        }

        if (changeLog.Count > 0)
            await _mediator.Send(new SendFamilyContactChangesReportCommand(changeLog), token);
    }
}
