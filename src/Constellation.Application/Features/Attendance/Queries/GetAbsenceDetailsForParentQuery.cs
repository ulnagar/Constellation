namespace Constellation.Application.Features.Attendance.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.Absences;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed record GetAbsenceDetailsForParentQuery(
    string ParentEmail,
    Guid AbsenceId)
    : IRequest<AbsenceDetailDto>;

public class AbsenceDetailDto : IMapFrom<Absence>
{
    public Guid Id { get; set; }
    public string StudentName => $"{StudentFirstName} {StudentLastName}";
    public string StudentFirstName { get; set; }
    public string StudentLastName { get; set; }
    public string StudentCurrentGrade { get; set; }
    public string Type { get; set; }
    public DateTime Date { get; set; }
    public string PeriodName { get; set; }
    public string PeriodTimeframe { get; set; }
    public int AbsenceLength { get; set; }
    public string AbsenceTimeframe { get; set; }
    public string AbsenceReason { get; set; }
    public string OfferingName { get; set; }
    public string Reason { get; set; }
    public string Validation { get; set; }
    public string ValidatedBy { get; set; }
    public bool Explained { get; set; }
    public bool CanBeExplainedByParent { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Absence, AbsenceDetailDto>()
            .ForMember(dest => dest.Reason, opt =>
            {
                opt.MapFrom(src => (string.IsNullOrWhiteSpace(src.ExternalExplanation)) ? src.Responses.First().Explanation : src.ExternalExplanation);
            })
            .ForMember(dest => dest.Validation, opt =>
            {
                opt.PreCondition(src => src.Type == Absence.Partial);
                opt.MapFrom(src => src.Responses.Max(response => response.VerificationStatus));
            })
            .ForMember(dest => dest.ValidatedBy, opt =>
            {
                opt.PreCondition(src => src.Type == Absence.Partial);
                opt.MapFrom(src => src.Responses.First(response => !string.IsNullOrWhiteSpace(response.Verifier)).Verifier);
            })
            .ForMember(dest => dest.Explained, opt =>
            {
                opt.MapFrom(src => src.ExternallyExplained ||
                    src.Responses.Any(response => response.Type == AbsenceResponse.Parent) ||
                    src.Responses.Any(response => response.Type == AbsenceResponse.System) ||
                    src.Responses.Any(response => response.Type == AbsenceResponse.Coordinator) ||
                    src.Responses.Any(response => response.Type == AbsenceResponse.Student && response.VerificationStatus == AbsenceResponse.Verified));
            })
            .ForMember(dest => dest.CanBeExplainedByParent, opt => opt.Ignore());
    }
}

public class GetAbsenceDetailsForParentQueryHandler : IRequestHandler<GetAbsenceDetailsForParentQuery, AbsenceDetailDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFamilyRepository _familyRepository;

    public GetAbsenceDetailsForParentQueryHandler(
        IAppDbContext context,
        IMapper mapper,
        IFamilyRepository familyRepository)
    {
        _context = context;
        _mapper = mapper;
        _familyRepository = familyRepository;
    }

    public async Task<AbsenceDetailDto> Handle(GetAbsenceDetailsForParentQuery request, CancellationToken cancellationToken)
    {
        var studentId = await _context
            .Students
            .Where(student =>
                student.Absences.Any(absence =>
                    absence.Id == request.AbsenceId))
            .Select(student => student.StudentId)
            .FirstOrDefaultAsync(cancellationToken);

        var studentIds = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        var record = studentIds.FirstOrDefault(entry => entry.Key == studentId);

        if (record.Key is null)
            return new AbsenceDetailDto();

        var absence = await _context.Absences
            .ProjectTo<AbsenceDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(src => src.Id == request.AbsenceId, cancellationToken);

        absence.CanBeExplainedByParent = record.Value;

        return absence;
    }
}
