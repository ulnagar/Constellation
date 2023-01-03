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
    public class GetStudentFamilyEmailAddressesQueryHandler : IRequestHandler<GetStudentFamilyEmailAddressesQuery, ICollection<string>>
    {
        private readonly IAppDbContext _context;
        private readonly ISentralGatewayConfiguration _settings;

        public GetStudentFamilyEmailAddressesQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public GetStudentFamilyEmailAddressesQueryHandler(IAppDbContext context, ISentralGatewayConfiguration settings)
        {
            _context = context;
            _settings = settings;
        }

        public async Task<ICollection<string>> Handle(GetStudentFamilyEmailAddressesQuery request, CancellationToken cancellationToken)
        {
            var emailAddresses = new List<string>();

            var studentFamily = await _context.StudentFamilies
                .Where(family => family.Students.Any(student => student.StudentId == request.StudentId))
                .FirstOrDefaultAsync(cancellationToken);

            if (studentFamily == null)
                return emailAddresses;

            switch (_settings?.ContactPreference)
            {
                case ISentralGatewayConfiguration.ContactPreferenceOptions.MotherFirstThenFather:
                    if (!string.IsNullOrWhiteSpace(studentFamily?.Parent2?.EmailAddress))
                    {
                        emailAddresses.Add(studentFamily.Parent2.EmailAddress);
                    }
                    else if (!string.IsNullOrWhiteSpace(studentFamily?.Parent1?.EmailAddress))
                    {
                        emailAddresses.Add(studentFamily.Parent1.EmailAddress);
                    }

                    break;
                case ISentralGatewayConfiguration.ContactPreferenceOptions.FatherFirstThenMother:
                    if (!string.IsNullOrWhiteSpace(studentFamily?.Parent1?.EmailAddress))
                    {
                        emailAddresses.Add(studentFamily.Parent1.EmailAddress);
                    }
                    else if (!string.IsNullOrWhiteSpace(studentFamily?.Parent2?.EmailAddress))
                    {
                        emailAddresses.Add(studentFamily.Parent2.EmailAddress);
                    }

                    break;
                case ISentralGatewayConfiguration.ContactPreferenceOptions.BothParentsIfPresent:
                    if (!string.IsNullOrWhiteSpace(studentFamily?.Parent2?.EmailAddress))
                    {
                        emailAddresses.Add(studentFamily.Parent2.EmailAddress);
                    }

                    if (!string.IsNullOrWhiteSpace(studentFamily?.Parent1?.EmailAddress))
                    {
                        emailAddresses.Add(studentFamily.Parent1.EmailAddress);
                    }

                    break;
                default:
                    if (!string.IsNullOrWhiteSpace(studentFamily?.Parent2?.EmailAddress))
                    {
                        emailAddresses.Add(studentFamily.Parent2.EmailAddress);
                    }

                    if (!string.IsNullOrWhiteSpace(studentFamily?.Parent1?.EmailAddress))
                    {
                        emailAddresses.Add(studentFamily.Parent1.EmailAddress);
                    }

                    break;
            }

            return emailAddresses.Distinct().ToList();
        }
    }
}
