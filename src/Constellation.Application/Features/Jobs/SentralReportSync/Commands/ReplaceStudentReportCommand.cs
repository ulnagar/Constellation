using MediatR;

namespace Constellation.Application.Features.Jobs.SentralReportSync.Commands
{
    public class ReplaceStudentReportCommand : IRequest
    {
        public string OldPublishId { get; set; }
        public string NewPublishId { get; set; }
        public byte[] File { get; set; }
    }
}
