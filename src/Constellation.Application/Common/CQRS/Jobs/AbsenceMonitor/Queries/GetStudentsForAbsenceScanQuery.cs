using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Jobs.AbsenceMonitor.Queries
{
    public class GetStudentsForAbsenceScanQuery : IRequest<ICollection<StudentForAbsenceScan>>
    {
    }

    public class StudentForAbsenceScan : IMapFrom<Student>
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName => $"{FirstName} {LastName}";
        public Grade CurrentGrade { get; set; }
        public string StudentSentralId { get; set; }
        public DateTime? AbsenceNotificationStartDate { get; set; }
    }

    public class GetStudentsForAbsenceScanQueryHandler : IRequestHandler<GetStudentsForAbsenceScanQuery, ICollection<StudentForAbsenceScan>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentsForAbsenceScanQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StudentForAbsenceScan>> Handle(GetStudentsForAbsenceScanQuery request, CancellationToken cancellationToken)
        {
            var students = await _context.Students
                .Where(student => student.IncludeInAbsenceNotifications && !student.IsDeleted)
                .ProjectTo<StudentForAbsenceScan>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return students;
        }
    }
}
