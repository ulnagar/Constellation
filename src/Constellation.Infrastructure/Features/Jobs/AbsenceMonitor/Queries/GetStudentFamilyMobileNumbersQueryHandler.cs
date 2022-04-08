using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.GatewayConfigurations;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Jobs.AbsenceMonitor.Queries
{
    public class GetStudentFamilyMobileNumbersQueryHandler : IRequestHandler<GetStudentFamilyMobileNumbersQuery, ICollection<string>>
    {
        private readonly IAppDbContext _context;
        private readonly ISentralGatewayConfiguration _settings;

        public GetStudentFamilyMobileNumbersQueryHandler(IAppDbContext context, ISentralGatewayConfiguration settings)
        {
            _context = context;
            _settings = settings;
        }

        public async Task<ICollection<string>> Handle(GetStudentFamilyMobileNumbersQuery request, CancellationToken cancellationToken)
        {
            var studentFamily = await _context.StudentFamilies
                .Where(family => family.Students.Any(student => student.StudentId == request.StudentId))
                .FirstOrDefaultAsync(cancellationToken);

            var phoneNumbers = new List<string>();

            switch (_settings.ContactPreference)
            {
                case ISentralGatewayConfiguration.ContactPreferenceOptions.MotherFirstThenFather:
                    if (studentFamily.Parent2.MobileNumber.StartsWith("04"))
                    {
                        phoneNumbers.Add(studentFamily.Parent2.MobileNumber);
                    }
                    else if (studentFamily.Parent1.MobileNumber.StartsWith("04"))
                    {
                        phoneNumbers.Add(studentFamily.Parent1.MobileNumber);
                    }

                    break;
                case ISentralGatewayConfiguration.ContactPreferenceOptions.FatherFirstThenMother:
                    if (studentFamily.Parent1.MobileNumber.StartsWith("04"))
                    {
                        phoneNumbers.Add(studentFamily.Parent1.MobileNumber);
                    }
                    else if (studentFamily.Parent2.MobileNumber.StartsWith("04"))
                    {
                        phoneNumbers.Add(studentFamily.Parent2.MobileNumber);
                    }

                    break;
                case ISentralGatewayConfiguration.ContactPreferenceOptions.BothParentsIfPresent:
                    if (studentFamily.Parent2.MobileNumber.StartsWith("04"))
                    {
                        phoneNumbers.Add(studentFamily.Parent2.MobileNumber);
                    }

                    if (studentFamily.Parent1.MobileNumber.StartsWith("04"))
                    {
                        phoneNumbers.Add(studentFamily.Parent1.MobileNumber);
                    }

                    break;
            }

            return phoneNumbers.Distinct().ToList();
        }
    }
}
