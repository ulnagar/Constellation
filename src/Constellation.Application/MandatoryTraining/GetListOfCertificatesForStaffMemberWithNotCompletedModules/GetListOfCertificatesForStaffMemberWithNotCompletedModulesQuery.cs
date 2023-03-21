namespace Constellation.Application.MandatoryTraining.GetListOfCertificatesForStaffMemberWithNotCompletedModules;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Features.MandatoryTraining.Models;

public sealed record GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery(
    string StaffId) 
    : IQuery<StaffCompletionListDto>;
