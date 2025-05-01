namespace Constellation.Application.Domains.Training.Queries.GetListOfStaffMembersWithoutModule;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetListOfStaffMembersWithoutModuleQuery()
    : IQuery<List<StaffResponse>>;