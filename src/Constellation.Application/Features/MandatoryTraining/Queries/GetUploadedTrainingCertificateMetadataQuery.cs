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

public record GetUploadedTrainingCertificateMetadataQuery : IRequest<CompletionRecordCertificateDto>
{
    public string LinkType { get; init; }
    public string LinkId { get; init; }
}

public class GetUploadedTrainingCertificateFileMetadataHandler : IRequestHandler<GetUploadedTrainingCertificateMetadataQuery, CompletionRecordCertificateDto>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetUploadedTrainingCertificateFileMetadataHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CompletionRecordCertificateDto> Handle(GetUploadedTrainingCertificateMetadataQuery request, CancellationToken cancellationToken)
    {
        return await _context.StoredFiles
            .Where(file => file.LinkType == request.LinkType && file.LinkId == request.LinkId)
            .ProjectTo<CompletionRecordCertificateDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
