namespace Constellation.Application.Domains.Auth.Queries.GetParentUserFromMobileNumber;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models.Families;
using Core.Models.Families.Errors;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Models.Identity;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetParentUserFromMobileNumberQueryHandler
: IQueryHandler<GetParentUserFromMobileNumberQuery, AppUser>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public GetParentUserFromMobileNumberQueryHandler(
        IFamilyRepository familyRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _userManager = userManager;
        _logger = logger
            .ForContext<GetParentUserFromMobileNumberQuery>();
    }

    public async Task<Result<AppUser>> Handle(GetParentUserFromMobileNumberQuery request, CancellationToken cancellationToken)
    {
        List<Family> families = await _familyRepository.GetFamilyByMobileNumber(request.PhoneNumber, cancellationToken);

        if (families.Count == 0)
        {
            _logger
                .ForContext(nameof(GetParentUserFromMobileNumberQuery), request, true)
                .ForContext(nameof(Error), FamilyErrors.NotFoundFromPhoneNumber(request.PhoneNumber), true)
                .Warning("Failed to retrieve family from mobile number {number}", request.PhoneNumber);

            return Result.Failure<AppUser>(FamilyErrors.NotFoundFromPhoneNumber(request.PhoneNumber));
        }

        List<Parent> parents = families
            .SelectMany(family => family.Parents)
            .Where(parent => parent.MobileNumber == request.PhoneNumber.ToString(PhoneNumber.Format.None))
            .ToList();

        switch (parents.Count)
        {
            case 0:
                _logger
                    .ForContext(nameof(GetParentUserFromMobileNumberQuery), request, true)
                    .ForContext(nameof(Error), ParentErrors.NotFoundWithNumber(request.PhoneNumber), true)
                    .Warning("Failed to retrieve parent from mobile number {number}", request.PhoneNumber);

                return Result.Failure<AppUser>(ParentErrors.NotFoundWithNumber(request.PhoneNumber));
            case 1:
                AppUser singleUser = await _userManager.FindByEmailAsync(parents.First().EmailAddress);

                return singleUser;
            case > 1:
                List<string> emails = parents.Select(parent => parent.EmailAddress).Distinct().ToList();

                if (emails.Count > 1)
                {
                    _logger
                        .ForContext(nameof(GetParentUserFromMobileNumberQuery), request, true)
                        .ForContext(nameof(Error), ParentErrors.TooManyWithSameNumber(request.PhoneNumber), true)
                        .Warning("Failed to retrieve parent from mobile number {number}", request.PhoneNumber);

                    return Result.Failure<AppUser>(ParentErrors.TooManyWithSameNumber(request.PhoneNumber));
                }

                AppUser singleEmail = await _userManager.FindByEmailAsync(emails.First());

                return singleEmail;
        }

        _logger
            .ForContext(nameof(GetParentUserFromMobileNumberQuery), request, true)
            .ForContext(nameof(Error), ApplicationErrors.UnknownError, true)
            .Warning("Failed to retrieve parent from mobile number {number}", request.PhoneNumber);

        return Result.Failure<AppUser>(ApplicationErrors.UnknownError);
    }
}
