using Constellation.Application.Features.Portal.School.Assignments.Commands;
using Constellation.Application.Features.Portal.School.Assignments.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Test.Pages
{
    public class SubmitModel : BasePageModel
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;

        public SubmitModel(IMediator mediator, IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            Files = new List<FileDto>();
        }

        [BindProperty]
        public IFormFile SubmittedFile { get; set; }

        public ICollection<FileDto> Files { get; set; }
        [BindProperty]
        public int FileId { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var files = await _mediator.Send(new GetListOfStoredFilesQuery());

            foreach (var file in files)
            {
                var entry = new FileDto
                {
                    Id = file.Id,
                    Name = file.Name,
                    FileType = file.FileType,
                    Size = file.FileData.Length
                };

                Files.Add(entry);
            }

            await GetClasses(_unitOfWork);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var command = new SaveUploadedFileToDatabaseCommand
            {
                FileName = SubmittedFile.FileName,
                FileType = SubmittedFile.ContentType
            };

            using (var target = new MemoryStream())
            {
                SubmittedFile.CopyTo(target);
                command.FileData = target.ToArray();
            }

            var id = await _mediator.Send(command);

            return Page();
        }

        public async Task<IActionResult> OnPostDownloadFileAsync()
        {
            var file = await _mediator.Send(new GetStoredFileByIdQuery { Id = FileId });

            if (file != null)
                return File(file.FileData, file.FileType, file.Name);
            else
                return NotFound();
        }

        public class FileDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string FileType { get; set; }
            public int Size { get; set; }
        }
    }
}
