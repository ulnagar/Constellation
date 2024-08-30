namespace Constellation.Application.Assets.GetDevicesAllocatedToStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetDevicesAllocatedToStudentQuery(
    StudentId StudentId)
    : IQuery<List<StudentDeviceResponse>>;