﻿namespace Constellation.Application.Domains.Schools.Queries.GetSchoolDetails;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.Enrolments;
using Core.Models.Enrolments.Repositories;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using Students.Queries.GetCurrentStudentsWithCurrentOfferings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolDetailsQueryHandler
: IQueryHandler<GetSchoolDetailsQuery, SchoolDetailsResponse>
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ILogger _logger;

    public GetSchoolDetailsQueryHandler(
        ISchoolRepository schoolRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ISchoolContactRepository contactRepository,
        IEnrolmentRepository enrolmentRepository,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _contactRepository = contactRepository;
        _enrolmentRepository = enrolmentRepository;
        _logger = logger.ForContext<GetSchoolDetailsQuery>();
    }

    public async Task<Result<SchoolDetailsResponse>> Handle(GetSchoolDetailsQuery request, CancellationToken cancellationToken)
    {
        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(GetSchoolDetailsQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to retrieve School details");

            return Result.Failure<SchoolDetailsResponse>(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }

        List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(school.Code, cancellationToken);
        
        List<StudentWithOfferingsResponse> studentResponse = new();

        foreach (Student student in students)
        {
            List<StudentWithOfferingsResponse.OfferingResponse> studentOfferings = new();

            List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(student.Id, cancellationToken);

            foreach (Enrolment enrolment in enrolments)
            {
                Offering offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

                if (offering is null)
                    continue;

                studentOfferings.Add(new(
                    offering.Id,
                    offering.Name,
                    offering.IsCurrent));
            }

            SchoolEnrolment? schoolEnrolment = student.CurrentEnrolment;

            bool currentEnrolment = true;

            if (schoolEnrolment is null)
            {
                currentEnrolment = false;

                // retrieve most recent applicable school enrolment
                if (student.SchoolEnrolments.Count > 0)
                {
                    int maxYear = student.SchoolEnrolments.Max(item => item.Year);

                    SchoolEnrolmentId enrolmentId = student.SchoolEnrolments
                        .Where(entry => entry.Year == maxYear)
                        .Select(entry => new { entry.Id, Date = entry.EndDate ?? DateOnly.MaxValue })
                    .MaxBy(entry => entry.Date)
                        .Id;

                    schoolEnrolment = student.SchoolEnrolments.FirstOrDefault(entry => entry.Id == enrolmentId);
                }
            }

            studentResponse.Add(new(
                student.Id,
                student.StudentReferenceNumber,
                student.Name,
                student.Gender,
                schoolEnrolment?.SchoolName,
                schoolEnrolment?.Grade,
                studentOfferings,
                currentEnrolment));
        }

        List<StaffMember> teachers = await _staffRepository.GetActiveFromSchool(school.Code, cancellationToken);
        List<Faculty> faculties = await _facultyRepository.GetAll(cancellationToken);
        List<SchoolDetailsResponse.SchoolStaff> staffResponse = new();

        foreach (StaffMember teacher in teachers)
        {
            List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(teacher.Id, cancellationToken);

            List<SchoolDetailsResponse.OfferingResponse> teacherOfferings = offerings
                .Where(offering => 
                    offering.Teachers.Any(entry => 
                        entry.StaffId == teacher.Id && 
                        entry.Type == AssignmentType.ClassroomTeacher))
                .Select(offering => new SchoolDetailsResponse.OfferingResponse(
                    offering.Id,
                    offering.Name,
                    offering.IsCurrent))
                .ToList();

            List<Faculty> teacherFaculties = await _facultyRepository.GetCurrentForStaffMember(teacher.Id, cancellationToken);
            Dictionary<string, string> teacherFacultyList = teacherFaculties.ToDictionary(key => key.Name, value => value.Colour);
            
            staffResponse.Add(new(
                teacher.Id,
                teacher.Name,
                teacherFacultyList,
                teacherOfferings));
        }

        List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(school.Code, cancellationToken);
        List<SchoolDetailsResponse.SchoolContact> contactResponse = new();

        foreach (SchoolContact contact in contacts)
        {
            foreach (SchoolContactRole role in contact.Assignments.Where(role => !role.IsDeleted && role.SchoolCode == school.Code))
            {
                Result<Name> name = contact.GetName();

                if (name.IsFailure)
                {
                    _logger
                        .ForContext(nameof(Error), name.Error, true)
                        .ForContext(nameof(SchoolContact), contact, true)
                        .ForContext(nameof(SchoolContactRole), role, true)
                        .Warning("Failed to retrieve School details");
                }

                PhoneNumber phoneNumber = PhoneNumber.Empty;

                if (!string.IsNullOrWhiteSpace(contact.PhoneNumber))
                {
                    Result<PhoneNumber> convertedNumber = PhoneNumber.Create(contact.PhoneNumber);

                    if (convertedNumber.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(Error), convertedNumber.Error, true)
                            .ForContext(nameof(SchoolContact), contact, true)
                            .ForContext(nameof(SchoolContactRole), role, true)
                            .Warning("Failed to retrieve School details");
                    }
                    else
                    {
                        phoneNumber = convertedNumber.Value;
                    }
                }

                EmailAddress emailAddress = EmailAddress.None;

                if (!string.IsNullOrWhiteSpace(contact.EmailAddress))
                {
                    Result<EmailAddress> convertedEmail = EmailAddress.Create(contact.EmailAddress);

                    if (convertedEmail.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(Error), convertedEmail.Error, true)
                            .ForContext(nameof(SchoolContact), contact, true)
                            .ForContext(nameof(SchoolContactRole), role, true)
                            .Warning("Failed to retrieve School details");
                    }
                    else
                    {
                        emailAddress = convertedEmail.Value;
                    }
                }

                contactResponse.Add(new(
                    contact.Id,
                    role.Id,
                    name.Value,
                    role.Role,
                    role.Note,
                    phoneNumber,
                    emailAddress));
            }
        }

        return new SchoolDetailsResponse(
            school.Code,
            school.Name,
            school.Address,
            school.Town,
            school.State,
            school.PostCode,
            school.PhoneNumber,
            school.FaxNumber,
            school.EmailAddress,
            school.HeatSchool,
            school.Directorate,
            school.PrincipalNetwork,
            school.EducationalServicesTeam,
            staffResponse,
            studentResponse,
            contactResponse);
    }
}
