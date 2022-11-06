namespace Constellation.Application.Features.MandatoryTraining.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.MandatoryTraining.Models;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetUploadedTrainingCertificateFileByIdQuery : IRequest<CompletionRecordCertificateDetailsDto>
{
    public string LinkType { get; init; }
    public string LinkId { get; init; }
}

public class GetUploadedTrainingCertificateFileByIdQueryHandler : IRequestHandler<GetUploadedTrainingCertificateFileByIdQuery, CompletionRecordCertificateDetailsDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetUploadedTrainingCertificateFileByIdQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CompletionRecordCertificateDetailsDto> Handle(GetUploadedTrainingCertificateFileByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.StoredFiles
            .Where(file => file.LinkType == request.LinkType && file.LinkId == request.LinkId)
            .ProjectTo<CompletionRecordCertificateDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
