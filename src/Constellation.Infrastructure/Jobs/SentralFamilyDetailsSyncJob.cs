namespace Constellation.Infrastructure.Jobs;

using Application.Domains.Students.Commands.SendFamilyContactChangesReport;
using Application.DTOs;
using Application.Interfaces.Gateways;
using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Jobs;
using Core.Abstractions.Repositories;
using Core.Extensions;
using Core.Models.Families;
using Core.Models.Identifiers;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SentralFamilyDetailsSyncJob : ISentralFamilyDetailsSyncJob
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
        _logger.ForContext(nameof(jobId), jobId);

        _logger
            .Information("Starting Sentral Family Details Scan.");

        List<ParentContactChangeDto> changeLog = new();
        List<Student> students = await _studentRepository.GetCurrentStudents(token);

        // Get the CSV file from Sentral
        // Convert to temporary objects
        //ICollection<FamilyDetailsDto> families = await _gateway.GetFamilyDetailsReport(_logger);

#region 2025-02-06: Converted to API use
        // 2025-02-06: Converted to API use
        //List<FamilyDetailsDto> families = new();

        //Dictionary<string, List<string>> familyGroups = await _gateway.GetFamilyGroupings();


        //foreach (KeyValuePair<string, List<string>> family in familyGroups)
        //{
        //    Student firstStudent = students.FirstOrDefault(student => student.StudentReferenceNumber.Number == family.Value.First());

        //    if (firstStudent is null)
        //        continue;

        //    SystemLink link = firstStudent.SystemLinks.FirstOrDefault(link => link.System == SystemType.Sentral);

        //    if (link is null)
        //        continue;

        //    FamilyDetailsDto entry = await _gateway.GetParentContactEntry(link.Value);

        //    entry.StudentReferenceNumbers = family.Value;
        //    entry.FamilyId = family.Key;

        //    foreach (FamilyDetailsDto.Contact contact in entry.Contacts)
        //    {
        //        string name = contact.FirstName.Contains(' ')
        //            ? contact.FirstName.Split(' ')[0]
        //            : contact.FirstName;

        //        name = name.Length > 8 ? name[..8] : name;

        //        contact.SentralId = $"{entry.FamilyId}-{contact.SentralReference}-{name.ToLowerInvariant()}";
        //    }

        //    families.Add(entry);
        //}
#endregion

        ICollection<FamilyDetailsDto> families = await _gateway.GetFamilyDetailsReportFromApi(_logger, token);

        _logger
            .Information("Found {count} families", families.Count);

        // if there are no families in Sentral, end now
        if (families.Count == 0)
            return;

        List<Family> dbFamilies = await _familyRepository.GetAll(token);

        // Check objects against database
        foreach (FamilyDetailsDto family in families)
        {
            if (token.IsCancellationRequested)
                return;

            _logger
                .Information("Checking family: {name} ({code})", family.AddressName, family.FamilyId);

            foreach (FamilyDetailsDto.Contact contact in family.Contacts)
                contact.Mobile = contact.Mobile.Replace(" ", "");

            // Check family exists in database
            Family entry = dbFamilies.FirstOrDefault(entry => entry.SentralId == family.FamilyId);

            if (entry is null)
            {
                _logger
                    .Information("No existing entry for {name} ({code}). Creating new family.", family.AddressName, family.FamilyId);

                // New Family... Add to database
                entry = Family.Create(new FamilyId(), family.AddressName);
                entry.LinkFamilyToSentralDetails(family.FamilyId);
                entry.UpdateFamilyAddress(
                    family.AddressName,
                    family.AddressLine1,
                    family.AddressLine2,
                    family.AddressTown,
                    family.AddressPostCode);

                List<Student> familyStudents = students
                    .Where(student => student.StudentReferenceNumber != StudentReferenceNumber.Empty)
                    .Where(student => family.StudentReferenceNumbers.Contains(student.StudentReferenceNumber.Number)).ToList();

                foreach (Student student in familyStudents)
                {
                    _logger
                        .Information("Adding student {name} to family {family} ({code})", student.Name.DisplayName, family.AddressName, family.FamilyId);

                    entry.AddStudent(student.Id, student.StudentReferenceNumber, true);
                }

                foreach (FamilyDetailsDto.Contact contact in family.Contacts)
                {
                    ParentContactChangeDto logEntry = CreateNewParent(
                        contact.Title,
                        contact.FirstName,
                        contact.LastName,
                        contact.Mobile,
                        contact.Email,
                        contact.SentralReference,
                        contact.SentralId,
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
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.FirstName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.LastName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentEnrolment?.Grade.AsName() : string.Empty,
                        "No Email Supplied"));
                } 
                else
                {
                    Result<EmailAddress> email = EmailAddress.Create(family.FamilyEmail);

                    if (email.IsFailure)
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            string.Empty,
                            family.FamilyEmail,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.PreferredName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentEnrolment?.Grade.AsName() : string.Empty,
                            "Family Email Invalid"));
                    }
                    else
                    {
                        Result result = entry.UpdateFamilyEmail(family.FamilyEmail);

                        if (result.IsSuccess)
                        {
                            changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            string.Empty,
                            family.FamilyEmail,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.PreferredName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentEnrolment?.Grade.AsName() : string.Empty,
                            "Family Email Added"));
                        }
                    }
                }

                _familyRepository.Insert(entry);
            } 
            else
            {
                _logger
                    .Information("Found existing entry for {family} ({code}). Updating details.", family.AddressName, family.FamilyId);

                if (entry.IsDeleted)
                    entry.Reinstate();

                entry.UpdateFamilyAddress(
                    family.AddressName,
                    family.AddressLine1,
                    family.AddressLine2,
                    family.AddressTown,
                    family.AddressPostCode);

                List<Student> familyStudents = students
                    .Where(student => student.StudentReferenceNumber != StudentReferenceNumber.Empty)
                    .Where(student => family.StudentReferenceNumbers.Contains(student.StudentReferenceNumber.Number))
                    .ToList();

                List<Student> dbStudents = students
                    .Where(student => entry.Students.Any(member => member.StudentId == student.Id))
                    .ToList();

                foreach (Student dbStudent in dbStudents)
                {
                    // Student is not listed in the Sentral Students list, but is in the db Students list
                    if (familyStudents.All(sentralStudent => sentralStudent.Id != dbStudent.Id))
                    {
                        _logger
                            .Information("Removing student {name} from family {family} ({code})", dbStudent.Name.DisplayName, family.AddressName, family.FamilyId);

                        // Student should not be linked
                        entry.RemoveStudent(dbStudent.Id);
                    }
                }

                foreach (Student sentralStudent in familyStudents)
                {
                    // Student is listed in the Sentral Students list, but is not in the db Students list
                    if (entry.Students.All(entry => entry.StudentId != sentralStudent.Id))
                    {
                        // Student is not currently linked
                        _logger
                            .Information("Adding student {name} to family {family} ({code})", sentralStudent.Name.DisplayName, family.AddressName, family.FamilyId);

                        entry.AddStudent(sentralStudent.Id, sentralStudent.StudentReferenceNumber, true);
                    }
                }
                
                foreach (Parent parent in entry.Parents.ToList())
                {
                    FamilyDetailsDto.Contact contact = family.Contacts.FirstOrDefault(contact => contact.SentralId == parent.SentralId);

                    if (contact is null)
                    {
                        // Parent should be removed as it is not in the latest pull from Sentral
                        entry.RemoveParent(parent.Id);
                    }
                }

                foreach (FamilyDetailsDto.Contact contact in family.Contacts)
                {
                    Parent parent = entry.Parents.FirstOrDefault(parent => parent.SentralId == contact.SentralId);

                    if (parent is not null)
                    {
                        ParentContactChangeDto logEntry = UpdateParent(
                            parent,
                            contact.Title,
                            contact.FirstName,
                            contact.LastName,
                            contact.Mobile,
                            contact.Email,
                            contact.SentralReference,
                            contact.SentralId,
                            familyStudents.FirstOrDefault(),
                            entry);

                        if (logEntry is not null)
                            changeLog.Add(logEntry);
                    }
                    else
                    {
                        ParentContactChangeDto logEntry = CreateNewParent(
                            contact.Title,
                            contact.FirstName,
                            contact.LastName,
                            contact.Mobile,
                            contact.Email,
                            contact.SentralReference,
                            contact.SentralId,
                            familyStudents.FirstOrDefault(),
                            entry);

                        if (logEntry is not null)
                            changeLog.Add(logEntry);
                    }
                }

                Result<EmailAddress> entryEmail = EmailAddress.Create(entry.FamilyEmail);

                if (entryEmail.IsFailure)
                {
                    entryEmail = EmailAddress.Create("a@a.com");
                }

                Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);
                
                if (familyEmail.IsFailure)
                {
                    _logger
                        .Warning("{family} ({code}): Family email address has changed from {oldEntry} to {newEntry} and is invalid", family.AddressName, family.FamilyId, entry.FamilyEmail, family.FamilyEmail);

                    changeLog.Add(new ParentContactChangeDto(
                        "Family Email",
                        string.Empty,
                        family.FamilyEmail,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.PreferredName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.LastName : string.Empty,
                        (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentEnrolment?.Grade.AsName() : string.Empty,
                        "Family Email Invalid"));
                }
                else if (familyEmail.Value != entryEmail.Value)
                {
                    string oldEmail = entry.FamilyEmail;

                    Result result = entry.UpdateFamilyEmail(family.FamilyEmail);

                    if (result.IsFailure)
                        continue;

                    if (string.IsNullOrWhiteSpace(oldEmail) && !string.IsNullOrWhiteSpace(family.FamilyEmail))
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            string.Empty,
                            family.FamilyEmail,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.PreferredName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentEnrolment?.Grade.AsName() : string.Empty,
                            "Family Email Added"));
                    }

                    if (!string.IsNullOrWhiteSpace(oldEmail) && !string.IsNullOrWhiteSpace(family.FamilyEmail))
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            oldEmail,
                            family.FamilyEmail,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.PreferredName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentEnrolment?.Grade.AsName() : string.Empty,
                            "Family Email Changed"));
                    }

                    if (!string.IsNullOrWhiteSpace(oldEmail) && string.IsNullOrWhiteSpace(family.FamilyEmail))
                    {
                        changeLog.Add(new ParentContactChangeDto(
                            "Family Email",
                            oldEmail,
                            string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.PreferredName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().Name.LastName : string.Empty,
                            (familyStudents.FirstOrDefault() is not null) ? familyStudents.First().CurrentEnrolment?.Grade.AsName() : string.Empty,
                            "Family Email Removed"));
                    }

                    _logger
                        .Information("{family} ({code}): Updated Family Email Address from {old} to {new}", family.AddressName, family.FamilyId, oldEmail, family.FamilyEmail);
                }
            }

            await _unitOfWork.CompleteAsync(token);
        }

        List<string> dbFamilySentralIds = dbFamilies
            .Where(family => 
                !family.IsDeleted &&
                !string.IsNullOrWhiteSpace(family.SentralId)) // No SentralId means these are non-residential families
            .Select(family => family.SentralId)
            .ToList();

        List<string> familySentralIds = families
            .Select(family => family.FamilyId)
            .ToList();

        IEnumerable<string> staleDbFamilySentralIds = dbFamilySentralIds.Except(familySentralIds).ToList();

        if (staleDbFamilySentralIds.Any())
        {
            foreach (string familyId in staleDbFamilySentralIds)
            {
                Family family = dbFamilies.First(entry => entry.SentralId == familyId);

                family.Delete();
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
        string sentralId,
        Student? firstStudent,
        Family entry)
    {
        Result<EmailAddress> email = EmailAddress.Create(emailAddress);
        Result<Name> name = Name.Create(firstName, string.Empty, lastName);
        string displayName = name.IsSuccess ? name.Value.DisplayName : $"{firstName} {lastName}";

        if (email.IsFailure)
        {
            return new ParentContactChangeDto(
                displayName,
                string.Empty,
                emailAddress,
                (firstStudent is not null) ? firstStudent.Name.PreferredName : string.Empty,
                (firstStudent is not null) ? firstStudent.Name.LastName : string.Empty,
                (firstStudent is not null) ? firstStudent.CurrentEnrolment?.Grade.AsName() : string.Empty,
                "Parent Email Invalid");
        }
        
        Result<Parent> result = entry.AddParent(
            title,
            firstName,
            lastName,
            mobile,
            email.Value.Email,
            parentType);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Parent update failed due to error");

            return null;
        }

        result.Value.SetSentralId(sentralId);

        return new ParentContactChangeDto(
            displayName,
            string.Empty,
            email.Value.Email,
            (firstStudent is not null) ? firstStudent.Name.PreferredName : string.Empty,
            (firstStudent is not null) ? firstStudent.Name.LastName : string.Empty,
            (firstStudent is not null) ? firstStudent.CurrentEnrolment?.Grade.AsName() : string.Empty,
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
        string sentralId,
        Student? firstStudent,
        Family entry)
    {
        Result<Name> name = Name.Create(firstName, string.Empty, lastName);
        string displayName = name.IsSuccess ? name.Value.DisplayName : $"{firstName} {lastName}";

        _logger
            .Information("Updating parent details for {parentName} in family {family} ({code})", displayName, entry.FamilyTitle, entry.SentralId);

        Result<EmailAddress> email = EmailAddress.Create(emailAddress);
        if (email.IsFailure)
        {
            _logger
                .Information("Parent email has changed from {oldEntry} to {newEntry} however new entry is invalid", existingParent.EmailAddress, emailAddress);

            return new ParentContactChangeDto(
                displayName,
                string.Empty,
                emailAddress,
                (firstStudent is not null) ? firstStudent.Name.PreferredName : string.Empty,
                (firstStudent is not null) ? firstStudent.Name.LastName : string.Empty,
                (firstStudent is not null) ? firstStudent.CurrentEnrolment?.Grade.AsName() : string.Empty,
                "Parent Email Invalid");
        }
        
        if (existingParent.EmailAddress != email.Value.Email)
        {
            // Email has changed
            _logger
                .Information("Parent email has changed from {oldEntry} to {newEntry}", existingParent.EmailAddress, emailAddress);
        }

        if (existingParent.Title != title)
        {
            // Title has changed
            _logger
                .Information("Parent title has changed from {oldEntry} to {newEntry}", existingParent.Title, title);
        }

        if (existingParent.FirstName != firstName)
        {
            // FirstName has changed
            _logger
                .Information("Parent first name has changed from {oldEntry} to {newEntry}", existingParent.FirstName, firstName);
        }

        if (existingParent.LastName != lastName)
        {
            // LastName has changed
            _logger
                .Information("Parent last name has changed from {oldEntry} to {newEntry}", existingParent.LastName, lastName);
        }

        Result<PhoneNumber> mobileCheck = PhoneNumber.Create(mobile);
        string mobileNumber = mobileCheck.IsSuccess ? mobileCheck.Value.ToString(PhoneNumber.Format.None) : string.Empty;

        if (existingParent.MobileNumber != mobileNumber)
        {
            // MobileNumber has changed
            _logger
                .Information("Parent mobile number has changed from {oldEntry} to {newEntry}", existingParent.MobileNumber, mobileNumber);
        }

        bool emailChanged = existingParent.EmailAddress != email.Value.Email;
        string oldEmail = existingParent.EmailAddress;

        Result<Parent> result = entry.UpdateParent(
            existingParent.Id,
            title,
            firstName,
            lastName,
            mobile,
            email.Value.Email,
            parentType);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Parent update failed due to error");
            
            return null;
        }

        result.Value.SetSentralId(sentralId);

        if (emailChanged)
        {
            return new ParentContactChangeDto(
                displayName,
                oldEmail,
                email.Value.Email,
                (firstStudent is not null) ? firstStudent.Name.PreferredName : string.Empty,
                (firstStudent is not null) ? firstStudent.Name.LastName : string.Empty,
                (firstStudent is not null) ? firstStudent.CurrentEnrolment?.Grade.AsName() : string.Empty,
                "Parent Email Updated");
        }

        return null;
    }
}
