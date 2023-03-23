using Constellation.Application.DTOs;
using Constellation.Application.Features.API.Operations.Commands;
using Constellation.Application.Features.API.Operations.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Server.Areas.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.API.Controllers
{
    [Route("api/v1/Operations")]
    [ApiController]
    public class OperationsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ICasualRepository _casualRepository;

        public OperationsController(IUnitOfWork unitOfWork, IMediator mediator, ICasualRepository casualRepository)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _casualRepository = casualRepository;
        }

        // GET api/Operations/Due
        [Route("Due")]
        public async Task<IEnumerable<TeamsOperation>> GetDue()
        {
            var operations = await _unitOfWork.MSTeamOperations.ToProcess();

            return await BuildOperations(operations);
        }


        // GET api/Operations/Overdue
        [Route("Overdue")]
        public async Task<IEnumerable<TeamsOperation>> GetOverdue()
        {
            var operations = await _unitOfWork.MSTeamOperations.OverdueToProcess();

            return await BuildOperations(operations);
        }

        private async Task<ICollection<TeamsOperation>> BuildOperations(MSTeamOperationsList operations)
        {
            var returnData = new List<TeamsOperation>();

            foreach (var operation in operations.StudentOperations)
            {
                var teamOperation = new TeamsOperation
                {
                    Id = operation.Id,
                    TeamName = $"AC - {operation.Offering.EndDate:yyyy} - {operation.Offering.Name}",
                    UserEmail = operation.Student.EmailAddress
                };

                switch (operation.Action)
                {
                    case MSTeamOperationAction.Add:
                        teamOperation.Action = "Add";
                        break;
                    case MSTeamOperationAction.Remove:
                        teamOperation.Action = "Remove";
                        break;
                    case MSTeamOperationAction.Promote:
                        teamOperation.Action = "Promote";
                        break;
                    case MSTeamOperationAction.Demote:
                        teamOperation.Action = "Demote";
                        break;
                }

                switch (operation.PermissionLevel)
                {
                    case MSTeamOperationPermissionLevel.Member:
                        teamOperation.Role = "Member";
                        break;
                    case MSTeamOperationPermissionLevel.Owner:
                        teamOperation.Role = "Owner";
                        break;
                }

                teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.Offering.Name, Year = operation.Offering.EndDate.Year.ToString() });

                returnData.Add(teamOperation);
            }

            foreach (var operation in operations.TeacherOperations)
            {
                var teamOperation = new TeamsOperation
                {
                    Id = operation.Id,
                    TeamName = $"AC - {operation.Offering.EndDate:yyyy} - {operation.Offering.Name}",
                    UserEmail = operation.Staff.EmailAddress
                };

                switch (operation.Action)
                {
                    case MSTeamOperationAction.Add:
                        teamOperation.Action = "Add";
                        break;
                    case MSTeamOperationAction.Remove:
                        teamOperation.Action = "Remove";
                        break;
                    case MSTeamOperationAction.Promote:
                        teamOperation.Action = "Promote";
                        break;
                    case MSTeamOperationAction.Demote:
                        teamOperation.Action = "Demote";
                        break;
                }

                switch (operation.PermissionLevel)
                {
                    case MSTeamOperationPermissionLevel.Member:
                        teamOperation.Role = "Member";
                        break;
                    case MSTeamOperationPermissionLevel.Owner:
                        teamOperation.Role = "Owner";
                        break;
                }

                teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.Offering.Name, Year = operation.Offering.EndDate.Year.ToString() });

                returnData.Add(teamOperation);
            }

            foreach (var operation in operations.CasualOperations)
            {
                var casual = await _casualRepository.GetById(CasualId.FromValue(operation.CasualId));

                var teamOperation = new TeamsOperation
                {
                    Id = operation.Id,
                    TeamName = $"AC - {operation.Offering.EndDate:yyyy} - {operation.Offering.Name}",
                    UserEmail = casual.EmailAddress
                };

                switch (operation.Action)
                {
                    case MSTeamOperationAction.Add:
                        teamOperation.Action = "Add";
                        break;
                    case MSTeamOperationAction.Remove:
                        teamOperation.Action = "Remove";
                        break;
                    case MSTeamOperationAction.Promote:
                        teamOperation.Action = "Promote";
                        break;
                    case MSTeamOperationAction.Demote:
                        teamOperation.Action = "Demote";
                        break;
                }

                switch (operation.PermissionLevel)
                {
                    case MSTeamOperationPermissionLevel.Member:
                        teamOperation.Role = "Member";
                        break;
                    case MSTeamOperationPermissionLevel.Owner:
                        teamOperation.Role = "Owner";
                        break;
                }

                teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.Offering.Name, Year = operation.Offering.EndDate.Year.ToString() });

                returnData.Add(teamOperation);
            }

            foreach (var operation in operations.GroupOperations)
            {
                var teamOperation = new TeamsOperation
                {
                    Id = operation.Id,
                    TeamName = $"AC - {operation.Offering.EndDate:yyyy} - {operation.Offering.Name}",
                    Action = "Group",
                    Faculty = operation.Faculty.ToString()
                };

                teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.Offering.Name, Year = operation.Offering.EndDate.Year.ToString() });

                returnData.Add(teamOperation);
            }

            foreach (var operation in operations.TutorialOperations)
            {
                var teamOperation = new TeamsOperation
                {
                    Id = operation.Id,
                    TeamName = $"AC - {operation.GroupTutorial.EndDate:yyyy} - {operation.GroupTutorial.Name}",
                    Action = "Group"
                };

                returnData.Add(teamOperation);
            }

            foreach (var operation in operations.EnrolmentOperations)
            {
                var teamOperation = new TeamsOperation
                {
                    Id = operation.Id,
                    TeamName = operation.TeamName,
                    UserEmail = operation.Student.EmailAddress,
                    AdditionalInformation = operation.Student.CurrentGrade.ToString()
                };

                switch (operation.Action)
                {
                    case MSTeamOperationAction.Add:
                        teamOperation.Action = "Add";
                        break;
                    case MSTeamOperationAction.Remove:
                        teamOperation.Action = "Remove";
                        break;
                    case MSTeamOperationAction.Promote:
                        teamOperation.Action = "Promote";
                        break;
                    case MSTeamOperationAction.Demote:
                        teamOperation.Action = "Demote";
                        break;
                }

                switch (operation.PermissionLevel)
                {
                    case MSTeamOperationPermissionLevel.Member:
                        teamOperation.Role = "Member";
                        break;
                    case MSTeamOperationPermissionLevel.Owner:
                        teamOperation.Role = "Owner";
                        break;
                }

                teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.TeamName, Year = operation.TeamName });

                returnData.Add(teamOperation);
            }

            foreach (var operation in operations.EmploymentOperations)
            {
                var teamOperation = new TeamsOperation
                {
                    Id = operation.Id,
                    TeamName = operation.TeamName,
                    UserEmail = operation.Staff.EmailAddress,
                    AdditionalInformation = "All"
                };

                switch (operation.Action)
                {
                    case MSTeamOperationAction.Add:
                        teamOperation.Action = "Add";
                        break;
                    case MSTeamOperationAction.Remove:
                        teamOperation.Action = "Remove";
                        break;
                    case MSTeamOperationAction.Promote:
                        teamOperation.Action = "Promote";
                        break;
                    case MSTeamOperationAction.Demote:
                        teamOperation.Action = "Demote";
                        break;
                }

                switch (operation.PermissionLevel)
                {
                    case MSTeamOperationPermissionLevel.Member:
                        teamOperation.Role = "Member";
                        break;
                    case MSTeamOperationPermissionLevel.Owner:
                        teamOperation.Role = "Owner";
                        break;
                }

                teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.TeamName, Year = operation.TeamName });

                returnData.Add(teamOperation);
            }

            foreach (var operation in operations.ContactOperations)
            {
                var teamOperation = new TeamsOperation
                {
                    Id = operation.Id,
                    TeamName = operation.TeamName,
                    UserEmail = operation.Contact.EmailAddress
                };

                switch (operation.Action)
                {
                    case MSTeamOperationAction.Add:
                        teamOperation.Action = "Add";
                        break;
                    case MSTeamOperationAction.Remove:
                        teamOperation.Action = "Remove";
                        break;
                    case MSTeamOperationAction.Promote:
                        teamOperation.Action = "Promote";
                        break;
                    case MSTeamOperationAction.Demote:
                        teamOperation.Action = "Demote";
                        break;
                }

                switch (operation.PermissionLevel)
                {
                    case MSTeamOperationPermissionLevel.Member:
                        teamOperation.Role = "Member";
                        break;
                    case MSTeamOperationPermissionLevel.Owner:
                        teamOperation.Role = "Owner";
                        break;
                }

                teamOperation.TeamId = await _mediator.Send(new GetTeamIdForOfferingQuery { ClassName = operation.TeamName, Year = operation.TeamName });

                returnData.Add(teamOperation);
            }

            return returnData;
        }

        // POST api/Operations/Complete
        [Route("Complete/{id}")]
        [HttpPost]
        public async Task Complete(int id)
        {
            var operation = await _unitOfWork.MSTeamOperations.ForMarkingCompleteOrCancelled(id);

            if (operation != null)
            {
                operation.Complete();
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
