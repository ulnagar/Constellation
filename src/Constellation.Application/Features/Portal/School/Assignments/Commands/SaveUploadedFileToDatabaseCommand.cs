using MediatR;

namespace Constellation.Application.Features.Portal.School.Assignments.Commands
{
    public class SaveUploadedFileToDatabaseCommand : IRequest<int>
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] FileData { get; set; }
    }
}
