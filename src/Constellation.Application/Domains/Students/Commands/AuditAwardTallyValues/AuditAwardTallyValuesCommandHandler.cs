namespace Constellation.Application.Domains.Students.Commands.AuditAwardTallyValues;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Repositories;
using Core.Models.Awards;
using Core.Models.Students;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AuditAwardTallyValuesCommandHandler
    :ICommandHandler<AuditAwardTallyValuesCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AuditAwardTallyValuesCommandHandler(
        IStudentRepository studentRepository,
        IStudentAwardRepository awardRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _awardRepository = awardRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AuditAwardTallyValuesCommand>();
    }

    public async Task<Result> Handle(AuditAwardTallyValuesCommand request, CancellationToken cancellationToken)
    {
        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        foreach (Student student in students)
        {
            List<StudentAward> awards = await _awardRepository.GetByStudentId(student.Id, cancellationToken);

            int issuedAstras = awards.Count(award => award.Type == StudentAward.Astra);
            int issuedStellars = awards.Count(award => award.Type == StudentAward.Stellar);
            int issuedGalaxies = awards.Count(award => award.Type == StudentAward.Galaxy);
            int issuedUniversals = awards.Count(award => award.Type == StudentAward.Universal);

            if (issuedAstras > student.AwardTally.Astras)
            {
                int difference = issuedAstras - student.AwardTally.Astras;

                for (int i = 0; i < difference; i++)
                    student.AwardTally.AddAstra();
            }

            if (student.AwardTally.Astras > issuedAstras)
            {
                _logger
                    .ForContext(nameof(Student), student, true)
                    .ForContext(nameof(issuedAstras), issuedAstras)
                    .ForContext(nameof(AwardTally.Astras), student.AwardTally.Astras)
                    .Error("Student Award Tally has become out of sync");
            }

            if (issuedStellars > student.AwardTally.Stellars)
            {
                int difference = issuedStellars - student.AwardTally.Stellars;

                for (int i = 0; i < difference; i++)
                    student.AwardTally.AddStellar();
            }

            if (student.AwardTally.Stellars > issuedStellars)
            {
                _logger
                    .ForContext(nameof(Student), student, true)
                    .ForContext(nameof(issuedStellars), issuedStellars)
                    .ForContext(nameof(AwardTally.Stellars), student.AwardTally.Stellars)
                    .Error("Student Award Tally has become out of sync");
            }

            if (issuedGalaxies > student.AwardTally.GalaxyMedals)
            {
                int difference = issuedGalaxies - student.AwardTally.GalaxyMedals;

                for (int i = 0; i < difference; ++i)
                    student.AwardTally.AddGalaxyMedal();
            }

            if (student.AwardTally.GalaxyMedals > issuedGalaxies)
            {
                _logger
                    .ForContext(nameof(Student), student, true)
                    .ForContext(nameof(issuedGalaxies), issuedGalaxies)
                    .ForContext(nameof(AwardTally.GalaxyMedals), student.AwardTally.GalaxyMedals)
                    .Error("Student Award Tally has become out of sync");
            }

            if (issuedUniversals > student.AwardTally.UniversalAchievers)
            {
                int difference = issuedUniversals - student.AwardTally.UniversalAchievers;

                for (int i = 0; i < difference; ++i)
                    student.AwardTally.AddUniversalAchiever();
            }

            if (student.AwardTally.UniversalAchievers > issuedUniversals)
            {
                _logger
                    .ForContext(nameof(Student), student, true)
                    .ForContext(nameof(issuedUniversals), issuedUniversals)
                    .ForContext(nameof(AwardTally.UniversalAchievers), student.AwardTally.UniversalAchievers)
                    .Error("Student Award Tally has become out of sync");
            }
            
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}
