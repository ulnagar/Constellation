using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Portal.School.Assignment.Queries
{
    public class GetStoredFileByIdQuery : IRequest<StoredFile>
    {
        public int Id { get; set; }
    }

    public class GetStoredFileByIdQueryHandler : IRequestHandler<GetStoredFileByIdQuery, StoredFile>
    {
        private readonly IAppDbContext _context;

        public GetStoredFileByIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<StoredFile> Handle(GetStoredFileByIdQuery request, CancellationToken cancellationToken)
        {
            var file = await _context.StoredFiles.SingleOrDefaultAsync(file => file.Id == request.Id, cancellationToken);

            return file;
        }
    }
}
