namespace Constellation.Application.Features.Partners.Students.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetStudentsOfParentQuery : IRequest<ICollection<StudentOfParent>>
{
    public string ParentEmail { get; set; }
}

public class StudentOfParent : IMapFrom<Student>
{
    public string StudentId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName => $"{FirstName} {LastName}";
    public string CurrentGrade { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Student, StudentOfParent>()
            .ForMember(d => d.CurrentGrade, opt => opt.MapFrom(s => s.CurrentGrade.AsName()));
    }
}

public class GetStudentsOfParentQueryHandler : IRequestHandler<GetStudentsOfParentQuery, ICollection<StudentOfParent>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetStudentsOfParentQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ICollection<StudentOfParent>> Handle(GetStudentsOfParentQuery request, CancellationToken cancellationToken)
    {
        return await _context.Students
            .Where(student => student.Family.Parent1.EmailAddress == request.ParentEmail || student.Family.Parent2.EmailAddress == request.ParentEmail)
            .ProjectTo<StudentOfParent>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
