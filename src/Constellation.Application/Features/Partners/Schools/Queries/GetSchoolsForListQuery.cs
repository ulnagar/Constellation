using Constellation.Application.Features.Partners.Schools.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Partners.Schools.Queries
{
    public class GetSchoolsForListQuery : IRequest<List<SchoolForList>>
    {
        public bool? WithStudents { get; set; }
        public bool? WithStaff { get; set; }
    }
}
