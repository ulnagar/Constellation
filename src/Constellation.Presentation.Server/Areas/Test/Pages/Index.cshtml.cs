using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Test.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppDbContext _context;

        public IndexModel(IUnitOfWork unitOfWork, IAppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            await GetClasses(_unitOfWork);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var absences = await _context.Absences
                .Include(absence => absence.Responses)
                .Where(absence => 
                    absence.ExternallyExplained && 
                    absence.ExternalExplanation != null && 
                    absence.Responses.Any(response => response.VerificationStatus != AbsenceResponse.Pending))
                .ToListAsync();

            foreach (var absence in absences)
            {
                var response = new AbsenceResponse
                {
                    Explanation = absence.ExternalExplanation,
                    From = absence.ExternalExplanationSource,
                    ReceivedAt = absence.LastSeen,
                    Type = AbsenceResponse.System
                };

                absence.Responses.Add(response);

                await _context.SaveChangesAsync(new CancellationToken());
            }

            return Page();
        }
    }
}
