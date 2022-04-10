using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.Absences.Models;
using Constellation.Application.Features.Portal.School.Absences.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Absences.Queries
{
    public class GetAbsenceResponseForVerificationQueryHandler : IRequestHandler<GetAbsenceResponseForVerificationQuery, PartialAbsenceResponseForVerification>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetAbsenceResponseForVerificationQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PartialAbsenceResponseForVerification> Handle(GetAbsenceResponseForVerificationQuery request, CancellationToken cancellationToken)
        {
            return await _context.AbsenceResponse
                .Where(response => response.Id == request.Id)
                .ProjectTo<PartialAbsenceResponseForVerification>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
