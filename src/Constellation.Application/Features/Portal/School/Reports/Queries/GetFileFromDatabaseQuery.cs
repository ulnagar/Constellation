using Constellation.Core.Models;
using MediatR;

namespace Constellation.Application.Features.Portal.School.Reports.Queries
{
    public class GetFileFromDatabaseQuery : IRequest<StoredFile>
    {
        public string LinkType { get; set; }
        public string LinkId { get; set; }
    }
}
