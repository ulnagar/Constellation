namespace Constellation.Application.SchoolContacts.DoesEmailBelongToSchoolContact;

using Abstractions.Messaging;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DoesEmailBelongToSchoolContactQueryHandler
: IQueryHandler<DoesEmailBelongToSchoolContactQuery, bool>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ILogger _logger;

    public DoesEmailBelongToSchoolContactQueryHandler(
        ISchoolContactRepository contactRepository,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _logger = logger.ForContext<DoesEmailBelongToSchoolContactQuery>();
    }

    public async Task<Result<bool>> Handle(DoesEmailBelongToSchoolContactQuery request, CancellationToken cancellationToken)
    {
        SchoolContact? response = await _contactRepository.GetWithRolesByEmailAddress(request.EmailAddress, cancellationToken);

        if (response is null)
            return false;

        if (response.Assignments.Count(entry => !entry.IsDeleted) > 0)
            return true;

        return false;
    }
}
