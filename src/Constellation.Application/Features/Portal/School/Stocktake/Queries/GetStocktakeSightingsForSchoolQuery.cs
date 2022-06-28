using Constellation.Application.Features.Portal.School.Stocktake.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Stocktake.Queries
{
    public class GetStocktakeSightingsForSchoolQuery : IRequest<ICollection<StocktakeSightingsForList>>
    {
        public string SchoolCode { get; set; }
        public Guid StocktakeEvent { get; set; }
    }
}
