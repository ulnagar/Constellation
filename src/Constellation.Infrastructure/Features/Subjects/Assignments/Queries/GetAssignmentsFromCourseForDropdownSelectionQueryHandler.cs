using AutoMapper;
using Constellation.Application.Common.ValidationRules;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Subject.Assignments.Models;
using Constellation.Application.Features.Subject.Assignments.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Subjects.Assignments.Queries
{
    public class GetAssignmentsFromCourseForDropdownSelectionQueryHandler : IRequestHandler<GetAssignmentsFromCourseForDropdownSelectionQuery, ValidateableResponse<ICollection<AssignmentFromCourseForDropdownSelection>>>
    {
        private readonly IAppDbContext _context;
        private readonly ICanvasGateway _canvasGateway;
        private readonly IMapper _mapper;

        public GetAssignmentsFromCourseForDropdownSelectionQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public GetAssignmentsFromCourseForDropdownSelectionQueryHandler(IAppDbContext context, ICanvasGateway canvasGateway, IMapper mapper)
        {
            _context = context;
            _canvasGateway = canvasGateway;
            _mapper = mapper;
        }
        public async Task<ValidateableResponse<ICollection<AssignmentFromCourseForDropdownSelection>>> Handle(GetAssignmentsFromCourseForDropdownSelectionQuery request, CancellationToken cancellationToken)
        {
            if (_canvasGateway is null)
                return new ValidateableResponse<ICollection<AssignmentFromCourseForDropdownSelection>>(new List<AssignmentFromCourseForDropdownSelection>(), new List<string> { "Canvas Gateway is not added to this application" });

            var offering = await _context.Offerings
                .FirstOrDefaultAsync(offering => offering.CourseId == request.CourseId && offering.EndDate >= DateTime.Now, cancellationToken);

            if (offering == null)
                return new ValidateableResponse<ICollection<AssignmentFromCourseForDropdownSelection>>(new List<AssignmentFromCourseForDropdownSelection>(), new List<string> { "Could not find a valid offering for this course!" });

            var canvasCourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 1)}";

            var assignments = await _canvasGateway.GetAllCourseAssignments(canvasCourseId);

            var assignmentDtos = _mapper.Map<ICollection<CanvasAssignmentDto>, ICollection<AssignmentFromCourseForDropdownSelection>>(assignments);

            foreach (var assignment in assignmentDtos)
                if (await _context.CanvasAssignments.AnyAsync(a => a.CanvasId == assignment.CanvasId))
                    assignment.ExistsInDatabase = true;

            return new ValidateableResponse<ICollection<AssignmentFromCourseForDropdownSelection>>(assignmentDtos, null);
        }
    }
}
