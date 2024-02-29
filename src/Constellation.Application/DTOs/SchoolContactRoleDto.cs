using Constellation.Core.Models;

namespace Constellation.Application.DTOs
{
    using Core.Models.SchoolContacts.Identifiers;
    using System;

    public class SchoolContactRoleDto
    {
        public Guid SchoolContactId { get; set; }
        public string SchoolContactName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string SchoolCode { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
    }
}