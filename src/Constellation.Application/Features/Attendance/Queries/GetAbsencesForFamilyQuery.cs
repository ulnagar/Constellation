﻿namespace Constellation.Application.Features.Attendance.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetAbsencesForFamilyQuery : IRequest<IList<AbsenceDto>>
{
    public string ParentEmail { get; set; }
}

public class AbsenceDto : IMapFrom<Absence>
{
    public Guid Id { get; set; }
    public string StudentId { get; set; }
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
    public string Explanation { get; set; }
    public AbsenceStatus Status => GetAbsenceStatus();
    public bool Explained { get; set; }
    public bool CanParentExplainAbsence { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Absence, AbsenceDto>()
            .ForMember(dest => dest.Explanation, opt =>
            {
                //opt.PreCondition(src => src.Responses.Any(response => response.VerificationStatus == AbsenceResponse.Pending));
                //opt.MapFrom(src => src.Responses.First(response => response.VerificationStatus == AbsenceResponse.Pending).Explanation);
                opt.MapFrom(src => (string.IsNullOrWhiteSpace(src.ExternalExplanation)) ? src.Responses.First().Explanation : src.ExternalExplanation);
            })
            .ForMember(dest => dest.Explained, opt =>
            {
                opt.MapFrom(src => src.ExternallyExplained || 
                    src.Responses.Any(response => response.Type == AbsenceResponse.Parent) || 
                    src.Responses.Any(response => response.Type == AbsenceResponse.System) ||
                    src.Responses.Any(response => response.Type == AbsenceResponse.Coordinator) ||
                    src.Responses.Any(response => response.Type == AbsenceResponse.Student && response.VerificationStatus == AbsenceResponse.Verified));
            })
            .ForMember(dest => dest.CanParentExplainAbsence, opt => opt.Ignore());
    }

    public AbsenceStatus GetAbsenceStatus()
    {
        if (Type == Absence.Whole)
        {
            if (Explained)
                return AbsenceStatus.ExplainedWhole;
            else
                return AbsenceStatus.UnexplainedWhole;
        }

        //if (Type == Absence.Partial)
        if (Explained)
            return AbsenceStatus.VerifiedPartial;
        else if (!string.IsNullOrWhiteSpace(Explanation))
            return AbsenceStatus.UnverifiedPartial;
        else
            return AbsenceStatus.UnexplainedPartial;
    }

    public enum AbsenceStatus
    {
        UnexplainedPartial = 1,
        UnverifiedPartial = 2,
        UnexplainedWhole = 3,
        VerifiedPartial = 4,
        ExplainedWhole = 5
    }
}

public class GetAbsencesForFamilyQueryHandler : IRequestHandler<GetAbsencesForFamilyQuery, IList<AbsenceDto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFamilyRepository _familyRepository;

    public GetAbsencesForFamilyQueryHandler(
        IAppDbContext context,
        IMapper mapper,
        IFamilyRepository familyRepository)
    {
        _context = context;
        _mapper = mapper;
        _familyRepository = familyRepository;
    }

    public async Task<IList<AbsenceDto>> Handle(GetAbsencesForFamilyQuery request, CancellationToken cancellationToken)
    {
        var studentIds = await _familyRepository.GetStudentIdsFromFamilyWithEmail(request.ParentEmail, cancellationToken);

        var absences = await _context.Absences
            .Where(absence => studentIds.Keys.Contains(absence.StudentId) && absence.Date.Year == DateTime.Today.Year)
            .ProjectTo<AbsenceDto>(_mapper.ConfigurationProvider, cancellationToken)
            .ToListAsync(cancellationToken);

        foreach (var absence in absences)
        {
            var record = studentIds.First(entry => entry.Key == absence.StudentId);

            absence.CanParentExplainAbsence = record.Value;
        }

        return absences;
    }
}
