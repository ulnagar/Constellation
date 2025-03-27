namespace Constellation.Application.Awards.IssueAwardInSentral;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Abstractions.Services;
using Core.Models.Awards;
using Core.Models.Students;
using Core.Models.Students.Enums;
using Core.Models.Students.Repositories;
using Core.Shared;
using Enums;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed record IssueAwardInSentralCommandHandler
    : ICommandHandler<IssueAwardInSentralCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentAwardRepository _awardRepository;
    private readonly ISentralGateway _gateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IssueAwardInSentralCommandHandler(
        IStudentRepository studentRepository,
        IStudentAwardRepository awardRepository,
        ISentralGateway gateway,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _awardRepository = awardRepository;
        _gateway = gateway;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IssueAwardInSentralCommand>();
    }

    public async Task<Result> Handle(IssueAwardInSentralCommand request, CancellationToken cancellationToken)
    {
        List<Student> students = await _studentRepository.GetListFromIds(request.StudentIds, cancellationToken);

        List<string> sentralIds = students
            .SelectMany(student => student.SystemLinks.Where(link => link.System == SystemType.Sentral))
            .Select(link => link.Value)
            .Distinct()
            .ToList();

        Result<DateTime> result = await _gateway.IssueAward(sentralIds, request.AwardType);

        if (result.IsFailure)
            return result;

        foreach (var student in students)
        {
            string awardType = request.AwardType switch
            {
                IssueAwardType.Stellar => StudentAward.Stellar,
                IssueAwardType.Galaxy => StudentAward.Galaxy,
                IssueAwardType.Universal => StudentAward.Universal
            };

            StudentAward entry = StudentAward.Create(
                student.Id,
                awardType,
                awardType,
                result.Value);

            switch (request.AwardType)
            {
                case IssueAwardType.Stellar:
                    student.AwardTally.AddStellar();
                    break;

                case IssueAwardType.Galaxy:
                    student.AwardTally.AddGalaxyMedal();
                    break;

                case IssueAwardType.Universal:
                    student.AwardTally.AddUniversalAchiever();
                    break;
            }

            _awardRepository.Insert(entry);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
        
        return result;
    }
}