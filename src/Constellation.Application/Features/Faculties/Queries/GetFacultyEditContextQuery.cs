namespace Constellation.Application.Features.Faculties.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Faculties.Models;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetFacultyEditContextQuery(Guid FacultyId) : IRequest<FacultyEditContextDto> { }

public class GetFacultyEditContextQueryHandler : IRequestHandler<GetFacultyEditContextQuery, FacultyEditContextDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetFacultyEditContextQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<FacultyEditContextDto> Handle(GetFacultyEditContextQuery request, CancellationToken cancellationToken)
    {
        return await _context.Faculties
            .Where(faculty => faculty.Id == request.FacultyId)
            .ProjectTo<FacultyEditContextDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}