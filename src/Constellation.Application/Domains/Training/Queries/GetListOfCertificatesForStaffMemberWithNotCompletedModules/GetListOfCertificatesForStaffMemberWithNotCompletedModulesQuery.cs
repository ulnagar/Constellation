namespace Constellation.Application.Domains.Training.Queries.GetListOfCertificatesForStaffMemberWithNotCompletedModules;

using Abstractions.Messaging;
using Models;

public sealed record GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery(
    string StaffId)
    : IQuery<StaffCompletionListDto>;
