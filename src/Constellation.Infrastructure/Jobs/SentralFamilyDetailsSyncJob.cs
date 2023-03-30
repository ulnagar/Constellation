namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Students.SendFamilyContactChangesReport;
using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class SentralFamilyDetailsSyncJob : ISentralFamilyDetailsSyncJob, IHangfireJob
{
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISentralGateway _gateway;
    private readonly IMediator _mediator;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;

    private Guid _jobId;

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
        _jobId = jobId;

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
                entry = Family.Create(new FamilyId(), family.AddressName);
                entry.LinkFamilyToSentralDetails(family.FamilyId);
                entry.UpdateFamilyAddress(
                    family.AddressName,
                    family.AddressLine1,
                    family.AddressLine2,
                    family.AddressTown,
                    family.AddressPostCode);

                var familyStudents = await _studentRepository.GetListFromIds(family.StudentIds, token);

                foreach (var student in familyStudents)
                {
                    _logger.Information("{id}: Adding student {name} to family {family} ({code})", jobId, student.DisplayName, family.AddressName, family.FamilyId);

                    entry.AddStudent(student.StudentId, true);
                }

                if (!string.IsNullOrWhiteSpace(family.FatherFirstName))
                {
                    var logEntry = CreateNewParent(
                        family.FatherTitle,
                        family.FatherFirstName,
                        family.FatherLastName,
                        family.FatherMobile,
                        family.FatherEmail,
                        Parent.SentralReference.Father,
                        familyStudents.FirstOrDefault(),
                        entry);

                    if (logEntry is not null)
                        changeLog.Add(logEntry);
                }

                if (!string.IsNullOrWhiteSpace(family.MotherFirstName))
                {
                    var logEntry = CreateNewParent(
                        family.MotherTitle,
                        family.MotherFirstName,
                        family.MotherLastName,
                        family.MotherMobile,
                        family.MotherEmail,
                        Parent.SentralReference.Mother,
                        familyStudents.FirstOrDefault(),
                        entry);

                    if (logEntry is not null)
                        changeLog.Add(logEntry);
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
                    var email = EmailAddress.Create(family.FamilyEmail);

                    if (email.IsFailure)
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            string.Empty,
                            family.FamilyEmail,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                            "Family Email Invalid"));
                    }
                    else
                    {
                        var result = entry.UpdateFamilyEmail(family.FamilyEmail);

                        if (result.IsSuccess)
                        {
                            changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            string.Empty,
                            family.FamilyEmail,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                            "Family Email Changed"));
                        }
                    }
                }

                _familyRepository.Insert(entry);
            } 
            else
            {
                _logger.Information("{id}: Found existing entry for {family} ({code}). Updating details.", jobId, family.AddressName, family.FamilyId);

                entry.UpdateFamilyAddress(
                    family.AddressName,
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

                if (fatherEntry is null && !string.IsNullOrWhiteSpace(family.FatherFirstName))
                {
                    var logEntry = CreateNewParent(
                        family.FatherTitle,
                        family.FatherFirstName,
                        family.FatherLastName,
                        family.FatherMobile,
                        family.FatherEmail,
                        Parent.SentralReference.Father,
                        familyStudents.FirstOrDefault(),
                        entry);

                    if (logEntry is not null)
                        changeLog.Add(logEntry);
                } 
                else if (!string.IsNullOrWhiteSpace(family.FatherFirstName))
                {
                    var logEntry = UpdateParent(
                        fatherEntry,
                        family.FatherTitle,
                        family.FatherFirstName,
                        family.FatherLastName,
                        family.FatherMobile,
                        family.FatherEmail,
                        Parent.SentralReference.Father,
                        familyStudents.FirstOrDefault(),
                        entry);

                    if (logEntry is not null)
                        changeLog.Add(logEntry);
                }

                var motherEntry = entry.Parents.FirstOrDefault(parent => parent.SentralLink == Parent.SentralReference.Mother);

                if (motherEntry is null && !string.IsNullOrWhiteSpace(family.MotherFirstName))
                {
                    var logEntry = CreateNewParent(
                        family.MotherTitle,
                        family.MotherFirstName,
                        family.MotherLastName,
                        family.MotherMobile,
                        family.MotherEmail,
                        Parent.SentralReference.Mother,
                        familyStudents.FirstOrDefault(),
                        entry);

                    if (logEntry is not null)
                        changeLog.Add(logEntry);
                }
                else if (!string.IsNullOrWhiteSpace(family.MotherFirstName))
                {
                    var logEntry = UpdateParent(
                        motherEntry,
                        family.MotherTitle,
                        family.MotherFirstName,
                        family.MotherLastName,
                        family.MotherMobile,
                        family.MotherEmail,
                        Parent.SentralReference.Mother,
                        familyStudents.FirstOrDefault(),
                        entry);

                    if (logEntry is not null)
                        changeLog.Add(logEntry);
                }

                if (entry.FamilyEmail != family.FamilyEmail)
                {
                    var email = EmailAddress.Create(family.FamilyEmail);

                    if (email.IsFailure)
                    {
                        _logger.Warning("{id}: {family} ({code}): Family email address has changed from {oldEntry} to {newEntry} and is invalid", jobId, family.AddressName, family.FamilyId, entry.FamilyEmail, family.FamilyEmail);

                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            string.Empty,
                            family.FamilyEmail,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().FirstName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentGrade.AsName() : string.Empty,
                            "Family Email Invalid"));
                    }

                    var oldEmail = entry.FamilyEmail;

                    var result = entry.UpdateFamilyEmail(family.FamilyEmail);

                    if (result.IsFailure)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(oldEmail) && !string.IsNullOrWhiteSpace(family.FamilyEmail))
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

                    if (!string.IsNullOrWhiteSpace(oldEmail) && !string.IsNullOrWhiteSpace(family.FamilyEmail))
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

                    if (!string.IsNullOrWhiteSpace(oldEmail) && string.IsNullOrWhiteSpace(family.FamilyEmail))
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

                    _logger.Information("{id}: {family} ({code}): Updated Family Email Address from {old} to {new}", jobId, family.AddressName, family.FamilyId, oldEmail, family.FamilyEmail);
                }
            }

            await _unitOfWork.CompleteAsync(token);
        }

        if (changeLog.Count > 0)
            await _mediator.Send(new SendFamilyContactChangesReportCommand(changeLog), token);
    }

    private ParentContactChangeDto CreateNewParent(
        string title,
        string firstName,
        string lastName,
        string mobile,
        string emailAddress,
        Parent.SentralReference parentType,
        Student? firstStudent,
        Family entry)
    {
        var email = EmailAddress.Create(emailAddress);
        var name = Name.Create(firstName, string.Empty, lastName);
        var displayName = name.IsSuccess ? name.Value.DisplayName : $"{firstName} {lastName}";

        if (email.IsFailure)
        {
            return new ParentContactChangeDto(
                displayName,
                string.Empty,
                emailAddress,
                (firstStudent is not null) ? firstStudent.FirstName : string.Empty,
                (firstStudent is not null) ? firstStudent.LastName : string.Empty,
                (firstStudent is not null) ? firstStudent.CurrentGrade.AsName() : string.Empty,
                "Parent Email Invalid");
        }
        
        var result = entry.AddParent(
            title,
            firstName,
            lastName,
            mobile,
            email.Value.Email,
            parentType);

        if (result.IsFailure)
        {
            _logger.Warning("{id}: Parent update failed due to error {@error}", _jobId, result.Error);

            return null;
        }

        return new ParentContactChangeDto(
            displayName,
            string.Empty,
            email.Value.Email,
            (firstStudent is not null) ? firstStudent.FirstName : string.Empty,
            (firstStudent is not null) ? firstStudent.LastName : string.Empty,
            (firstStudent is not null) ? firstStudent.CurrentGrade.AsName() : string.Empty,
            "New Parent Added");
    }

    private ParentContactChangeDto UpdateParent(
        Parent existingParent,        
        string title,
        string firstName,
        string lastName,
        string mobile,
        string emailAddress,
        Parent.SentralReference parentType,
        Student? firstStudent,
        Family entry)
    {
        var name = Name.Create(firstName, string.Empty, lastName);
        var displayName = name.IsSuccess ? name.Value.DisplayName : $"{firstName} {lastName}";

        _logger.Information("{id}: Updating parent details for {parentName} in family {family} ({code})", _jobId, displayName, entry.FamilyTitle, entry.SentralId);

        var email = EmailAddress.Create(emailAddress);
        if (email.IsFailure)
        {
            _logger.Information("{id}: Parent email has changed from {oldEntry} to {newEntry} however new entry is invalid", _jobId, existingParent.EmailAddress, emailAddress);

            return new ParentContactChangeDto(
                displayName,
                string.Empty,
                emailAddress,
                (firstStudent is not null) ? firstStudent.FirstName : string.Empty,
                (firstStudent is not null) ? firstStudent.LastName : string.Empty,
                (firstStudent is not null) ? firstStudent.CurrentGrade.AsName() : string.Empty,
                "Parent Email Invalid");
        }
        else if (existingParent.EmailAddress != email.Value.Email)
        {
            // Email has changed
            _logger.Information("{id}: Parent email has changed from {oldEntry} to {newEntry}", _jobId, existingParent.EmailAddress, emailAddress);
        }

        if (existingParent.Title != title)
        {
            // Title has changed
            _logger.Information("{id}: Parent title has changed from {oldEntry} to {newEntry}", _jobId, existingParent.Title, title);
        }

        if (existingParent.FirstName != firstName)
        {
            // FirstName has changed
            _logger.Information("{id}: Parent first name has changed from {oldEntry} to {newEntry}", _jobId, existingParent.FirstName, firstName);
        }

        if (existingParent.LastName != lastName)
        {
            // LastName has changed
            _logger.Information("{id}: Parent last name has changed from {oldEntry} to {newEntry}", _jobId, existingParent.LastName, lastName);
        }

        var mobileCheck = PhoneNumber.Create(mobile);
        var mobileNumber = mobileCheck.IsSuccess ? mobileCheck.Value.ToString(PhoneNumber.Format.None) : string.Empty;

        if (existingParent.MobileNumber != mobileNumber)
        {
            // MobileNumber has changed
            _logger.Information("{id}: Parent mobile number has changed from {oldEntry} to {newEntry}", _jobId, existingParent.MobileNumber, mobileNumber);
        }

        var emailChanged = existingParent.EmailAddress == email.Value.Email;
        var oldEmail = existingParent.EmailAddress;

        var result = entry.UpdateParent(
            existingParent.Id,
            title,
            firstName,
            lastName,
            mobile,
            email.Value.Email,
            parentType);

        if (result.IsFailure)
        {
            _logger.Warning("{id}: Parent update failed due to error {@error}", _jobId, result.Error);
            
            return null;
        }

        if (emailChanged)
        {
            return new ParentContactChangeDto(
                displayName,
                oldEmail,
                email.Value.Email,
                (firstStudent is not null) ? firstStudent.FirstName : string.Empty,
                (firstStudent is not null) ? firstStudent.LastName : string.Empty,
                (firstStudent is not null) ? firstStudent.CurrentGrade.AsName() : string.Empty,
                "Parent Email Updated");
        }

        return null;
    }
}
