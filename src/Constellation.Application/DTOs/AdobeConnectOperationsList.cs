using Constellation.Core.Models;
using System.Collections.Generic;

namespace Constellation.Application.DTOs
{
    public class AdobeConnectOperationsList
    {
        public ICollection<StudentAdobeConnectOperation> StudentOperations { get; set; }
        public ICollection<TeacherAdobeConnectOperation> TeacherOperations { get; set; }
        public ICollection<CasualAdobeConnectOperation> CasualOperations { get; set; }
        public ICollection<TeacherAdobeConnectGroupOperation> TeacherGroupOperations { get; set; }

        public AdobeConnectOperationsList()
        {
            StudentOperations = new List<StudentAdobeConnectOperation>();
            TeacherOperations = new List<TeacherAdobeConnectOperation>();
            CasualOperations = new List<CasualAdobeConnectOperation>();
            TeacherGroupOperations = new List<TeacherAdobeConnectGroupOperation>();
        }
    }
}