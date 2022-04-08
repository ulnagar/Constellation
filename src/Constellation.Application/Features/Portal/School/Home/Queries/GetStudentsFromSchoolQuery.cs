using Constellation.Application.DTOs;
using FluentValidation;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Home.Queries
{
    public class GetStudentsFromSchoolQuery : IRequest<ICollection<StudentDto>>
    {
        public string SchoolCode { get; set; }
    }

    public class GetStudentsFromSchoolQueryValidator : AbstractValidator<GetStudentsFromSchoolQuery>
    {
        public GetStudentsFromSchoolQueryValidator()
        {
            RuleFor(query => query.SchoolCode).NotEmpty();
        }
    }
}
