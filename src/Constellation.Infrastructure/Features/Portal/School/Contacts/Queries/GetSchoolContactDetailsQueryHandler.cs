using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.Contacts.Models;
using Constellation.Application.Features.Portal.School.Contacts.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Contacts.Queries
{
    public class GetSchoolContactDetailsQueryHandler : IRequestHandler<GetSchoolContactDetailsQuery, SchoolContactDetails>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetSchoolContactDetailsQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<SchoolContactDetails> Handle(GetSchoolContactDetailsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Schools
                .ProjectTo<SchoolContactDetails>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(school => school.Code == request.Code, cancellationToken);
        }
    }
}
