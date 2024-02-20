namespace Constellation.Application.Features.API.Schools.Queries;

using MediatR;
using System.Collections.Generic;

public sealed class GetSchoolCodeOfAllPartnerSchoolsQuery : IRequest<ICollection<string>>
{
}