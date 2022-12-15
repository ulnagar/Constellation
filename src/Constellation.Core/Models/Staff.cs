namespace Constellation.Core.Models;

using Constellation.Core.Models.MandatoryTraining;
using System;
using System.Collections.Generic;

public class Staff
{
    public string StaffId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PortalUsername { get; set; } = string.Empty;
    public string AdobeConnectPrincipalId { get; set; } = string.Empty;
    public virtual List<FacultyMembership> Faculties { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }
    public DateTime? DateEntered { get; set; } = DateTime.Now;
    public string SchoolCode { get; set; } = string.Empty;
    public virtual School? School { get; set; }
    public string DisplayName => FirstName + " " + LastName;
    public string EmailAddress => PortalUsername + "@det.nsw.edu.au";
    public bool IsShared { get; set; }
    public virtual List<OfferingSession> CourseSessions { get; set; } = new();
    public virtual List<TeacherClassCover> ClassCovers { get; set; } = new();
    public virtual List<TeacherAdobeConnectOperation> AdobeConnectOperations { get; set; } = new();
    public virtual List<TeacherAdobeConnectGroupOperation> AdobeConnectGroupOperations { get; set; } = new();
    public virtual List<TeacherMSTeamOperation> MSTeamOperations { get; set; } = new();
    public virtual List<ClassworkNotification> ClassworkNotifications { get; set; } = new();
    public virtual List<TrainingCompletion> TrainingCompletionRecords { get; set; } = new();
}