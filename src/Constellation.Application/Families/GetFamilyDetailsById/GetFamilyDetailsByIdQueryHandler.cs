namespace Constellation.Application.Families.GetFamilyDetailsById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFamilyDetailsByIdQueryHandler
    : IQueryHandler<GetFamilyDetailsByIdQuery, FamilyDetailsResponse>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;

    public GetFamilyDetailsByIdQueryHandler(
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository)
    {
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<FamilyDetailsResponse>> Handle(GetFamilyDetailsByIdQuery request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure<FamilyDetailsResponse>(DomainErrors.Families.Family.NotFound(request.FamilyId));

        List<FamilyDetailsResponse.StudentResponse> students = new();

        foreach (var member in family.Students)
        {
            var student = await _studentRepository.GetById(member.StudentId, cancellationToken);

            if (student is null)
                continue;

            var school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

            if (school is null) 
                continue;

            var name = Name.Create(student.FirstName, null, student.LastName);
            var email = EmailAddress.Create(student.EmailAddress);

            var entry = new FamilyDetailsResponse.StudentResponse(
                student.StudentId,
                name.Value,
                email.Value,
                student.SchoolCode,
                school.Name,
                student.CurrentGrade);

            students.Add(entry);
        }

        List<FamilyDetailsResponse.ParentResponse> parents = new();

        foreach (var parent in family.Parents)
        {
            var name = Name.Create(parent.FirstName, null, parent.LastName);
            var email = EmailAddress.Create(parent.EmailAddress);
            var mobile = PhoneNumber.Create(parent.MobileNumber);

            var entry = new FamilyDetailsResponse.ParentResponse(
                parent.Id,
                name.Value,
                email.Value,
                (mobile.IsSuccess ? mobile.Value : null));

            parents.Add(entry);
        }

        return new FamilyDetailsResponse(
            family.Id,
            family.FamilyTitle,
            family.AddressLine1,
            family.AddressLine2,
            family.AddressTown,
            family.AddressPostCode,
            family.FamilyEmail,
            parents,
            students,
            family.Students.Any(member => member.IsResidentialFamily));
    }
}
