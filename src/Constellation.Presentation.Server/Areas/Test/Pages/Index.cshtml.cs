namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.Contacts.Interfaces;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IStudentFlagCacheService _flagCache;
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IStudentFlagCacheService flagCache,
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _flagCache = flagCache;
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public List<string> Flags = [];
    public List<StudentId> StudentIds = [];

    [BindProperty(SupportsGet = true)]
    public string Flag { get; set; } = string.Empty;

    public async Task OnGet()
    {
        Flags = await _flagCache.GetFlags();

        if (!string.IsNullOrWhiteSpace(Flag))
        {
            StudentIds = await _flagCache.GetStudentsWithFlag(Flag);
        }
    }

}