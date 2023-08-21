namespace Constellation.Core.Models;

using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Models.Subjects;
using System;
using System.Collections.Generic;

public class Staff
{
    public string StaffId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PortalUsername { get; set; }
    public string AdobeConnectPrincipalId { get; set; }
    public virtual List<FacultyMembership> Faculties { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public DateTime? DateEntered { get; set; } = DateTime.Now;
    public string SchoolCode { get; set; }
    public virtual School School { get; set; }
    public string DisplayName => FirstName + " " + LastName;
    public string EmailAddress => PortalUsername + "@det.nsw.edu.au";
    public bool IsShared { get; set; }
    public virtual List<Session> CourseSessions { get; set; } = new();
    public virtual List<TeacherAdobeConnectOperation> AdobeConnectOperations { get; set; } = new();
    public virtual List<TeacherAdobeConnectGroupOperation> AdobeConnectGroupOperations { get; set; } = new();
    public virtual List<TeacherMSTeamOperation> MSTeamOperations { get; set; } = new();
    public virtual List<TrainingCompletion> TrainingCompletionRecords { get; set; } = new();
}