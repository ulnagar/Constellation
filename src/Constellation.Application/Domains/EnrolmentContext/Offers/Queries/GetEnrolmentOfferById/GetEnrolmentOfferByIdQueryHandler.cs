namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetEnrolmentOfferById;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.Application.Errors;
using Core.Models.EnrolmentContext.Application.Repositories;
using Core.Models.EnrolmentContext.EnrolmentParent;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Repositories;
using Core.Models.EnrolmentContext.EnrolmentStudent;
using Core.Models.EnrolmentContext.EnrolmentStudent.Errors;
using Core.Models.EnrolmentContext.EnrolmentStudent.Repositories;
using Core.Models.EnrolmentContext.Offer;
using Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Offer.Repositories;
using Core.Shared;
using Serilog;

internal sealed class GetEnrolmentOfferByIdQueryHandler
: IQueryHandler<GetEnrolmentOfferByIdQuery, EnrolmentOfferDetailsResponse>
{
    private readonly IEnrolmentApplicationRepository _applicationRepository;
    private readonly IEnrolmentOfferRepository _offerRepository;
    private readonly IEnrolmentPeriodRepository _periodRepository;
    private readonly IEnrolmentStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetEnrolmentOfferByIdQueryHandler(
        IEnrolmentApplicationRepository applicationRepository,
        IEnrolmentOfferRepository offerRepository,
        IEnrolmentPeriodRepository periodRepository,
        IEnrolmentStudentRepository studentRepository,
        ILogger logger)
    {
        _applicationRepository = applicationRepository;
        _offerRepository = offerRepository;
        _periodRepository = periodRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetEnrolmentOfferByIdQuery>();
    }

    public async Task<Result<EnrolmentOfferDetailsResponse>> Handle(GetEnrolmentOfferByIdQuery request, CancellationToken cancellationToken)
    {
        Offer? offer = await _offerRepository.GetById(request.OfferId, cancellationToken);

        if (offer is null)
        {
            _logger
                .ForContext(nameof(GetEnrolmentOfferByIdQuery), request, true)
                .ForContext(nameof(Error), EnrolmentOfferErrors.NotFound(request.OfferId), true)
                .Warning("Failed to retrieve Offer");

            return Result.Failure<EnrolmentOfferDetailsResponse>(EnrolmentOfferErrors.NotFound(request.OfferId));
        }

        Application? application = await _applicationRepository.GetApplicationById(offer.ApplicationId, cancellationToken);

        if (application is null)
        {
            _logger
                .ForContext(nameof(GetEnrolmentOfferByIdQuery), request, true)
                .ForContext(nameof(Error), EnrolmentApplicationErrors.NotFound(offer.ApplicationId), true)
                .Warning("Failed to retrieve Offer");

            return Result.Failure<EnrolmentOfferDetailsResponse>(EnrolmentApplicationErrors.NotFound(offer.ApplicationId));
        }

        EnrolmentStudent? student = await _studentRepository.GetById(application.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(GetEnrolmentOfferByIdQuery), request, true)
                .ForContext(nameof(Error), EnrolmentStudentErrors.NotFound(application.StudentId), true)
                .Warning("Failed to retrieve Offer");

            return Result.Failure<EnrolmentOfferDetailsResponse>(EnrolmentStudentErrors.NotFound(application.StudentId));
        }

        EnrolmentPeriod? period = await _periodRepository.GetEnrolmentPeriodById(offer.PeriodId, cancellationToken);

        if (period is null)
        {
            _logger
                .ForContext(nameof(GetEnrolmentOfferByIdQuery), request, true)
                .ForContext(nameof(Error), EnrolmentPeriodErrors.NotFound(offer.PeriodId), true)
                .Warning("Failed to retrieve Offer");

            return Result.Failure<EnrolmentOfferDetailsResponse>(EnrolmentPeriodErrors.NotFound(offer.PeriodId));
        }

        List<EnrolmentOfferDetailsResponse.Note> notes = [];

        foreach (OfferNote note in offer.Notes)
        {
            notes.Add(new(
                note.CreatedAt,
                note.CreatedBy,
                note.Note));
        }

        List<EnrolmentOfferDetailsResponse.Parent> parents = [];

        foreach (EnrolmentParent parent in application.Parents)
        {
            parents.Add(new(
                parent.Id,
                parent.Name,
                parent.Email,
                parent.PhoneNumber,
                parent.MailingAddress));
        }

        return new EnrolmentOfferDetailsResponse(
            offer.Id,
            application.Id,
            period.Id,
            period.Label,
            student.Id,
            student.StudentReferenceNumber,
            student.Name,
            student.StudentGender,
            student.DateOfBirth,
            student.StudentEmailAddress,
            parents,
            application.ApplicationReference,
            application.CurrentSchoolCode,
            application.CurrentSchool,
            application.DestinationSchoolCode,
            application.DestinationSchool,
            application.Program,
            application.Grade,
            application.SelectedCourses.Where(entry => entry.Status == CourseSelectionStatus.Approved).Select(entry => entry.Course).ToList(),
            offer.Status,
            offer.OfferedAt,
            offer.RespondBy,
            offer.RespondedAt,
            offer.Response,
            offer.HasCourtOrders,
            offer.HasHealthConcerns,
            offer.RequestedLaptop,
            notes);
    }
}
