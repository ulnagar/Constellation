namespace Constellation.Application.Courses.GetCourseDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed record GetCourseDetailsQuery(
    CourseId CourseId)
    : IQuery<CourseDetailsResponse>;