﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.StaffSidebarMenu;

using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffByEmail;
using Application.Domains.Timetables.Timetables.Queries.GetStaffDailyTimetableData;
using Application.Domains.Training.Queries.GetCountOfExpiringCertificatesForStaffMember;
using Application.Extensions;
using Constellation.Application.Domains.Offerings.Queries.GetCurrentOfferingsForTeacher;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Enums;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class StaffSidebarMenuViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IStaffRepository _staffRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ISender _mediator;

    public StaffSidebarMenuViewComponent(
        ICurrentUserService currentUserService,
        IStaffRepository staffRepository,
        IDateTimeProvider dateTime,
        ISender mediator)
    {
        _currentUserService = currentUserService;
        _staffRepository = staffRepository;
        _dateTime = dateTime;
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(string activePage)
    {
        Result<EmailAddress> staffEmailAddress = EmailAddress.Create(_currentUserService.EmailAddress);
        StaffMember? staffMember = staffEmailAddress.IsSuccess 
            ? await _staffRepository.GetCurrentByEmailAddress(staffEmailAddress.Value) 
            : null;
        StaffId staffId = staffMember?.Id ?? StaffId.Empty;

        string[] segments = activePage.Split('.');
        string module = segments[0];
        
        return module switch
        {
            "Dashboard" => View("Dashboard", await GenerateDashboardData()),
            "Equipment" => View("Equipment", activePage),
            "Partner" => View("Partner", activePage),
            "ShortTerm" => View("ShortTerm", activePage),
            "SchoolAdmin" => View("SchoolAdmin", (activePage, staffId)),
            "StudentAdmin" => View("StudentAdmin", activePage),
            "Subject" => View("Subject", activePage),
            _ => Content(string.Empty)
        };
    }

    private async Task<DashboardModel> GenerateDashboardData()
    {
        DashboardModel model = new();

        string? username = User.Identity?.Name;

        if (username is not null)
        {
            Result<List<TeacherOfferingResponse>> query = await _mediator.Send(new GetCurrentOfferingsForTeacherQuery(StaffId.Empty, username));

            if (query.IsSuccess)
                model.Classes = query.Value.ToDictionary(k => k.OfferingName.Value, k => k.OfferingId);
        }

        Result<StaffSelectionListResponse> teacherRequest = await _mediator.Send(new GetStaffByEmailQuery(username));
        if (teacherRequest.IsFailure)
            return model;

        model.StaffId = teacherRequest.Value!.StaffId;

        Result<int> trainingExpiringSoonRequest = await _mediator.Send(new GetCountOfExpiringCertificatesForStaffMemberQuery(model.StaffId));

        if (trainingExpiringSoonRequest.IsSuccess)
            model.ExpiringTraining = trainingExpiringSoonRequest.Value;

        Result<List<StaffDailyTimetableResponse>> timetable = await _mediator.Send(new GetStaffDailyTimetableDataQuery(model.StaffId));

        if (timetable.IsFailure)
            return model;

        DateOnly today = _dateTime.Today;
        int dayNumber = today.GetDayNumber();
        PeriodWeek week = PeriodWeek.FromDayNumber(dayNumber);
        PeriodDay day = PeriodDay.FromDayNumber(dayNumber);

        model.Day = day;
        model.Week = week;
        model.Periods = timetable.Value;
        
        return model;
    }
}