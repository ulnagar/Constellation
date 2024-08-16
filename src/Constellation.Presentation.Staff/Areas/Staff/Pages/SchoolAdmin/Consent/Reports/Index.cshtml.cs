namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Consent.Reports;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.ThirdPartyConsent.GetApplications;
using Application.ThirdPartyConsent.GetConsentStatusByApplication;
using Constellation.Application.Offerings.GetOfferingsForSelectionList;
using Constellation.Application.Schools.GetCurrentPartnerSchoolsWithStudentsList;
using Constellation.Application.Schools.Models;
using Constellation.Application.StaffMembers.GetStaffLinkedToOffering;
using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Enums;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Offerings.Identifiers;
using Core.Models.ThirdPartyConsent.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;
using System.Threading;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Consent_Reports;
    [ViewData] public string PageTitle => "Consent Reports";

    [BindProperty] 
    public FilterDefinition Filter { get; set; } = new();
    
    public List<ConsentStatusResponse> Statuses { get; set; } = new();

    public List<ApplicationSummaryResponse> ApplicationList { get; set; } = new();
    public List<ClassRecord> ClassSelectionList { get; set; } = new();
    public List<SchoolSelectionListResponse> SchoolsList { get; set; } = new();
    
    public async Task<IActionResult> OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        _logger.Information("Requested to retrieve filtered Consent data by user {User}", _currentUserService.UserName);

        List<OfferingId> offeringIds = Filter.Offerings.Select(OfferingId.FromValue).ToList();

        foreach (Guid id in Filter.Applications)
        {
            ApplicationId applicationId = ApplicationId.FromValue(id);

            Result<List<ConsentStatusResponse>> response = await _mediator.Send(new GetConsentStatusByApplicationQuery(
                applicationId,
                offeringIds,
                Filter.Grades,
                Filter.Schools),
                cancellationToken);

            if (response.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), response.Error, true)
                    .Warning("Failed to retrieve filtered Consent data by user {User}", _currentUserService.UserName);
                
                ModalContent = new ErrorDisplay(response.Error);

                return await PreparePage(cancellationToken);
            }

            Statuses.AddRange(response.Value);
        }

        return await PreparePage(cancellationToken);
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve reports for Consent data by user {User}", _currentUserService.UserName);

        Result<List<OfferingSelectionListResponse>> classesResponse = await _mediator.Send(new GetOfferingsForSelectionListQuery(), cancellationToken);

        if (classesResponse.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), classesResponse.Error, true)
                .Warning("Failed to retrieve reports for Consent data by user {User}", _currentUserService.UserName);
            
            ModalContent = new ErrorDisplay(classesResponse.Error);

            return Page();
        }

        foreach (OfferingSelectionListResponse course in classesResponse.Value)
        {
            Result<List<StaffSelectionListResponse>> teachers = await _mediator.Send(new GetStaffLinkedToOfferingQuery(course.Id), cancellationToken);

            if (teachers.IsFailure || teachers.Value.Count == 0)
            {
                ClassSelectionList.Add(new ClassRecord(
                    course.Id,
                    course.Name,
                    string.Empty,
                    $"Year {course.Name[..2]}"));

                continue;
            }

            if (teachers.Value.Count == 1)
            {
                StaffSelectionListResponse primaryTeacher = teachers.Value.First();

                ClassSelectionList.Add(new ClassRecord(
                    course.Id,
                    course.Name,
                    $"{primaryTeacher.FirstName[..1]} {primaryTeacher.LastName}",
                    $"Year {course.Name[..2]}"));

                continue;
            }

            string teacherNames = string.Join(
                ", ", 
                teachers.Value
                    .Select(entry => $"{entry.FirstName[..1]} {entry.LastName}"));

            ClassSelectionList.Add(new ClassRecord(
                course.Id,
                course.Name,
                teacherNames,
                $"Year {course.Name[..2]}"));
        }

        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetCurrentPartnerSchoolsWithStudentsListQuery(), cancellationToken);

        if (schoolsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), schoolsRequest.Error, true)
                .Warning("Failed to retrieve reports for Consent data by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(schoolsRequest.Error);

            return Page();
        }

        SchoolsList = schoolsRequest.Value;

        Result<List<ApplicationSummaryResponse>> applicationsRequest = await _mediator.Send(new GetApplicationsQuery(), cancellationToken);

        if (applicationsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), applicationsRequest.Error, true)
                .Warning("Failed to retrieve reports for Consent data by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(applicationsRequest.Error);

            return Page();
        }

        ApplicationList = applicationsRequest.Value;

        return Page();
    }

    public class FilterDefinition
    {
        public List<Guid> Offerings { get; set; } = new();
        public List<Grade> Grades { get; set; } = new();
        public List<string> Schools { get; set; } = new();
        public List<Guid> Applications { get; set; } = new();
    }
}