using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Subject.Assignments.Queries
{
    public class GetAssignmentsFromCourseForDropdownSelectionQuery : IRequest<ICollection<AssignmentFromCourseForDropdownSelection>>
    {
        public int CourseId { get; set; }
    }

    public class AssignmentFromCourseForDropdownSelection : IMapFrom<CanvasAssignmentDto>
    {
        public string Name { get; set; }
        public int CanvasId { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? LockDate { get; set; }
        public DateTime? UnlockDate { get; set; }
        public int AllowedAttempts { get; set; }
        public bool ExistsInDatabase { get; set; }
    }

    public class GetAssignmentsFromCourseForDropdownSelectionQueryValidator : AbstractValidator<GetAssignmentsFromCourseForDropdownSelectionQuery>
    {
        public GetAssignmentsFromCourseForDropdownSelectionQueryValidator()
        {
            RuleFor(request => request.CourseId).NotEmpty();
        }
    }

    public class GetAssignmentsFromCourseForDropdownSelectionQueryHandler : IRequestHandler<GetAssignmentsFromCourseForDropdownSelectionQuery, ICollection<AssignmentFromCourseForDropdownSelection>>
    {
        private readonly IAppDbContext _context;
        private readonly ICanvasGateway _canvasGateway;
        private readonly IMapper _mapper;

        public GetAssignmentsFromCourseForDropdownSelectionQueryHandler(IAppDbContext context, ICanvasGateway canvasGateway, IMapper mapper)
        {
            _context = context;
            _canvasGateway = canvasGateway;
            _mapper = mapper;
        }
        public async Task<ICollection<AssignmentFromCourseForDropdownSelection>> Handle(GetAssignmentsFromCourseForDropdownSelectionQuery request, CancellationToken cancellationToken)
        {
            var offering = await _context.Offerings
                .FirstOrDefaultAsync(offering => offering.CourseId == request.CourseId && offering.EndDate >= DateTime.Now, cancellationToken);

            if (offering == null)
                return new List<AssignmentFromCourseForDropdownSelection>();

            var canvasCourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 1)}";

            var assignments = await _canvasGateway.GetAllCourseAssignments(canvasCourseId);

            var assignmentDtos = _mapper.Map<ICollection<CanvasAssignmentDto>, ICollection<AssignmentFromCourseForDropdownSelection>>(assignments);

            foreach (var assignment in assignmentDtos)
                if (await _context.CanvasAssignments.AnyAsync(a => a.CanvasId == assignment.CanvasId))
                    assignment.ExistsInDatabase = true;

            return assignmentDtos;
        }
    }
}
