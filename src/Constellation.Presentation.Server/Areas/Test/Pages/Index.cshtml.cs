namespace Constellation.Presentation.Server.Areas.Test.Pages;

using BaseModels;
using Constellation.Application.Assets.ImportAssetsFromFile;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Models;
using Core.Models.SchoolContacts.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly ISchoolContactRepository _contactRepository;


    public IndexModel(
        ISchoolContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }


    public string Message { get; set; } = string.Empty;

    public async Task OnGet()
    {
        var contact = await _contactRepository.GetByNameAndSchool("Timothy Lloyd", "2114", default);

        Message = contact?.EmailAddress;
    }
}