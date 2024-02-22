namespace Constellation.Application.Features.Equipment.Stocktake.Queries;

using Constellation.Application.Features.Equipment.Stocktake.Models;
using MediatR;
using System.Collections.Generic;

public sealed class GetSchoolsForSelectionQuery
    : IRequest<ICollection<PartnerSchoolForDropdownSelection>>
{ }