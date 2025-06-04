namespace Constellation.Application.Domains.Training.Queries.GetListOfCertificatesForStaffMemberWithNotCompletedModules;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Models;

public sealed record GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery(
    StaffId StaffId)
    : IQuery<StaffCompletionListDto>;
