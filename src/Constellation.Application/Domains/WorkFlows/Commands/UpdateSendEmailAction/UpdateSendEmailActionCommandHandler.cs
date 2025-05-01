namespace Constellation.Application.Domains.WorkFlows.Commands.UpdateSendEmailAction;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Enums;
using Core.Models.WorkFlow.Errors;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using DTOs;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateSendEmailActionCommandHandler
    : ICommandHandler<UpdateSendEmailActionCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public UpdateSendEmailActionCommandHandler(
        ICaseRepository caseRepository,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger.ForContext<UpdateSendEmailActionCommand>();
    }

    public async Task<Result> Handle(UpdateSendEmailActionCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(UpdateSendEmailActionCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.NotFound(request.CaseId), true)
                .Warning("Failed to update Action");

            return Result.Failure(CaseErrors.NotFound(request.CaseId));
        }

        Action action = item.Actions.FirstOrDefault(action => action.Id == request.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(UpdateSendEmailActionCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), ActionErrors.NotFound(request.ActionId), true)
                .Warning("Failed to update Action");

            return Result.Failure(ActionErrors.NotFound(request.ActionId));
        }

        if (action is SendEmailAction emailAction)
        {
            List<Attachment> attachments = new();
            
            foreach (FileDto file in request.Attachments)
            {
                MemoryStream stream = new(file.FileData);

                Attachment attachment = new(stream, file.FileName);

                attachments.Add(attachment);
            }

            Result updateAction = emailAction.Update(
                request.Recipients, 
                request.Sender, 
                request.Subject, 
                request.Body,
                request.Attachments.Count > 0, 
                _dateTime.Now);

            if (updateAction.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpdateSendEmailActionCommand), request, true)
                    .ForContext(nameof(Case), item, true)
                    .ForContext(nameof(Error), updateAction.Error, true)
                    .Warning("Failed to update Action");

                return Result.Failure(updateAction.Error);
            }

            Result statusUpdate = item.UpdateActionStatus(action.Id, ActionStatus.Completed, _currentUserService.UserName);

            if (statusUpdate.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpdateSendEmailActionCommand), request, true)
                    .ForContext(nameof(Case), item, true)
                    .ForContext(nameof(Error), statusUpdate.Error, true)
                    .Warning("Failed to update Action");

                return Result.Failure(statusUpdate.Error);
            }

            await _emailService.SendEnteredEmailForAction(
                request.Recipients,
                request.Sender,
                request.Subject,
                request.Body,
                attachments,
                cancellationToken);

            await _unitOfWork.CompleteAsync(cancellationToken);
            
            return Result.Success();
        }

        _logger
            .ForContext(nameof(UpdateSendEmailActionCommand), request, true)
            .ForContext(nameof(Case), item, true)
            .ForContext(nameof(Error),
                ActionErrors.UpdateTypeMismatch(nameof(SendEmailAction), action.GetType().ToString()),
                true)
            .Warning("Failed to update Action");

        return Result.Failure(ActionErrors.UpdateTypeMismatch(nameof(SendEmailAction), action.GetType().ToString()));

    }
}