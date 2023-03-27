namespace Constellation.Application.Families.GetResidentialFamilyEmailAddresses;

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

public class GetResidentialFamilyEmailAddressesQueryHandler
    : IQueryHandler<GetResidentialFamilyEmailAddressesQuery, List<EmailRecipient>>
{
    private readonly IFamilyRepository _studentFamilyRepository;
    private readonly ILogger _logger;
    private readonly ISentralGatewayConfiguration _settings;

    public GetResidentialFamilyEmailAddressesQueryHandler(
        IFamilyRepository studentFamilyRepository,
        Serilog.ILogger logger,
        ISentralGatewayConfiguration settings = null)
    {
        _studentFamilyRepository = studentFamilyRepository;
        _logger = logger.ForContext<GetResidentialFamilyEmailAddressesQuery>();
        _settings = settings;
    }

    public async Task<Result<List<EmailRecipient>>> Handle(GetResidentialFamilyEmailAddressesQuery request, CancellationToken cancellationToken)
    {
        var emailAddresses = new List<EmailRecipient>();

        var studentFamilies = await _studentFamilyRepository.GetFamiliesByStudentId(request.StudentId, cancellationToken);

        if (!studentFamilies.Any())
        {
            _logger.Warning("Could not find any families associated with student id {id}.", request.StudentId);

            return Result.Failure<List<EmailRecipient>>(DomainErrors.Families.Students.NoLinkedFamilies);
        }

        var residentialFamily = studentFamilies.FirstOrDefault(family =>
            family.Students.Any(student =>
                student.StudentId == request.StudentId &&
                student.IsResidentialFamily));

        if (residentialFamily is null)
        {
            _logger.Warning("Could not find a residential family associated with student id {id}.", request.StudentId);

            return Result.Failure<List<EmailRecipient>>(DomainErrors.Families.Students.NoResidentialFamily);
        }

        var mother = residentialFamily
            .Parents
            .FirstOrDefault(parent =>
                parent.SentralLink == Core.Models.Families.Parent.SentralReference.Mother);

        var motherEmailRequest = EmailRecipient.Create($"{mother.FirstName} {mother.LastName}", mother.EmailAddress);

        if (motherEmailRequest.IsFailure)
        {
            _logger.Warning("Parent contact email is invalid: {@parent}", mother);
        }

        var father = residentialFamily
            .Parents
            .FirstOrDefault(parent =>
                parent.SentralLink == Core.Models.Families.Parent.SentralReference.Father);

        var fatherEmailRequest = EmailRecipient.Create($"{father.FirstName} {father.LastName}", father.EmailAddress);
        
        if (fatherEmailRequest.IsFailure)
        {
            _logger.Warning("Parent contact email is invalid: {@parent}", father);
        }

        switch (_settings?.ContactPreference)
        {
            case ISentralGatewayConfiguration.ContactPreferenceOptions.MotherFirstThenFather:
                if (motherEmailRequest.IsSuccess)
                    emailAddresses.Add(motherEmailRequest.Value);
                else
                    if (fatherEmailRequest.IsSuccess)
                    emailAddresses.Add(fatherEmailRequest.Value);

                break;
            case ISentralGatewayConfiguration.ContactPreferenceOptions.FatherFirstThenMother:
                if (fatherEmailRequest.IsSuccess)
                    emailAddresses.Add(fatherEmailRequest.Value);
                else
                    if (motherEmailRequest.IsSuccess)
                        emailAddresses.Add(motherEmailRequest.Value);

                break;
            case ISentralGatewayConfiguration.ContactPreferenceOptions.BothParentsIfPresent:
            default:
                if (motherEmailRequest.IsSuccess)
                    emailAddresses.Add(motherEmailRequest.Value);
                
                if (fatherEmailRequest.IsSuccess)
                    emailAddresses.Add(fatherEmailRequest.Value);

                break;
        }

        return emailAddresses.Distinct().ToList();
    }
}
