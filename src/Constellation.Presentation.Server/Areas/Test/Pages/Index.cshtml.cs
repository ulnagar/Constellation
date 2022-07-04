using Constellation.Application.Extensions;
using Constellation.Application.Features.Awards.Commands;
using Constellation.Application.Features.Awards.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Test.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ISentralGateway _sentralGateway;

        public IndexModel(IUnitOfWork unitOfWork, IMediator mediator, ISentralGateway sentralGateway)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _sentralGateway = sentralGateway;
        }

        public List<string> Messages { get; set; } = new();

        public async Task OnGetAsync()
        {
            await GetClasses(_unitOfWork);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var details = await _sentralGateway.GetAwardsReport();

            // Process individual students
            // Tally awards
            // Calculate expected award levels
            // Highlight discrepancies

            foreach (var group in details.GroupBy(detail => detail.StudentId))
            {
                var student = await _mediator.Send(new GetStudentWithAwardQuery { StudentId = group.Key });

                //foreach (var item in group)
                //{
                //    if (!student.Awards.Any(award => award.Type == item.AwardType && award.AwardedOn == item.AwardCreated))
                //    {
                //        await _mediator.Send(new CreateStudentAwardCommand
                //        {
                //            StudentId = student.StudentId,
                //            Category = item.AwardCategory,
                //            Type = item.AwardType,
                //            AwardedOn = item.AwardCreated
                //        });
                //    }
                //}

                //var astras = student.Awards.Count(award => award.Type == "Astra Award");
                //var stellars = student.Awards.Count(award => award.Type == "Stellar Award");
                //var galaxies = student.Awards.Count(award => award.Type == "Galaxy Medal");
                //var universals = student.Awards.Count(award => award.Type == "Aurora Universal Achiever"); 
                
                decimal astras = group.Count(award => award.AwardType == "Astra Award");
                decimal stellars = group.Count(award => award.AwardType == "Stellar Award");
                decimal galaxies = group.Count(award => award.AwardType == "Galaxy Medal");
                decimal universals = group.Count(award => award.AwardType == "Aurora Universal Achiever");

                decimal expectedStellars = Math.Floor(astras / 5);
                decimal expectedGalaxies = Math.Floor(astras / 25);
                decimal expectedUniversals = Math.Floor(astras / 125);

                if (universals != expectedUniversals)
                {
                    Messages.Add($"{student.DisplayName} ({student.CurrentGrade.AsName()}) - Incorrect number of Universal Achiever awards: has {universals} but should have {expectedUniversals}");
                }

                if (galaxies != expectedGalaxies)
                {
                    Messages.Add($"{student.DisplayName} ({student.CurrentGrade.AsName()}) - Incorrect number of Galaxy awards: has {galaxies} but should have {expectedGalaxies}");
                }

                if (stellars != expectedStellars)
                {
                    Messages.Add($"{student.DisplayName} ({student.CurrentGrade.AsName()}) - Incorrect number of Stellar awards: has {stellars} but should have {expectedStellars}");
                }
            }

            return Page();
        }
    }
}
