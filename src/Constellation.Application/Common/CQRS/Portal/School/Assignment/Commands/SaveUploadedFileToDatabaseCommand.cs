using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Portal.School.Assignment.Commands
{
    public class SaveUploadedFileToDatabaseCommand : IRequest<int>
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] FileData { get; set; }
    }

    public class SaveUploadedFileToDatabaseCommandHandler : IRequestHandler<SaveUploadedFileToDatabaseCommand, int>
    {
        private readonly IAppDbContext _context;

        public SaveUploadedFileToDatabaseCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(SaveUploadedFileToDatabaseCommand request, CancellationToken cancellationToken)
        {
            var entry = new StoredFile
            {                
                Name = request.FileName,
                FileType = request.FileType,
                FileData = request.FileData
            };

            _context.StoredFiles.Add(entry);
            await _context.SaveChangesAsync(cancellationToken);

            return entry.Id;
        }
    }
}
