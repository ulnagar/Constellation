using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC Operations
    public class CasualService : ICasualService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICoverService _coverService;

        public CasualService(IUnitOfWork unitOfWork, ICoverService coverService)
        {
            _unitOfWork = unitOfWork;
            _coverService = coverService;
        }

        public async Task<ServiceOperationResult<Casual>> CreateCasual(CasualDto casualResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<Casual>();

            if (!await _unitOfWork.Casuals.AnyWithId(casualResource.Id) || !await _unitOfWork.Casuals.AnyWithPortalUsername(casualResource.PortalUsername))
            {
                var casual = new Casual
                {
                    FirstName = casualResource.FirstName,
                    LastName = casualResource.LastName,
                    PortalUsername = casualResource.PortalUsername,
                    SchoolCode = casualResource.SchoolCode,
                    AdobeConnectPrincipalId = casualResource.AdobeConnectPrincipalId
                };

                _unitOfWork.Add(casual);

                result.Success = true;
                result.Entity = casual;
            }
            else
            {
                result.Success = false;
                result.Errors.Add($"A casual with those details already exists!");
            }

            return result;
        }

        public async Task<ServiceOperationResult<Casual>> UpdateCasual(int casualId, CasualDto casualResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<Casual>();

            // Validate entries
            var casual = await _unitOfWork.Casuals.ForEditAsync(casualId);

            if (casual == null)
            {
                result.Success = false;
                result.Errors.Add($"A casual with that ID could not be found!");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(casualResource.FirstName))
                    casual.FirstName = casualResource.FirstName;

                if (!string.IsNullOrWhiteSpace(casualResource.LastName))
                    casual.LastName = casualResource.LastName;

                if (!string.IsNullOrWhiteSpace(casualResource.PortalUsername))
                    casual.PortalUsername = casualResource.PortalUsername;

                if (!string.IsNullOrWhiteSpace(casualResource.SchoolCode))
                    casual.SchoolCode = casualResource.SchoolCode;

                if (!string.IsNullOrWhiteSpace(casualResource.AdobeConnectPrincipalId))
                    casual.AdobeConnectPrincipalId = casualResource.AdobeConnectPrincipalId;

                result.Success = true;
                result.Entity = casual;
            }

            return result;
        }

        public async Task RemoveCasual(int casualId)
        {
            // Validate entries
            var casual = await _unitOfWork.Casuals.ForEditAsync(casualId);

            if (casual == null)
                return;

            var outstandingCovers = await _unitOfWork.Covers.OutstandingForCasual(casualId);

            // Remove all current casual covers
            foreach (var cover in outstandingCovers)
            {
                // TODO: Remove the cover, including operations if necessary
                await _coverService.RemoveCasualCover(cover.Id);
            }

            casual.IsDeleted = true;
            casual.DateDeleted = DateTime.Now;
        }
    }
}
