﻿namespace Constellation.Application.Models.Auth;

// ReSharper disable InconsistentNaming

public static class AuthPolicies
{
    public const string CanViewTrainingCompletionRecord = "CanViewTrainingCompletionRecord";
    public const string CanEditTrainingModuleContent = "CanManageTrainingModule";
    public const string CanViewTrainingModuleContent = "CanViewTrainingModuleContent";
    public const string CanViewTrainingModuleContentDetails = "CanViewTrainingModuleContentDetails";
    public const string CanRunTrainingModuleReports = "CanRunTrainingModuleReports";
    public const string IsStaffMember = "IsStaffMember";
    public const string IsExecutive = "IsExecutive";
    public const string CanEditFaculties = "CanEditFaculties";
    public const string CanViewFacultyDetails = "CanEditFacultyDetails";
    public const string CanViewGroupTutorials = "CanViewGroupTutorials";
    public const string CanEditGroupTutorials = "CanEditGroupTutorials";
    public const string CanSubmitGroupTutorialRolls = "CanSubmitGroupTutorialRolls";
    public const string IsSiteAdmin = "IsSiteAdmin";
    public const string CanViewCovers = "CanViewCovers";
    public const string CanEditCovers = "CanEditCovers";
    public const string CanEditCasuals = "CanEditCasuals";
    public const string CanEditStudents = "CanEditStudents";
    public const string CanManageAbsences = "CanManageAbsences";
    public const string CanAddAwards = "CanAddAwards";
    public const string CanManageAwards = "CanManageAwards";
    public const string CanViewAwardNominations = "CanViewAwards";
    public const string CanManageSciencePracs = "CanManageSciencePracs";
    public const string CanEditSubjects = "CanEditSubjects";
    public const string CanManageCompliance = "CanManageCompliance";
    public const string CanEditStaff = "CanEditStaff";
    public const string CanManageWorkflows = "CanManageWorkflows";
    public const string CanEditWorkFlowAction = "CanEditWorkFlowAction";
    public const string CanManageSchoolContacts = "CanManageSchoolContacts";
    public const string CanManageAssets = "CanManageAssets";
    public const string CanEditSchools = "CanEditSchools";
    public const string CanManageReports = "CanManageReports";

    public const string IsSchoolContact = "IsSchoolContact";

    public const string IsParent = "IsParent";

    public const string IsStudent = "IsStudent";
}