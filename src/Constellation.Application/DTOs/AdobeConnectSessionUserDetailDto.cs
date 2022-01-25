using System;

namespace Constellation.Application.DTOs
{
    public class AdobeConnectSessionUserDetailDto
    {
        public string Name { get; set; }
        public string Login { get; set; }
        public DateTime? LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
    }
}
