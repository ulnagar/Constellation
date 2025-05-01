namespace Constellation.Application.Domains.Families.Queries.IsResidentialParent;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class IsResidentialParentQueryHandler
    : IQueryHandler<IsResidentialParentQuery, bool>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;

    public IsResidentialParentQueryHandler(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
    }

    public async Task<Result<bool>> Handle(IsResidentialParentQuery request, CancellationToken cancellationToken)
    {
        Dictionary<StudentId, bool> studentIds = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        bool isResidential = studentIds.Any(entry => entry.Value);

        return isResidential;
    }
}
