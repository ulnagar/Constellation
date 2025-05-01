namespace Constellation.Application.Domains.Families.Queries.GetFamilyDetailsById;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Families;
using Core.Models.Families.Errors;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFamilyDetailsByIdQueryHandler
    : IQueryHandler<GetFamilyDetailsByIdQuery, FamilyDetailsResponse>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;

    public GetFamilyDetailsByIdQueryHandler(
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository)
    {
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<FamilyDetailsResponse>> Handle(GetFamilyDetailsByIdQuery request, CancellationToken cancellationToken)
    {
        Family family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure<FamilyDetailsResponse>(FamilyErrors.NotFound(request.FamilyId));

        List<FamilyDetailsResponse.StudentResponse> students = new();

        foreach (StudentFamilyMembership member in family.Students)
        {
            Student student = await _studentRepository.GetById(member.StudentId, cancellationToken);

            if (student is null)
                continue;

            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            FamilyDetailsResponse.StudentResponse entry = new(
                student.Id,
                student.Name,
                student.EmailAddress,
                enrolment.SchoolCode,
                enrolment.SchoolName,
                enrolment.Grade);

            students.Add(entry);
        }

        List<FamilyDetailsResponse.ParentResponse> parents = new();

        foreach (Parent parent in family.Parents)
        {
            Result<Name> name = Name.Create(parent.FirstName, null, parent.LastName);
            Result<EmailAddress> email = EmailAddress.Create(parent.EmailAddress);
            Result<PhoneNumber> mobile = PhoneNumber.Create(parent.MobileNumber);

            FamilyDetailsResponse.ParentResponse entry = new(
                parent.Id,
                name.Value,
                email.Value,
                mobile.IsSuccess ? mobile.Value : null);

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
