using Constellation.Application.Features.Jobs.SentralReportSync.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Jobs.SentralReportSync.Queries
{
    public class GetStudentsForReportDownloadQuery : IRequest<ICollection<StudentForReportDownload>>
    {
    }
}
