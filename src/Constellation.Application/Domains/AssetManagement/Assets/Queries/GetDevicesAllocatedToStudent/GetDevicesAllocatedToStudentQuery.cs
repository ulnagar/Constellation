namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetDevicesAllocatedToStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetDevicesAllocatedToStudentQuery(
    StudentId StudentId)
    : IQuery<List<StudentDeviceResponse>>;