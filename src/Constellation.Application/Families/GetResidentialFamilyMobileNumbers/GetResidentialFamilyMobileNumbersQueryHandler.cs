namespace Constellation.Application.Families.GetResidentialFamilyMobileNumbers;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
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
    private readonly ISentralGatewayConfiguration _settings;

    public GetResidentialFamilyMobileNumbersQueryHandler(
        IFamilyRepository familyRepository,
        Serilog.ILogger logger,
        ISentralGatewayConfiguration settings = null)
    {
        _familyRepository = familyRepository;
        _logger = logger.ForContext<GetResidentialFamilyMobileNumbersQuery>();
        _settings = settings;
    }

    public async Task<Result<List<PhoneNumber>>> Handle(GetResidentialFamilyMobileNumbersQuery request, CancellationToken cancellationToken)
    {
        var phoneNumbers = new List<PhoneNumber>();

        var studentFamilies = await _familyRepository.GetFamiliesByStudentId(request.StudentId, cancellationToken);

        if (!studentFamilies.Any())
        {
            _logger.Warning("Could not find any families associated with student id {id}.", request.StudentId);

            return Result.Failure<List<PhoneNumber>>(DomainErrors.Family.Students.NoLinkedFamilies);
        }

        var residentialFamily = studentFamilies.FirstOrDefault(family =>
            family.Students.Any(student =>
                student.StudentId == request.StudentId &&
                student.IsResidentialFamily));

        if (residentialFamily is null)
        {
            _logger.Warning("Could not find a residential family associated with student id {id}.", request.StudentId);

            return Result.Failure<List<PhoneNumber>>(DomainErrors.Family.Students.NoResidentialFamily);
        }

        var mother = residentialFamily
            .Parents
            .FirstOrDefault(parent =>
                parent.SentralLink == Core.Models.Families.Parent.SentralReference.Mother);

        var motherMobileRequest = PhoneNumber.Create(mother.MobileNumber);

        if (motherMobileRequest.IsFailure)
        {
            _logger.Warning("Parent contact mobile is invalid: {@parent}", mother);
        }

        var father = residentialFamily
            .Parents
            .FirstOrDefault(parent =>
                parent.SentralLink == Core.Models.Families.Parent.SentralReference.Father);

        var fatherMobileRequest = PhoneNumber.Create(father.MobileNumber);

        if (fatherMobileRequest.IsFailure)
        {
            _logger.Warning("Parent contact mobile is invalid: {@parent}", father);
        }

        switch (_settings?.ContactPreference)
        {
            case ISentralGatewayConfiguration.ContactPreferenceOptions.MotherFirstThenFather:
                if (motherMobileRequest.IsSuccess)
                    phoneNumbers.Add(motherMobileRequest.Value);
                else
                    if (fatherMobileRequest.IsSuccess)
                        phoneNumbers.Add(fatherMobileRequest.Value);

                break;
            case ISentralGatewayConfiguration.ContactPreferenceOptions.FatherFirstThenMother:
                if (fatherMobileRequest.IsSuccess)
                    phoneNumbers.Add(fatherMobileRequest.Value);
                else
                    if (motherMobileRequest.IsSuccess)
                        phoneNumbers.Add(motherMobileRequest.Value);

                break;
            case ISentralGatewayConfiguration.ContactPreferenceOptions.BothParentsIfPresent:
            default:
                if (motherMobileRequest.IsSuccess)
                    phoneNumbers.Add(motherMobileRequest.Value);

                if (fatherMobileRequest.IsSuccess)
                    phoneNumbers.Add(fatherMobileRequest.Value);

                break;
        }

        return phoneNumbers.Distinct().ToList();
    }
}
