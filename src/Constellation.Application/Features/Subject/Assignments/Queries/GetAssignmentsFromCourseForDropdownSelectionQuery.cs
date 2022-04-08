using Constellation.Application.Common.ValidationRules;
using Constellation.Application.Features.Subject.Assignments.Models;
using FluentValidation;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Subject.Assignments.Queries
{
    public class GetAssignmentsFromCourseForDropdownSelectionQuery : IRequest<ValidateableResponse<ICollection<AssignmentFromCourseForDropdownSelection>>>, IValidatable
    {
        public int CourseId { get; set; }
    }

    public class GetAssignmentsFromCourseForDropdownSelectionQueryValidator : AbstractValidator<GetAssignmentsFromCourseForDropdownSelectionQuery>
    {
        public GetAssignmentsFromCourseForDropdownSelectionQueryValidator()
        {
            RuleFor(request => request.CourseId).NotEmpty();
        }
    }
}
