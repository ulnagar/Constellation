namespace Constellation.Application.Features.Equipment.Stocktake.Queries;

using MediatR;
using System.Collections.Generic;
using StaffMembers.Models;

public class GetStaffForSelectionQuery : IRequest<ICollection<StaffSelectionListResponse>>
{
}