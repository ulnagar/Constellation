namespace Constellation.Application.Students.UpdateStudentAdobeConnectId;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Gateways;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateStudentAdobeConnectIdCommandHandler
    : ICommandHandler<UpdateStudentAdobeConnectIdCommand, string>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAdobeConnectGateway _gateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateStudentAdobeConnectIdCommandHandler(
        IStudentRepository studentRepository,
        IAdobeConnectGateway gateway,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _gateway = gateway;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateStudentAdobeConnectIdCommand>();
    }

    public async Task<Result<string>> Handle(UpdateStudentAdobeConnectIdCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(UpdateStudentAdobeConnectIdCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to update Student Adobe Connect Id");

            return Result.Failure<string>(StudentErrors.NotFound(request.StudentId));
        }

        string id = await _gateway.GetUserPrincipalId(student.PortalUsername);

        if (string.IsNullOrWhiteSpace(id))
        {
            _logger
                .ForContext(nameof(UpdateStudentAdobeConnectIdCommand), request, true)
                .ForContext(nameof(Error), new Error("ExternalGateway.AdobeConnect", "Failed to retrieve student Adobe Connect Id"), true)
                .Warning("Failed to update Student Adobe Connect Id");

            return Result.Failure<string>(new Error("ExternalGateway.AdobeConnect", "Failed to retrieve student Adobe Connect Id"));
        }

        student.AdobeConnectPrincipalId = id;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return id;
    }
}
