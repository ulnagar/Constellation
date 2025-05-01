namespace Constellation.Application.Domains.MeritAwards.Awards.Events;

using Abstractions.Messaging;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using Core.Models.Awards;
using Core.Models.Awards.Events;
using Core.Models.Students.Enums;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AwardMatchedToIncidentDomainEvent_DownloadAwardCertificate
: IDomainEventHandler<AwardMatchedToIncidentDomainEvent>
{
    private readonly IStudentAwardRepository _awardRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISentralGateway _gateway;
    private readonly IAttachmentService _attachmentService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AwardMatchedToIncidentDomainEvent_DownloadAwardCertificate(
        ILogger logger, 
        ISentralGateway gateway, 
        IStudentRepository studentRepository, 
        IAttachmentRepository attachmentRepository, 
        IStudentAwardRepository awardRepository, 
        IAttachmentService attachmentService, 
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _logger = logger.ForContext<AwardMatchedToIncidentDomainEvent>();
        _gateway = gateway;
        _studentRepository = studentRepository;
        _attachmentRepository = attachmentRepository;
        _awardRepository = awardRepository;
        _attachmentService = attachmentService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AwardMatchedToIncidentDomainEvent notification, CancellationToken cancellationToken)
    {
        StudentAward award = await _awardRepository.GetById(notification.AwardId, cancellationToken);

        if (award is null)
        {
            _logger
                .ForContext(nameof(AwardMatchedToIncidentDomainEvent), notification, true)
                .Warning("Failed to retrieve award certificate");

            return;
        }

        Student student = await _studentRepository.GetById(award.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(AwardMatchedToIncidentDomainEvent), notification, true)
                .ForContext(nameof(StudentAward), award, true)
                .Warning("Failed to retrieve award certificate");

            return;
        }

        SystemLink link = student.SystemLinks.FirstOrDefault(entry => entry.System == SystemType.Sentral);

        if (link is null)
        {
            _logger
                .ForContext(nameof(AwardMatchedToIncidentDomainEvent), notification, true)
                .ForContext(nameof(StudentAward), award, true)
                .Warning("Failed to retrieve award certificate");

            return;
        }

        byte[] awardDocument = await _gateway.GetAwardDocument(link.Value, award.IncidentId);

        if (awardDocument.Length == 0)
        {
            _logger
                .ForContext(nameof(AwardMatchedToIncidentDomainEvent), notification, true)
                .ForContext(nameof(StudentAward), award, true)
                .Warning("Failed to retrieve award certificate");
            
            return;
        }

        Attachment attachment = Attachment.CreateAwardCertificateAttachment(
            $"{student.Name.DisplayName} - Astra Award - {award.AwardedOn:dd-MM-yyyy} - {award.IncidentId}.pdf",
            MediaTypeNames.Application.Pdf,
            award.Id.ToString(),
            _dateTime.Now);

        await _attachmentService.StoreAttachmentData(
            attachment,
            awardDocument,
            false,
            cancellationToken);

        _attachmentRepository.Insert(attachment);

        award.CertificateDownloaded();

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
