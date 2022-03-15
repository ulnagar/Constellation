﻿using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Portal.School.Assignment.Queries
{
    public class GetListOfStoredFilesQuery : IRequest<ICollection<StoredFile>>
    {
    }

    public class GetListOfStoredFilesQueryHandler : IRequestHandler<GetListOfStoredFilesQuery, ICollection<StoredFile>>
    {
        private readonly IAppDbContext _context;

        public GetListOfStoredFilesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<StoredFile>> Handle(GetListOfStoredFilesQuery request, CancellationToken cancellationToken)
        {
            var files = await _context.StoredFiles.ToListAsync(cancellationToken);

            return files;
        }
    }
}
