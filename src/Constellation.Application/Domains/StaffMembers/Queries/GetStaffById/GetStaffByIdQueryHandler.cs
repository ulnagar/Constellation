﻿namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffById;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffByIdQueryHandler
    : IQueryHandler<GetStaffByIdQuery, StaffResponse>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ILogger _logger;

    public GetStaffByIdQueryHandler(
        IStaffRepository staffRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _logger = logger.ForContext<GetStaffByIdQuery>();
    }

    public async Task<Result<StaffResponse>> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        Staff staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger.Warning("Could not find Staff Member with Id {id}", request.StaffId);

            return Result.Failure<StaffResponse>(DomainErrors.Partners.Staff.NotFound(request.StaffId));
        }

        Result<Name> staffName = Name.Create(staffMember.FirstName, string.Empty, staffMember.LastName);

        if (staffName.IsFailure)
        {
            _logger.Warning(staffName.Error);

            return Result.Failure<StaffResponse>(staffName.Error);
        }

        Result<EmailAddress> staffEmail = EmailAddress.Create(staffMember.EmailAddress);

        if (staffEmail.IsFailure)
        {
            _logger.Warning(staffEmail.Error);

            return Result.Failure<StaffResponse>(staffEmail.Error);
        }

        StaffResponse response = new(
            staffMember.StaffId,
            staffName.Value,
            staffEmail.Value,
            staffMember.PortalUsername,
            staffMember.SchoolCode,
            staffMember.IsShared);

        return response;
    }
}
