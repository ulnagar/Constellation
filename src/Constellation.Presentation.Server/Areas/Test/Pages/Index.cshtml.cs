namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.DTOs;
using Application.Extensions;
using Application.Interfaces.Gateways;
using Application.Interfaces.Repositories;
using BaseModels;
using Core.Models.Enrolments;
using Core.Models.Offerings;
using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.Students;
using MediatR;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ISentralGateway _gateway;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IStudentRepository _studentRepository;

    public IndexModel(
        IMediator mediator,
        ISentralGateway gateway,
        IOfferingRepository offeringRepository,
        IEnrolmentRepository enrolmentRepository,
        IStudentRepository studentRepository)
    {
        _mediator = mediator;
        _gateway = gateway;
        _offeringRepository = offeringRepository;
        _enrolmentRepository = enrolmentRepository;
        _studentRepository = studentRepository;
    }

    public List<RollToInvestigate> Rolls { get; set; } = new();

    public async Task OnGet()
    {
    }

    public async Task OnGetScanDate(DateOnly date)
    {
        ICollection<RollMarkReportDto> rolls = await _gateway.GetRollMarkingReportAsync(date);

        foreach (RollMarkReportDto roll in rolls.Where(roll => roll.Submitted))
        {
            bool sentToAttendance = false;

            string offeringName = roll.ClassName.PadLeft(7, '0');

            Offering offering = await _offeringRepository.GetFromYearAndName(2024, offeringName);

            if (offering is null)
                continue;

            List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByOfferingId(offering.Id);

            IEnumerable<Enrolment> currentStudentIds = enrolments.Where(entry => !entry.IsDeleted).ToList();

            string firstStudentId = currentStudentIds.FirstOrDefault()?.StudentId;

            if (!string.IsNullOrWhiteSpace(firstStudentId))
            {
                Student student = await _studentRepository.GetById(firstStudentId);

                if (string.IsNullOrWhiteSpace(student?.SentralStudentId))
                    continue;

                sentToAttendance = await _gateway.GetAttendanceRollSyncStatus(student.SentralStudentId, date);
            }

            if (sentToAttendance)
                continue;

            Rolls.Add(new()
            {
                Date = date,
                OfferingId = offering.Id,
                OfferingName = offering.Name
            });
        }
    }

    public class RollToInvestigate
    {
        public DateOnly Date { get; set; }
        public OfferingId OfferingId { get; set; }
        public OfferingName OfferingName { get; set; }
    }
}