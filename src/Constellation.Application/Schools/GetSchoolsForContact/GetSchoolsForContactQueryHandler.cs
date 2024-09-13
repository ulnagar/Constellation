namespace Constellation.Application.Schools.GetSchoolsForContact;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolsForContactQueryHandler
: IQueryHandler<GetSchoolsForContactQuery, List<SchoolResponse>>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetSchoolsForContactQueryHandler(
        ISchoolContactRepository contactRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetSchoolsForContactQuery>();
    }

    public async Task<Result<List<SchoolResponse>>> Handle(GetSchoolsForContactQuery request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin && request.ContactId == SchoolContactId.Empty)
        {
            _logger
                .ForContext(nameof(GetSchoolsForContactQuery), request, true)
                .ForContext(nameof(Error), ApplicationErrors.ArgumentNull(nameof(request.ContactId)), true)
                .Warning("Failed to retrieve list of schools for Contact");

            return Result.Failure<List<SchoolResponse>>(ApplicationErrors.ArgumentNull(nameof(request.ContactId)));
        }

        List<SchoolResponse> response = new();

        if (request.ContactId == SchoolContactId.Empty)
        {
            List<School> schools = await _schoolRepository.GetWithCurrentStudents(cancellationToken);

            foreach(School school in schools)
            {
                response.Add(new(
                    school.Code,
                    school.Name));
            }
        }
        else
        {
            SchoolContact contact = await _contactRepository.GetById(request.ContactId, cancellationToken);

            if (contact is null)
            {
                _logger
                    .ForContext(nameof(GetSchoolsForContactQuery), request, true)
                    .ForContext(nameof(Error), SchoolContactErrors.NotFound(request.ContactId), true)
                    .Warning("Failed to retrieve list of schools for Contact");

                return Result.Failure<List<SchoolResponse>>(SchoolContactErrors.NotFound(request.ContactId));
            }

            List<string> activeSchoolCodes = contact.Assignments
                .Where(entry => !entry.IsDeleted)
                .Select(entry => entry.SchoolCode)
                .ToList();

            List<School> schools = await _schoolRepository.GetListFromIds(activeSchoolCodes, cancellationToken);

            foreach (School school in schools)
            {
                response.Add(new(
                    school.Code,
                    school.Name));
            }
        }

        return response;
    }
}
