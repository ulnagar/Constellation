namespace Constellation.Presentation.Schools.Pages.Shared.Components.StudentPhoto;

using Core.Models.Attachments;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Students.Identifiers;
using Core.Shared;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class StudentPhotoViewComponent
    : ViewComponent
{
    private readonly IAttachmentService _attachmentService;

    public StudentPhotoViewComponent(
        IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    public async Task<IViewComponentResult> InvokeAsync(StudentId studentId)
    {
        Result<AttachmentResponse> image = await _attachmentService.GetAttachmentFile(AttachmentType.StudentPhoto, studentId.ToString());

        if (image.IsFailure)
        {
            return Content(string.Empty);
        }

        return View(image.Value);
    }
}
