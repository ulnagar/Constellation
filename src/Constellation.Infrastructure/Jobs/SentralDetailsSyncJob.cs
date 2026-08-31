namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Core.Enums;
using Constellation.Core.Primitives;
using Core.Extensions;
using Core.Models.Attachments;
using Core.Models.Attachments.Enums;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Models.Students;
using Core.Models.Students.Enums;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SentralDetailsSyncJob : ISentralDetailsSyncJob
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISentralGateway _gateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SentralDetailsSyncJob(
        IStudentRepository studentRepository,
        ISentralGateway gateway,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _gateway = gateway;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<ISentralDetailsSyncJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    {
        List<Student> students = await _studentRepository.GetCurrentStudents(token);

        foreach (Student student in students.OrderBy(student => student.CurrentEnrolment?.Grade).ThenBy(student => student.Name.SortOrder))
        {
            if (token.IsCancellationRequested)
                return;

            _logger.Information("{id}: Checking student {student} ({grade}) for additional details", jobId, student.Name.DisplayName, student.CurrentEnrolment?.Grade.AsName());

            if (student.StudentReferenceNumber is null ||
                student.StudentReferenceNumber == StudentReferenceNumber.Empty)
            {
                _logger
                    .Warning("{id}: No student identifier found for student {student} ({grade})", jobId, student.Name.DisplayName, student.CurrentEnrolment?.Grade.AsName());
                
                continue;
            }
            
            SystemLink? sentralId = student.SystemLinks.FirstOrDefault(link => link.System == SystemType.Sentral);

            if (sentralId is null)
                continue;

            IndigenousStatus status = await _gateway.GetStudentIndigenousStatus(sentralId.Value);

            if (student.IndigenousStatus != status && status != IndigenousStatus.Unknown)
            {
                student.UpdateIndigenousStatus(status);

                await _unitOfWork.CompleteAsync(token);
            }
        }
    }
}