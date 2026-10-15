namespace Constellation.Application.Domains.EnrolmentContext.Applications.Models;

using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentStudent;

public sealed record ApplicationImportContext(
    EnrolmentPeriod Period,
    EnrolmentStudent? ExistingStudent = null);