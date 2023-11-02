namespace Constellation.Application.Features.Partners.Students.Queries;

using AutoMapper;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Students;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetStudentsOfParentQuery : IRequest<ICollection<StudentOfParent>>
{
    public string ParentEmail { get; set; }
}

public class StudentOfParent : IMapFrom<Student>
{
    public string StudentId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName => $"{FirstName} {LastName}";
    public string CurrentGrade { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Student, StudentOfParent>()
            .ForMember(d => d.CurrentGrade, opt => opt.MapFrom(s => s.CurrentGrade.AsName()));
    }
}

public class GetStudentsOfParentQueryHandler : IRequestHandler<GetStudentsOfParentQuery, ICollection<StudentOfParent>>
{
    private readonly IMapper _mapper;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;

    public GetStudentsOfParentQueryHandler(
        IMapper mapper,
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository)
    {
        _mapper = mapper;
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
    }

    public async Task<ICollection<StudentOfParent>> Handle(GetStudentsOfParentQuery request, CancellationToken cancellationToken)
    {
        var studentIds = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        var students = await _studentRepository.GetListFromIds(studentIds.Keys.ToList(), cancellationToken);

        return _mapper
            .Map<List<StudentOfParent>>(students);
    }
}
