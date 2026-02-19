namespace Constellation.Application.Domains.Families.Queries.GetResidentialFamilyEmailAddresses;

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

public class GetResidentialFamilyEmailAddressesQueryHandler
    : IQueryHandler<GetResidentialFamilyEmailAddressesQuery, List<EmailRecipient>>
{
    private readonly IFamilyRepository _studentFamilyRepository;
    private readonly ILogger _logger;

    public GetResidentialFamilyEmailAddressesQueryHandler(
        IFamilyRepository studentFamilyRepository,
        ILogger logger)
    {
        _studentFamilyRepository = studentFamilyRepository;
        _logger = logger
            .ForContext<GetResidentialFamilyEmailAddressesQuery>();
    }

    public async Task<Result<List<EmailRecipient>>> Handle(GetResidentialFamilyEmailAddressesQuery request, CancellationToken cancellationToken)
    {
        List<EmailRecipient> emailAddresses = new();

        List<Family> studentFamilies = await _studentFamilyRepository.GetFamiliesByStudentId(request.StudentId, cancellationToken);

        if (studentFamilies.Count == 0)
        {
            _logger.Warning("Could not find any families associated with student id {id}.", request.StudentId);

            return Result.Failure<List<EmailRecipient>>(FamilyStudentErrors.NoLinkedFamilies);
        }

        Family? residentialFamily = studentFamilies.FirstOrDefault(family =>
            family.Students.Any(student =>
                student.StudentId == request.StudentId &&
                student.IsResidentialFamily));

        if (residentialFamily is null)
        {
            _logger.Warning("Could not find a residential family associated with student id {id}.", request.StudentId);

            return Result.Failure<List<EmailRecipient>>(FamilyStudentErrors.NoResidentialFamily);
        }

        Parent? mother = residentialFamily
            .Parents
            .FirstOrDefault(parent =>
                parent.SentralLink == Parent.SentralReference.Mother);

        Result<EmailRecipient> motherEmail;

        if (mother is null)
        {
            motherEmail = Result.Failure<EmailRecipient>(Error.NullValue);
        }
        else
        {
            motherEmail = EmailRecipient.Create(mother.Name, mother.EmailAddress);

            if (motherEmail.IsFailure)
            {
                _logger.Warning("Parent contact email is invalid: {@parent}", mother);
            }
        }

        Parent? father = residentialFamily
            .Parents
            .FirstOrDefault(parent =>
                parent.SentralLink == Parent.SentralReference.Father);

        Result<EmailRecipient> fatherEmail;

        if (father is null)
        {
            fatherEmail = Result.Failure<EmailRecipient>(Error.NullValue);
        }
        else
        {
            fatherEmail = EmailRecipient.Create(father.Name, father.EmailAddress);

            if (fatherEmail.IsFailure)
            {
                _logger.Warning("Parent contact email is invalid: {@parent}", father);
            }
        }

        if (motherEmail.IsSuccess)
            emailAddresses.Add(motherEmail.Value);
        
        if (fatherEmail.IsSuccess)
            emailAddresses.Add(fatherEmail.Value);

        return emailAddresses.Distinct().ToList();
    }
}
