namespace Constellation.Application.Assets.GetDevicesAllocatedToStudent;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetDevicesAllocatedToStudentQuery(
    string StudentId)
    : IQuery<List<StudentDeviceResponse>>;