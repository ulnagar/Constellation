namespace Constellation.Application.Domains.Families.Queries.DoesEmailBelongToParentOrFamily;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DoesEmailBelongToParentOrFamilyQueryHandler
: IQueryHandler<DoesEmailBelongToParentOrFamilyQuery, bool>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly ILogger _logger;

    public DoesEmailBelongToParentOrFamilyQueryHandler(
        IFamilyRepository familyRepository,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _logger = logger.ForContext<DoesEmailBelongToParentOrFamilyQuery>();
    }

    public async Task<Result<bool>> Handle(DoesEmailBelongToParentOrFamilyQuery request, CancellationToken cancellationToken) => 
        await _familyRepository.DoesEmailBelongToParentOrFamily(request.EmailAddress, cancellationToken);
}
