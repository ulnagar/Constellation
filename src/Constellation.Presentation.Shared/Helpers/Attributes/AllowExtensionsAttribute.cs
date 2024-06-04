namespace Constellation.Presentation.Shared.Helpers.Attributes;

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class AllowExtensionsAttribute : ValidationAttribute
{
    private List<string> AllowedExtensions { get; set; }

    /// <summary>
    /// Limit the uploaded files to one or more file extensions
    /// </summary>
    /// <param name="FileExtensions">
    /// A comma separated list of file extensions to be allowed.
    /// E.g. "pdf,xlsx"
    /// </param>
    public AllowExtensionsAttribute(string FileExtensions)
    {
        AllowedExtensions = FileExtensions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public override bool IsValid(object value)
    {
        IFormFile file = value as IFormFile;
        bool isValid = true;

        if (file != null)
        {
            var fileName = file.FileName;
            isValid = AllowedExtensions.Any(y => fileName.EndsWith(y));
        }

        return isValid;
    }
}