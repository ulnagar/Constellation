namespace Constellation.Application.Training.Modules.GetListOfCertificatesForStaffMemberWithNotCompletedModules;

using Constellation.Application.Abstractions.Messaging;
using Models;

public sealed record GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery(
    string StaffId)
    : IQuery<StaffCompletionListDto>;
