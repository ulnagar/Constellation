namespace Constellation.Infrastructure.Services;

using Application.Extensions;
using Constellation.Core.Models.EmergencyConsole.Enums;
using Core.Abstractions.Repositories;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class EmergencyRecipientService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;

    public EmergencyRecipientService(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IStaffRepository staffRepository,
        ISchoolContactRepository schoolContactRepository)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _staffRepository = staffRepository;
        _schoolContactRepository = schoolContactRepository;
    }

    public async Task<List<EmailRecipient>> GetSelectedEmailRecipientsFromGroup(
        RecipientGroup group,
        CancellationToken cancellationToken = default)
    {
        List<EmailRecipient> recipients = [];

        if (group == RecipientGroup.AllStaff)
        {
            List<StaffMember> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

            foreach (StaffMember member in staffMembers)
            {
                Result<EmailRecipient> recipient = member.GetEmailRecipient();

                if (recipient.IsFailure)
                {
                    //TODO: Handle error properly!
                    continue;
                }

                recipients.Add(recipient.Value);
            }
        }

        if (group == RecipientGroup.AllExecStaff)
        {
            List<StaffMember> staffMembers = [];

            foreach (StaffMember member in staffMembers)
            {
                Result<EmailRecipient> recipient = member.GetEmailRecipient();

                if (recipient.IsFailure)
                {
                    //TODO: Handle error properly!
                    continue;
                }

                recipients.Add(recipient.Value);
            }
        }
    }
}
