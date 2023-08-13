namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Identifiers;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ISentralAwardSyncJob _awardSyncJob;
    private readonly IRollMarkingReportJob _rollMarkingReportJob;
    private readonly AppDbContext _context;
    private readonly AppConfiguration _configuration;

    public IndexModel(
        IMediator mediator,
        ISentralAwardSyncJob awardSyncJob,
        IRollMarkingReportJob rollMarkingReportJob,
        AppDbContext context,
        IOptions<AppConfiguration> configuration)
    {
        _mediator = mediator;
        _awardSyncJob = awardSyncJob;
        _rollMarkingReportJob = rollMarkingReportJob;
        _context = context;
        _configuration = configuration.Value;
    }

    public List<Award> Awards { get; set; } = new();

    public class Award 
    {
        public StudentAwardId Id { get; set; }
        public string StudentName { get; set; }
        public Grade StudentGrade { get; set; }
        public string TeacherName { get; set; }
        public DateTime AwardedOn { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public bool HasCertificate { get; set; }
    }

    public async Task OnGetAsync()
    {
        await GetClasses(_mediator);

        //var yearStart = new DateTime(2023, 1, 1);

        //var teachers = await _context
        //    .Set<Staff>()
        //    .ToListAsync();

        //var awards = await _context
        //    .Set<StudentAward>()
        //    .Where(award =>
        //        award.AwardedOn > yearStart)
        //    .ToListAsync();

        //var groupedAwards = awards.GroupBy(entry => entry.StudentId);

        //foreach (var awardGroup in groupedAwards)
        //{
        //    var student = await _context.Set<Student>().FirstOrDefaultAsync(entry => entry.StudentId == awardGroup.Key);

        //    foreach (var award in awardGroup)
        //    {
        //        var teacher = teachers.FirstOrDefault(entry => entry.StaffId == award.TeacherId);

        //        var certificate = await _context.Set<StoredFile>().AnyAsync(entry => entry.LinkId == award.Id.ToString() && entry.LinkType == StoredFile.AwardCertificate);

        //        Awards.Add(new()
        //        {
        //            Id = award.Id,
        //            StudentName = student.DisplayName,
        //            StudentGrade = student.CurrentGrade,
        //            TeacherName = teacher?.DisplayName,
        //            Type = award.Type,
        //            Category = award.Category,
        //            AwardedOn = award.AwardedOn,
        //            HasCertificate = certificate
        //        });
        //    }
        //}
    }

    public async Task OnGetCheckAwards(CancellationToken cancellationToken = default)
    {
        await _rollMarkingReportJob.StartJob(Guid.NewGuid(), cancellationToken);
    }

    public async Task<IActionResult> OnGetAttemptDownload(string id)
    {
        var file = await _context.Set<StoredFile>().FirstOrDefaultAsync(entry => entry.LinkId == id && entry.LinkType == StoredFile.AwardCertificate);

        if (file is null)
            return Page();

        return File(file.FileData, file.FileType, file.Name);
    }
}
