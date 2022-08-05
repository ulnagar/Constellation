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
    public class SchoolContactService : ISchoolContactService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SchoolContactService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceOperationResult<SchoolContact>> UpdateContact(SchoolContactDto schoolContactResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<SchoolContact>();

            // Validate entries
            if (schoolContactResource.Id == null)
            {
                result.Success = false;
                result.Errors.Add($"A contact with that ID already exists!");
                return result;
            }

            var schoolContact = await _unitOfWork.SchoolContacts.ForEditAsync(schoolContactResource.Id.Value);
            if (schoolContact == null)
            {
                result.Success = false;
                result.Errors.Add($"A contact with that ID cannot be found!");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(schoolContactResource.FirstName))
                    schoolContact.FirstName = schoolContactResource.FirstName;

                if (!string.IsNullOrWhiteSpace(schoolContactResource.LastName))
                    schoolContact.LastName = schoolContactResource.LastName;

                if (!string.IsNullOrWhiteSpace(schoolContactResource.EmailAddress))
                    schoolContact.EmailAddress = schoolContactResource.EmailAddress;

                if (!string.IsNullOrWhiteSpace(schoolContactResource.PhoneNumber))
                    schoolContact.PhoneNumber = schoolContactResource.PhoneNumber;

                result.Success = true;
                result.Entity = schoolContact;
            }

            return result;
        }

        public async Task UndeleteContact(int id)
        {
            var contact = await _unitOfWork.SchoolContacts.ForEditAsync(id);

            if (contact.IsDeleted)
            {
                contact.IsDeleted = false;
                contact.DateDeleted = null;
            }
        }

        public async Task RemoveContact(int? id)
        {
            // Validate entries
            if (id == null) return;

            var contact = await _unitOfWork.SchoolContacts.ForEditAsync(id.Value);

            if (contact == null)
                return;

            contact.IsDeleted = true;
            contact.DateDeleted = DateTime.Now;
        }


        public async Task<ServiceOperationResult<SchoolContactRole>> UpdateRole(SchoolContactRoleDto schoolContactRoleResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<SchoolContactRole>();

            // Validate entries
            var contactRole = await _unitOfWork.SchoolContactRoles.ForEdit(schoolContactRoleResource.Id);
            if (contactRole == null)
            {
                result.Success = false;
                result.Errors.Add($"A contact role with that ID cannot be found!");
            }
            else
            {
                contactRole.SchoolContactId = schoolContactRoleResource.SchoolContactId;

                if (!string.IsNullOrWhiteSpace(schoolContactRoleResource.Role))
                    contactRole.Role = schoolContactRoleResource.Role;

                if (!string.IsNullOrWhiteSpace(schoolContactRoleResource.SchoolCode))
                    contactRole.SchoolCode = schoolContactRoleResource.SchoolCode;

                result.Success = true;
                result.Entity = contactRole;
            }

            return result;
        }

        public async Task RemoveRole(int id)
        {
            // Validate entries
            var contactRole = await _unitOfWork.SchoolContactRoles.ForEdit(id);

            if (contactRole == null)
                return;

            contactRole.IsDeleted = true;
            contactRole.DateDeleted = DateTime.Now;
        }
    }
}
