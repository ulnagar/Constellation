namespace Constellation.Application.Domains.Families.Queries.GetResidentialFamilyMobileNumbers;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Core.Models.Families;
using Core.Models.Families.Errors;
using Core.Shared;
using Core.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetResidentialFamilyMobileNumbersQueryHandler 
    : IQueryHandler<GetResidentialFamilyMobileNumbersQuery, List<PhoneNumber>>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly ILogger _logger;

    public GetResidentialFamilyMobileNumbersQueryHandler(
        IFamilyRepository familyRepository,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _logger = logger.ForContext<GetResidentialFamilyMobileNumbersQuery>();
    }

    public async Task<Result<List<PhoneNumber>>> Handle(GetResidentialFamilyMobileNumbersQuery request, CancellationToken cancellationToken)
    {
        List<PhoneNumber> phoneNumbers = [];

        List<Family> studentFamilies = await _familyRepository.GetFamiliesByStudentId(request.StudentId, cancellationToken);

        if (studentFamilies.Count == 0)
        {
            _logger.Warning("Could not find any families associated with student id {id}.", request.StudentId);

            return Result.Failure<List<PhoneNumber>>(FamilyStudentErrors.NoLinkedFamilies);
        }

        Family? residentialFamily = studentFamilies.FirstOrDefault(family =>
            family.Students.Any(student =>
                student.StudentId == request.StudentId &&
                student.IsResidentialFamily));

        if (residentialFamily is null)
        {
            _logger.Warning("Could not find a residential family associated with student id {id}.", request.StudentId);

            return Result.Failure<List<PhoneNumber>>(FamilyStudentErrors.NoResidentialFamily);
        }

        Parent? mother = residentialFamily
            .Parents
            .FirstOrDefault(parent =>
                parent.SentralLink == Parent.SentralReference.Mother);

        Result<PhoneNumber> motherMobile;

        if (mother is null || mother.MobileNumber == PhoneNumber.Empty) 
            motherMobile = Result.Failure<PhoneNumber>(Error.NullValue);
        else
            motherMobile = mother.MobileNumber;

        Parent? father = residentialFamily
            .Parents
            .FirstOrDefault(parent =>
                parent.SentralLink == Parent.SentralReference.Father);

        Result<PhoneNumber> fatherMobile;

        if (father is null || father.MobileNumber == PhoneNumber.Empty)
            fatherMobile = Result.Failure<PhoneNumber>(Error.NullValue);
        else
            fatherMobile = father.MobileNumber;
        
        if (motherMobile.IsSuccess)
            phoneNumbers.Add(motherMobile.Value);

        if (fatherMobile.IsSuccess)
            phoneNumbers.Add(fatherMobile.Value);

        return phoneNumbers.Distinct().ToList();
    }
}
