using Constellation.Core.Models;

namespace Constellation.Application.DTOs
{
    public class SchoolContactRoleDto
    {
        public int Id { get; set; }
        public int SchoolContactId { get; set; }
        public SchoolContact SchoolContact { get; set; }
        public string Role { get; set; }
        public string SchoolCode { get; set; }
        public string SchoolName { get; set; }
    }
}