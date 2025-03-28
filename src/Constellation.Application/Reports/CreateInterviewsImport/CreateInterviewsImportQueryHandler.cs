﻿namespace Constellation.Application.Reports.CreateInterviewsImport;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.Subjects.Repositories;
using Core.Extensions;
using Core.Models;
using Core.Models.Families;
using Core.Models.OfferingEnrolments;
using Core.Models.OfferingEnrolments.Repositories;
using Core.Models.Offerings;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Shared;
using DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateInterviewsImportQueryHandler
: IQueryHandler<CreateInterviewsImportQuery, List<InterviewExportDto>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IOfferingEnrolmentRepository _offeringEnrolmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;

    public CreateInterviewsImportQueryHandler(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IOfferingEnrolmentRepository offeringEnrolmentRepository,
        ICourseRepository courseRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _offeringEnrolmentRepository = offeringEnrolmentRepository;
        _courseRepository = courseRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
    }

    public async Task<Result<List<InterviewExportDto>>> Handle(CreateInterviewsImportQuery request, CancellationToken cancellationToken)
    {
        List<InterviewExportDto> result = new();

        List<Student> students = await _studentRepository.ForInterviewsExportAsync(request.Grades, request.ClassList, cancellationToken);

        List<Course> courses = await _courseRepository.GetAll(cancellationToken);

        List<Offering> offerings = await _offeringRepository.GetAllActive(cancellationToken);
        
        foreach (Student student in students)
        {
            List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.Id, cancellationToken);

            if (request.ResidentialFamilyOnly)
                families = families
                    .Where(entry =>
                        entry.Students.Any(member =>
                        member.StudentId == student.Id &&
                            member.IsResidentialFamily))
                .ToList();

            List<OfferingEnrolment> validEnrolments = await _offeringEnrolmentRepository.GetCurrentByStudentId(student.Id, cancellationToken);

            foreach (Family family in families)
            {
                foreach (OfferingEnrolment enrolment in validEnrolments)
                {
                    Offering offering = offerings.FirstOrDefault(offering => offering.Id == enrolment.OfferingId);

                    if (offering is null)
                        continue;

                    Course course = courses.FirstOrDefault(course => course.Id == offering.CourseId);

                    if (course is null)
                        continue;

                    List<Staff> teachers = await _staffRepository.GetPrimaryTeachersForOffering(enrolment.OfferingId, cancellationToken);

                    foreach (Staff teacher in teachers)
                    {
                        InterviewExportDto dto = new()
                        {
                            StudentId = student.StudentReferenceNumber.Number,
                            StudentFirstName = student.Name.FirstName,
                            StudentLastName = student.Name.LastName,
                            ClassCode = offering.Name,
                            ClassGrade = course.Grade.AsNumber(),
                            ClassName = course.Name,
                            TeacherCode = teacher.StaffId,
                            TeacherTitle = "",
                            TeacherFirstName = teacher.FirstName,
                            TeacherLastName = teacher.LastName,
                            TeacherEmailAddress = teacher.EmailAddress
                        };

                        if (request.PerFamily)
                        {
                            string lastName = family.FamilyTitle.Split(' ').Last();
                            string firstName = family.FamilyTitle[..^lastName.Length].Trim();
                            string email = family.FamilyEmail;

                            InterviewExportDto.Parent entry = new()
                            {
                                ParentCode = email,
                                ParentFirstName = firstName,
                                ParentLastName = lastName,
                                ParentEmailAddress = email
                            };

                            dto.Parents.Add(entry);

                            result.Add(dto);
                            continue;
                        }

                        foreach (Parent parent in family.Parents)
                        {
                            InterviewExportDto.Parent entry = new()
                            {
                                ParentCode = parent.EmailAddress,
                                ParentFirstName = parent.FirstName,
                                ParentLastName = parent.LastName,
                                ParentEmailAddress = parent.EmailAddress
                            };

                            dto.Parents.Add(entry);
                        }

                        result.Add(dto);
                    }
                }
            }
        }

        return result;
    }
}
