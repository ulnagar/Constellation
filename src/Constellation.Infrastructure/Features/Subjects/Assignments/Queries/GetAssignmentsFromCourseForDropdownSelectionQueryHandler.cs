namespace Constellation.Infrastructure.Features.Subjects.Assignments.Queries;

using AutoMapper;
using Constellation.Application.Common.ValidationRules;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Subject.Assignments.Models;
using Constellation.Application.Features.Subject.Assignments.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Assignments;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;

public class GetAssignmentsFromCourseForDropdownSelectionQueryHandler : IRequestHandler<GetAssignmentsFromCourseForDropdownSelectionQuery, ValidateableResponse<ICollection<AssignmentFromCourseForDropdownSelection>>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly AppDbContext _context;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IMapper _mapper;

    public GetAssignmentsFromCourseForDropdownSelectionQueryHandler(
        IOfferingRepository offeringRepository,
        AppDbContext context, 
        ICanvasGateway canvasGateway, 
        IMapper mapper)
    {
        _offeringRepository = offeringRepository;
        _context = context;
        _canvasGateway = canvasGateway;
        _mapper = mapper;
    }
    public async Task<ValidateableResponse<ICollection<AssignmentFromCourseForDropdownSelection>>> Handle(GetAssignmentsFromCourseForDropdownSelectionQuery request, CancellationToken cancellationToken)
    {
        var offerings = await _offeringRepository.GetActiveByCourseId(request.CourseId, cancellationToken);

        var offering = offerings.FirstOrDefault();

        if (offering == null)
            return new ValidateableResponse<ICollection<AssignmentFromCourseForDropdownSelection>>(new List<AssignmentFromCourseForDropdownSelection>(), new List<string> { "Could not find a valid offering for this course!" });

        var canvasCourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 1)}";

        var assignments = await _canvasGateway.GetAllCourseAssignments(canvasCourseId);

        var assignmentDtos = _mapper.Map<ICollection<CanvasAssignmentDto>, ICollection<AssignmentFromCourseForDropdownSelection>>(assignments);

        foreach (var assignment in assignmentDtos)
            if (await _context.Set<CanvasAssignment>().AnyAsync(a => a.CanvasId == assignment.CanvasId))
                assignment.ExistsInDatabase = true;

        return new ValidateableResponse<ICollection<AssignmentFromCourseForDropdownSelection>>(assignmentDtos, null);
    }
}
