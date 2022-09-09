﻿namespace Constellation.Application.Features.Attendance.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetAbsenceDetailsForParentQuery : IRequest<AbsenceDetailDto>
{
    public Guid AbsenceId { get; set; }
}

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
            });
    }
}

public class GetAbsenceDetailsForParentQueryHandler : IRequestHandler<GetAbsenceDetailsForParentQuery, AbsenceDetailDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetAbsenceDetailsForParentQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AbsenceDetailDto> Handle(GetAbsenceDetailsForParentQuery request, CancellationToken cancellationToken)
    {
        return await _context.Absences
            .ProjectTo<AbsenceDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(src => src.Id == request.AbsenceId, cancellationToken);
    }
}