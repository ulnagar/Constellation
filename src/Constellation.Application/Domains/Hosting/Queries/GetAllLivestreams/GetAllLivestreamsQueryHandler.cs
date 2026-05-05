namespace Constellation.Application.Domains.Hosting.Queries.GetAllLivestreams;

using Abstractions.Messaging;
using Core.Models.Hosting;
using Core.Models.Hosting.Repositories;
using Core.Shared;
using System.Collections.Generic;

internal sealed class GetAllLivestreamsQueryHandler
: IQueryHandler<GetAllLivestreamsQuery, List<Livestream>>
{
    private readonly IHostingRepository _hostingRepository;

    public GetAllLivestreamsQueryHandler(
        IHostingRepository hostingRepository)
    {
        _hostingRepository = hostingRepository;
    }

    public async Task<Result<List<Livestream>>> Handle(GetAllLivestreamsQuery request, CancellationToken cancellationToken)
    {
        List<Livestream> livestreams = await _hostingRepository.GetAllLivestreams(cancellationToken);

        return livestreams;
    }
}
