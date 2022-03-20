using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Admin.AdobeConnectOperations.Queries
{
    public class GetStudentsWithoutAdobeConnectPrincipalIdQuery : IRequest<ICollection<StudentWithoutAdobeConnectPrincipalId>>
    {
    }

    public class StudentWithoutAdobeConnectPrincipalId : IMapFrom<Student>
    {
        public string StudentId { get; set; }
        public string PortalUsername { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName => $"{FirstName} {LastName}";
    }

    public class GetStudentsWithoutAdobeConnectPrincipalIdQueryHandler : IRequestHandler<GetStudentsWithoutAdobeConnectPrincipalIdQuery, ICollection<StudentWithoutAdobeConnectPrincipalId>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentsWithoutAdobeConnectPrincipalIdQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StudentWithoutAdobeConnectPrincipalId>> Handle(GetStudentsWithoutAdobeConnectPrincipalIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Students
                .Where(student => !student.IsDeleted && string.IsNullOrWhiteSpace(student.AdobeConnectPrincipalId))
                .ProjectTo<StudentWithoutAdobeConnectPrincipalId>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
