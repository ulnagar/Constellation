namespace Constellation.Application.Domains.Families.Queries.GetResidentialFamilyMobileNumbers;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Core.Models.Families;
using Core.Models.Families.Errors;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Configuration;
using Microsoft.Extensions.Options;
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
    private readonly SentralGatewayConfiguration _settings;

    public GetResidentialFamilyMobileNumbersQueryHandler(
        IFamilyRepository familyRepository,
        ILogger logger,
        IOptions<SentralGatewayConfiguration> settings)
    {
        _familyRepository = familyRepository;
        _logger = logger.ForContext<GetResidentialFamilyMobileNumbersQuery>();
        _settings = settings.Value;
    }

    public async Task<Result<List<PhoneNumber>>> Handle(GetResidentialFamilyMobileNumbersQuery request, CancellationToken cancellationToken)
    {
        List<PhoneNumber> phoneNumbers = new();

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
                parent.SentralLink == Core.Models.Families.Parent.SentralReference.Mother);

        Result<PhoneNumber> motherMobile;

        if (mother is null || mother.MobileNumber == PhoneNumber.Empty) 
            motherMobile = Result.Failure<PhoneNumber>(Error.NullValue);
        else
            motherMobile = mother.MobileNumber;

        Parent? father = residentialFamily
            .Parents
            .FirstOrDefault(parent =>
                parent.SentralLink == Core.Models.Families.Parent.SentralReference.Father);

        Result<PhoneNumber> fatherMobile;

        if (father is null || father.MobileNumber == PhoneNumber.Empty)
            fatherMobile = Result.Failure<PhoneNumber>(Error.NullValue);
        else
            fatherMobile = father.MobileNumber;

        switch (_settings?.ContactPreference)
        {
            case SentralGatewayConfiguration.ContactPreferenceOptions.MotherThenFather:
                if (motherMobile.IsSuccess)
                    phoneNumbers.Add(motherMobile.Value);
                else
                    if (fatherMobile.IsSuccess)
                        phoneNumbers.Add(fatherMobile.Value);

                break;
            case SentralGatewayConfiguration.ContactPreferenceOptions.FatherThenMother:
                if (fatherMobile.IsSuccess)
                    phoneNumbers.Add(fatherMobile.Value);
                else
                    if (motherMobile.IsSuccess)
                        phoneNumbers.Add(motherMobile.Value);

                break;
            case SentralGatewayConfiguration.ContactPreferenceOptions.Both:
            default:
                if (motherMobile.IsSuccess)
                    phoneNumbers.Add(motherMobile.Value);

                if (fatherMobile.IsSuccess)
                    phoneNumbers.Add(fatherMobile.Value);

                break;
        }

        return phoneNumbers.Distinct().ToList();
    }
}
