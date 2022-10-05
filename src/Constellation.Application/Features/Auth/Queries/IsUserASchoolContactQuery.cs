namespace Constellation.Application.Features.Auth.Queries;

using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public class IsUserASchoolContactQuery : IRequest<bool>
{
    public string EmailAddress { get; set; }
}

public class IsUserASchoolContactQueryHandler : IRequestHandler<IsUserASchoolContactQuery, bool>
{
    private readonly IAppDbContext _context;

    public IsUserASchoolContactQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(IsUserASchoolContactQuery request, CancellationToken cancellationToken)
    {
        return await _context.SchoolContacts
            .AnyAsync(contact => contact.EmailAddress == request.EmailAddress && !contact.IsDeleted);
    }
}