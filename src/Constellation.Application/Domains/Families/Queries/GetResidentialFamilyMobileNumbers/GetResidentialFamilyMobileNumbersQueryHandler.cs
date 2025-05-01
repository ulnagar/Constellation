namespace Constellation.Application.Domains.Families.Queries.GetResidentialFamilyMobileNumbers;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Core.Errors;
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
        var phoneNumbers = new List<PhoneNumber>();

        var studentFamilies = await _familyRepository.GetFamiliesByStudentId(request.StudentId, cancellationToken);

        if (!studentFamilies.Any())
        {
            _logger.Warning("Could not find any families associated with student id {id}.", request.StudentId);

            return Result.Failure<List<PhoneNumber>>(FamilyStudentErrors.NoLinkedFamilies);
        }

        var residentialFamily = studentFamilies.FirstOrDefault(family =>
            family.Students.Any(student =>
                student.StudentId == request.StudentId &&
                student.IsResidentialFamily));

        if (residentialFamily is null)
        {
            _logger.Warning("Could not find a residential family associated with student id {id}.", request.StudentId);

            return Result.Failure<List<PhoneNumber>>(FamilyStudentErrors.NoResidentialFamily);
        }

        var mother = residentialFamily
            .Parents
            .FirstOrDefault(parent =>
                parent.SentralLink == Core.Models.Families.Parent.SentralReference.Mother);

        Result<PhoneNumber> motherMobile;

        if (mother is null) 
        {
            motherMobile = Result.Failure<PhoneNumber>(Error.NullValue);
        }
        else
        {
            var motherMobileRequest = PhoneNumber.Create(mother.MobileNumber);

            if (motherMobileRequest.IsFailure)
            {
                _logger.Warning("Parent contact mobile is invalid: {@parent}", mother);
            }
                
            motherMobile = motherMobileRequest;
        }

        var father = residentialFamily
            .Parents
            .FirstOrDefault(parent =>
                parent.SentralLink == Core.Models.Families.Parent.SentralReference.Father);

        Result<PhoneNumber> fatherMobile;

        if (father is null)
        {
            fatherMobile = Result.Failure<PhoneNumber>(Error.NullValue);
        }
        else
        {
            var fatherMobileRequest = PhoneNumber.Create(father.MobileNumber);

            if (fatherMobileRequest.IsFailure)
            {
                _logger.Warning("Parent contact mobile is invalid: {@parent}", father);
            }
 
            fatherMobile = fatherMobileRequest;
        }

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
