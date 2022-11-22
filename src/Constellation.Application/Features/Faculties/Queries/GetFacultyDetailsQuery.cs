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

public record GetFacultyDetailsQuery(Guid FacultyId) : IRequest<FacultyDetailsDto> { }

public class GetFacultyDetailsQueryHandler : IRequestHandler<GetFacultyDetailsQuery, FacultyDetailsDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetFacultyDetailsQueryHandler(IAppDbContext context, IMapper mapper)
	{
        _context = context;
        _mapper = mapper;
    }

    public async Task<FacultyDetailsDto> Handle(GetFacultyDetailsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Faculties
            .Where(faculty => faculty.Id == request.FacultyId)
            .ProjectTo<FacultyDetailsDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}