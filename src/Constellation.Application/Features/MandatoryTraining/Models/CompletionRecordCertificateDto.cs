namespace Constellation.Application.Features.MandatoryTraining.Models;

using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;

public class CompletionRecordCertificateDto : IMapFrom<StoredFile>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FileType { get; set; }
}
