using MediatR;

namespace Constellation.Application.Features.Jobs.SentralReportSync.Commands
{
    public class UploadStudentReportCommand : IRequest
    {
        public string StudentId { get; set; }
        public string PublishId { get; set; }
        public string Year { get; set; }
        public string ReportingPeriod { get; set; }
        public byte[] File { get; set; }
        public string FileName { get; set; }
    }
}
