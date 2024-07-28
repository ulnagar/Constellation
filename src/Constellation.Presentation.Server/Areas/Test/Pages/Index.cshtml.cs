namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.DTOs;
using Application.Interfaces.Gateways;
using BaseModels;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Infrastructure.ExternalServices.Sentral;
using Microsoft.Extensions.Options;
using Serilog;
using System.Diagnostics;

public class IndexModel : BasePageModel
{
    private readonly IDateTimeProvider _dateTime;
    private readonly IOptions<SentralGatewayConfiguration> _settings;
    private readonly IHttpClientFactory _factory;
    private readonly ILogger _logger;
    private readonly IStudentRepository _studentRepository;

    public IndexModel(
        IDateTimeProvider dateTime,
        IOptions<SentralGatewayConfiguration> settings,
        IHttpClientFactory factory,
        ILogger logger,
        IStudentRepository studentRepository)
    {
        _dateTime = dateTime;
        _settings = settings;
        _factory = factory;
        _logger = logger;
        _studentRepository = studentRepository;
    }

    public string Message { get; set; } = string.Empty;

    public List<RollMarkReportDto> Items { get; set; }

    public async Task OnGet()
    {
        ISentralGateway newGateway = new ApiGateway(
            _settings,
            _factory,
            _logger);

        var newObject = await newGateway.GetRollMarkingReportAsync(new (2024, 7, 26));

        Items = newObject.ToList();
    }

    public async Task OnGetWeekForDateComparison()
    {
        DateOnly date = new DateOnly(2024, 5, 3);

        Stopwatch watch = Stopwatch.StartNew();
        ISentralGateway oldGateway = new Gateway(
            _dateTime,
            _settings,
            _factory,
            _logger);

        var oldObject = await oldGateway.GetWeekForDate(date);
        watch.Stop();

        long oldGatewayTime = watch.ElapsedMilliseconds;

        watch.Reset();
        watch.Start();
        ISentralGateway newGateway = new ApiGateway(
            _settings,
            _factory,
            _logger);

        var newObject = await newGateway.GetWeekForDate(date);
        watch.Stop();

        long newGatewayTime = watch.ElapsedMilliseconds;
    }

    public async Task OnGetDatesForWeekComparison()
    {
        Stopwatch watch = Stopwatch.StartNew();
        ISentralGateway oldGateway = new Gateway(
            _dateTime,
            _settings,
            _factory,
            _logger);

        var oldObject = await oldGateway.GetDatesForWeek("2024", "1", "4");
        watch.Stop();

        long oldGatewayTime = watch.ElapsedMilliseconds;

        watch.Reset();
        watch.Start();
        ISentralGateway newGateway = new ApiGateway(
            _settings,
            _factory,
            _logger);

        var newObject = await newGateway.GetDatesForWeek("2024", "1", "4");
        watch.Stop();

        long newGatewayTime = watch.ElapsedMilliseconds;
    }

    public async Task OnGetValidReportDatesComparison()
    {
        Stopwatch watch = Stopwatch.StartNew();
        ISentralGateway oldGateway = new Gateway(
            _dateTime,
            _settings,
            _factory,
            _logger);

        var oldObject = await oldGateway.GetValidAttendanceReportDatesFromCalendar("2024");
        watch.Stop();

        long oldGatewayTime = watch.ElapsedMilliseconds;

        watch.Reset();
        watch.Start();
        ISentralGateway newGateway = new ApiGateway(
            _settings,
            _factory,
            _logger);

        var newObject = await newGateway.GetValidAttendanceReportDatesFromCalendar("2024");
        watch.Stop();

        long newGatewayTime = watch.ElapsedMilliseconds;
    }

    public async Task OnGetExcludedDatesComparison()
    {
        Stopwatch watch = Stopwatch.StartNew();
        ISentralGateway oldGateway = new Gateway(
            _dateTime,
            _settings,
            _factory,
            _logger);

        List<DateOnly> oldObject = await oldGateway.GetExcludedDatesFromCalendar("2024");
        watch.Stop();

        long oldGatewayTime = watch.ElapsedMilliseconds;

        watch.Reset();
        watch.Start();
        ISentralGateway newGateway = new ApiGateway(
            _settings,
            _factory,
            _logger);

        List<DateOnly> newObject = await newGateway.GetExcludedDatesFromCalendar("2024");
        watch.Stop();

        long newGatewayTime = watch.ElapsedMilliseconds;
    }

    public async Task OnGetAllFamiliesComparison()
    {
        List<Student> students = await _studentRepository.GetCurrentStudentsWithSchool();

        Stopwatch watch = Stopwatch.StartNew();
        ISentralGateway oldGateway = new Gateway(
            _dateTime,
            _settings,
            _factory,
            _logger);

        List<FamilyDetailsDto> families = new();

        Dictionary<string, List<string>> familyGroups = await oldGateway.GetFamilyGroupings();

        foreach (KeyValuePair<string, List<string>> family in familyGroups)
        {
            Student firstStudent = students.FirstOrDefault(student => student.StudentId == family.Value.First());

            if (firstStudent is null)
                continue;

            FamilyDetailsDto entry = await oldGateway.GetParentContactEntry(firstStudent.SentralStudentId);

            entry.StudentIds = family.Value;
            entry.FamilyId = family.Key;

            foreach (FamilyDetailsDto.Contact contact in entry.Contacts)
            {
                string name = contact.FirstName.Contains(' ')
                    ? contact.FirstName.Split(' ')[0]
                    : contact.FirstName;

                name = name.Length > 8 ? name[..8] : name;

                contact.SentralId = $"{entry.FamilyId}-{contact.SentralReference}-{name.ToLowerInvariant()}";
            }

            families.Add(entry);
        }

        watch.Stop();

        long oldGatewayTime = watch.ElapsedMilliseconds;

        watch.Reset();
        watch.Start();
        ISentralGateway newGateway = new ApiGateway(
            _settings,
            _factory,
            _logger);

        ICollection<FamilyDetailsDto> newObject = await newGateway.GetFamilyDetailsReport(_logger);
        watch.Stop();

        long newGatewayTime = watch.ElapsedMilliseconds;
    }

    public async Task OnGetSingleFamilyComparison()
    {
        string sentralId = "2443";

        Stopwatch watch = Stopwatch.StartNew();
        ISentralGateway oldGateway = new Gateway(
            _dateTime,
            _settings,
            _factory,
            _logger);

        FamilyDetailsDto oldObject = await oldGateway.GetParentContactEntry(sentralId);
        watch.Stop();

        long oldGatewayTime = watch.ElapsedMilliseconds;

        watch.Reset();
        watch.Start();
        ISentralGateway newGateway = new ApiGateway(
            _settings,
            _factory,
            _logger);

        FamilyDetailsDto newObject = await newGateway.GetParentContactEntry(sentralId);
        watch.Stop();

        long newGatewayTime = watch.ElapsedMilliseconds;
    }
}