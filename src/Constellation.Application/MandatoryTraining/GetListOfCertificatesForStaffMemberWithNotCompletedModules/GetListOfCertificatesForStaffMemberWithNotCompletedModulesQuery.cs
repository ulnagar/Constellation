﻿namespace Constellation.Application.MandatoryTraining.GetListOfCertificatesForStaffMemberWithNotCompletedModules;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.MandatoryTraining.Models;

public sealed record GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery(
    string StaffId) 
    : IQuery<StaffCompletionListDto>;