using Constellation.Application.Features.Awards.Models;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;

namespace Constellation.Application.Features.Awards.Queries
{
    public class GetRecentAwardsListQuery : IRequest<ICollection<AwardWithStudentName>>
    {
        public int RecentCount { get; set; }
        public DateOnly SinceDate { get; set; } = new DateOnly();
    }

    public class GetRecentAwardsListQueryValidator : AbstractValidator<GetRecentAwardsListQuery>
    {
        public GetRecentAwardsListQueryValidator()
        {
            RuleFor(query => query.RecentCount).NotEqual(0).When(query => query.SinceDate == new DateOnly());
            RuleFor(query => query.SinceDate).NotEqual(new DateOnly()).When(query => query.RecentCount == 0);
        }
    }
}
