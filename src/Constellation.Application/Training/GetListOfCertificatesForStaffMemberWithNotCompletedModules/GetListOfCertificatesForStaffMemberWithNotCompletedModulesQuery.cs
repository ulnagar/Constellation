namespace Constellation.Application.Training.GetListOfCertificatesForStaffMemberWithNotCompletedModules;

using Constellation.Application.Abstractions.Messaging;
using Models;

public sealed record GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery(
    string StaffId)
    : IQuery<StaffCompletionListDto>;
