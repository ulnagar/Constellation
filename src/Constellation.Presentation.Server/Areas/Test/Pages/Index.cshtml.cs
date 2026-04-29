namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Domains.Students.Models;
using Application.Models.Auth;
using BaseModels;
using Core.Abstractions.Services;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IStudentRepository _studentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        IStudentRepository studentRepository,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _studentRepository = studentRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [BindProperty]
    public string Search { get; set; }

    public List<Student> Students { get; set; } = [];

    public async Task OnGet()
    {

    }

    public async Task<IActionResult> OnPostSearch()
    {
        if (Search is null || Search.Length == 0)
            return Page();

        Students = await _studentRepository.GetFuzzyNameSearch(Search);

        return Page();
    }
}