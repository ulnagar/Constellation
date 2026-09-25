namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance.Statistics;

using Application.Common.PresentationModels;
using Application.Domains.Attendance.Absences.Models;
using Application.Domains.Attendance.Absences.Queries.GetAttendanceStatistics;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.StudentAdmin_AttendanceList_View_Value)]
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
            .ForContext<IndexModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Attendance_Statistics;
    [ViewData] public string PageTitle => "Absences";

    public AttendanceStatisticsResponse Statistics { get; set; }
    
    public async Task OnGet()
    {
        Result<AttendanceStatisticsResponse> statistics = await _mediator.Send(new GetAttendanceStatisticsQuery());

        if (statistics.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), statistics.Error, true)
                .Warning("Failed to retrieve Attendance Statistics by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(statistics.Error);

            return;
        }

        Statistics = statistics.Value;
    }

    public async Task OnGetFaker()
    {
        Statistics = new AttendanceStatisticsResponse
        {
            WholeSchoolAttendancePercentage =
        new Dictionary<string, decimal>
        {
            ["Term 3, Week 5, 2026"] = 87.92057497181510710259301015m,
            ["Term 3, Week 6, 2026"] = 87.75744920993227990970654628m,
            ["Term 3, Week 7, 2026"] = 87.72610169491525423728813559m,
            ["Term 3, Week 8, 2026"] = 87.74577576443941109852774632m,
            ["Term 3, Week 9, 2026"] = 87.38468785471055618615209989m
        },
            CurrentWholeSchoolAttendanceByGrade =
        new Dictionary<Grade, decimal>
        {
            [Grade.Y12] = 83.13675m,
            [Grade.Y11] = 85.34776m,
            [Grade.Y09] = 87.35756521739130434782608696m,
            [Grade.Y08] = 86.33578571428571428571428571m,
            [Grade.Y05] = 91.16454545454545454545454545m,
            [Grade.Y07] = 88.9200884955752212389380531m,
            [Grade.Y06] = 89.26144578313253012048192771m,
            [Grade.Y10] = 85.03323943661971830985915493m
        },
            UnexplainedPartialAbsenceCountByGrade =
        new Dictionary<Grade, int>
        {
            [Grade.Y05] = 188,
            [Grade.Y06] = 180,
            [Grade.Y07] = 302,
            [Grade.Y08] = 452,
            [Grade.Y09] = 405,
            [Grade.Y10] = 195,
            [Grade.Y11] = 258,
            [Grade.Y12] = 269
        },
            TotalPartialAbsenceCountByGrade =
        new Dictionary<Grade, int>
        {
            [Grade.Y05] = 1632,
            [Grade.Y06] = 589,
            [Grade.Y07] = 3593,
            [Grade.Y08] = 4205,
            [Grade.Y09] = 3263,
            [Grade.Y10] = 2280,
            [Grade.Y11] = 946,
            [Grade.Y12] = 652
        },
            PartialAbsencePercentageByGrade =
        new Dictionary<Grade, decimal>
        {
            [Grade.Y05] = 11.519607843137254901960784310m,
            [Grade.Y06] = 30.560271646859083191850594230m,
            [Grade.Y07] = 8.405232396326189813526301140m,
            [Grade.Y08] = 10.749108204518430439952437570m,
            [Grade.Y09] = 12.411890897946674839105117990m,
            [Grade.Y10] = 8.552631578947368421052631580m,
            [Grade.Y11] = 27.272727272727272727272727270m,
            [Grade.Y12] = 41.257668711656441717791411040m
        },
            UnexplainedWholeAbsenceCountByGrade =
        new Dictionary<Grade, int>
        {
            [Grade.Y05] = 179,
            [Grade.Y06] = 84,
            [Grade.Y07] = 175,
            [Grade.Y08] = 318,
            [Grade.Y09] = 228,
            [Grade.Y10] = 141,
            [Grade.Y11] = 210,
            [Grade.Y12] = 143
        },
            TotalWholeAbsenceCountByGrade =
        new Dictionary<Grade, int>
        {
            [Grade.Y05] = 2693,
            [Grade.Y06] = 1824,
            [Grade.Y07] = 2232,
            [Grade.Y08] = 3313,
            [Grade.Y09] = 2529,
            [Grade.Y10] = 2046,
            [Grade.Y11] = 1409,
            [Grade.Y12] = 1328
        },
            WholeAbsencePercentageByGrade = new Dictionary<Grade, decimal>
            {
                [Grade.Y05] = 6.6468622354251763832157445200m,
                [Grade.Y06] = 4.6052631578947368421052631600m,
                [Grade.Y07] = 7.8405017921146953405017921100m,
                [Grade.Y08] = 9.598551162088741322064594020m,
                [Grade.Y09] = 9.015421115065243179122182680m,
                [Grade.Y10] = 6.8914956011730205278592375400m,
                [Grade.Y11] = 14.904187366926898509581263310m,
                [Grade.Y12] = 10.767206222891566265060240963m
            }
        };
    }
}