namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Reports;

using Application.Models.Auth;
using Constellation.Application.Assets.ImportAssetsFromFile;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class ImportModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public ImportModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Reports;
    [ViewData] public string PageTitle => "Asset Import";

    [BindProperty]
    [AllowExtensions(FileExtensions: "xlsx", ErrorMessage = "You can only upload XLSX files")]
    public IFormFile? UploadFile { get; set; }

    public List<ImportAssetStatusDto> Statuses { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostImportFile()
    {
        if (UploadFile is null)
        {
            ModelState.AddModelError("UploadFile", "You must select a file to upload and process");

            return Page();
        }

        try
        {
            await using MemoryStream target = new();
            await UploadFile.CopyToAsync(target);

            Result<List<ImportAssetStatusDto>> request = await _mediator.Send(new ImportAssetsFromFileCommand(target));

            if (request.IsFailure)
            {
                ModalContent = new ErrorDisplay(request.Error);

                return Page();
            }

            if (request.Value.Count > 0)
            {
                Statuses = request.Value;

                return Page();
            }
        }
        catch (Exception ex)
        {
            ModalContent = new ErrorDisplay(new(ex.Source, ex.Message));

            return Page();
        }

        return RedirectToPage("/Equipment/Assets/Index", new { area = "Staff" });
    }
}