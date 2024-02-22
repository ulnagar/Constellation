namespace Constellation.Application.Features.Portal.School.Home.Queries;

using MediatR;
using System.Collections.Generic;

public sealed class GetAllPartnerSchoolCodesQuery 
    : IRequest<ICollection<string>>
{ }