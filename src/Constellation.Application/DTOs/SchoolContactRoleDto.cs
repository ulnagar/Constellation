using Constellation.Core.Models;

namespace Constellation.Application.DTOs
{
    public class SchoolContactRoleDto
    {
        public int Id { get; set; }
        public int SchoolContactId { get; set; }
        public string SchoolContactName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string SchoolCode { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
    }
}