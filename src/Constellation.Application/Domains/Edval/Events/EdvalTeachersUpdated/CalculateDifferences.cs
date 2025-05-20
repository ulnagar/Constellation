namespace Constellation.Application.Domains.Edval.Events.EdvalTeachersUpdated;

using Abstractions.Messaging;
using Constellation.Core.Models.Edval.Enums;
using Core.Models;
using Core.Models.Edval;
using Core.Models.Edval.Events;
using Core.Models.StaffMembers.Repositories;
using Interfaces.Repositories;
using Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CalculateDifferences : IIntegrationEventHandler<EdvalTeachersUpdatedIntegrationEvent>
{
    private readonly IEdvalRepository _edvalRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CalculateDifferences(
        IEdvalRepository edvalRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _edvalRepository = edvalRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<EdvalTeachersUpdatedIntegrationEvent>();
    }
    public async Task Handle(EdvalTeachersUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        List<Staff> existingStaff = await _staffRepository.GetAllActive(cancellationToken);

        List<EdvalTeacher> edvalTeachers = await _edvalRepository.GetTeachers(cancellationToken);
        List<EdvalIgnore> ignoredTeachers = await _edvalRepository.GetIgnoreRecords(EdvalDifferenceType.EdvalTeacher, cancellationToken);

        foreach (EdvalTeacher teacher in edvalTeachers)
        {
            bool ignored = ignoredTeachers
                .Where(ignore => ignore.System == EdvalDifferenceSystem.EdvalDifference)
                .Any(ignore => ignore.Identifier == teacher.UniqueId);

            Staff staffMember = existingStaff.FirstOrDefault(member =>
                member.FirstName.Trim().Equals(teacher.FirstName, StringComparison.OrdinalIgnoreCase) &&
                member.LastName.Trim().Equals(teacher.LastName, StringComparison.OrdinalIgnoreCase));

            if (staffMember is null)
            {
                // Additional staff member in Edval
                _edvalRepository.Insert(new Difference(
                    EdvalDifferenceType.EdvalTeacher,
                    EdvalDifferenceSystem.EdvalDifference,
                    teacher.UniqueId,
                    $"{teacher.FirstName} {teacher.LastName} is not present in Constellation",
                    ignored));

                continue;
            }

            if (!staffMember.EmailAddress.Equals(teacher.EmailAddress, StringComparison.OrdinalIgnoreCase))
            {
                _edvalRepository.Insert(new Difference(
                    EdvalDifferenceType.EdvalTeacher,
                    EdvalDifferenceSystem.EdvalDifference,
                    teacher.UniqueId,
                    $"{staffMember.FirstName} {staffMember.LastName} has a different Email Address ({teacher.EmailAddress}) in Edval",
                    ignored));
            }
        }

        foreach (Staff staffMember in existingStaff)
        {
            bool ignored = ignoredTeachers
                .Where(ignore => ignore.System == EdvalDifferenceSystem.ConstellationDifference)
                .Any(ignore => ignore.Identifier == staffMember.StaffId);

            if (!edvalTeachers.Any(teacher => 
                    teacher.FirstName.Equals(staffMember.FirstName.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    teacher.LastName.Equals(staffMember.LastName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                _edvalRepository.Insert(new Difference(
                    EdvalDifferenceType.EdvalTeacher,
                    EdvalDifferenceSystem.ConstellationDifference,
                    staffMember.StaffId,
                    $"{staffMember.FirstName} {staffMember.LastName} is not present in Edval",
                    ignored));
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}