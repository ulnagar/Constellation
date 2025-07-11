﻿namespace Constellation.Application.Domains.GroupTutorials.Commands.SubmitRoll;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.GroupTutorials;
using Core.Errors;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SubmitRollCommandHandler
    : ICommandHandler<SubmitRollCommand>
{
    private readonly IGroupTutorialRepository _groupTutorialRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitRollCommandHandler(
        IGroupTutorialRepository groupTutorialRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork)
    {
        _groupTutorialRepository = groupTutorialRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SubmitRollCommand request, CancellationToken cancellationToken)
    {
        GroupTutorial tutorial = await _groupTutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
            return Result.Failure(DomainErrors.GroupTutorials.GroupTutorial.NotFound(request.TutorialId));

        TutorialRoll roll = tutorial.Rolls.FirstOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
            return Result.Failure(DomainErrors.GroupTutorials.TutorialRoll.NotFound(request.RollId));

        Result<EmailAddress> emailAddress = EmailAddress.Create(request.StaffEmail);

        StaffMember staffMember = emailAddress.IsSuccess
            ? await _staffRepository.GetCurrentByEmailAddress(emailAddress.Value, cancellationToken)
            : null;

        if (staffMember is null)
            return Result.Failure(StaffMemberErrors.NotFoundByEmail(request.StaffEmail));

        Result result = tutorial.SubmitRoll(roll, staffMember, request.StudentPresence);

        if (result.IsSuccess)
            await _unitOfWork.CompleteAsync(cancellationToken);

        return result;
    }
}
