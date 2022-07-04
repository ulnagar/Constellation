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
        public List<StudentAwardCalculations> Awards { get; set; } = new();

        public class StudentAwardCalculations
        {
            public string StudentId { get; set; }
            public string StudentName { get; set; }
            public string StudentGrade { get; set; }
            public decimal AwardedAstras { get; set; }
            public decimal AwardedStellars { get; set; }
            public decimal AwardedGalaxies { get; set; }
            public decimal AwardedUniversals { get; set; }
            public decimal CalculatedStellars { get; set; }
            public decimal CalculatedGalaxies { get; set; }
            public decimal CalculatedUniversals { get; set; }
        }

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
                var entry = new StudentAwardCalculations();
                
                var student = await _mediator.Send(new GetStudentWithAwardQuery { StudentId = group.Key });
                entry.StudentId = student.StudentId;
                entry.StudentName = student.DisplayName;
                entry.StudentGrade = student.CurrentGrade.AsName();

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
                
                entry.AwardedAstras = group.Count(award => award.AwardType == "Astra Award");
                entry.AwardedStellars = group.Count(award => award.AwardType == "Stellar Award");
                entry.AwardedGalaxies = group.Count(award => award.AwardType == "Galaxy Medal");
                entry.AwardedUniversals = group.Count(award => award.AwardType == "Aurora Universal Achiever");

                entry.CalculatedStellars = Math.Floor(entry.AwardedAstras / 5);
                entry.CalculatedGalaxies = Math.Floor(entry.AwardedAstras / 25);
                entry.CalculatedUniversals = Math.Floor(entry.AwardedAstras / 125);

                Awards.Add(entry);
            }

            return Page();
        }
    }
}
