USE CONSTELLATION_TEST
GO

-- CASUALS TABLE --
ALTER TABLE CASUALS NOCHECK CONSTRAINT ALL;
SET IDENTITY_INSERT CASUALS ON;

INSERT INTO CASUALS (Id, FirstName, LastName, PortalUsername, IsDeleted, DateDeleted, DateEntered, AdobeConnectPrincipalId, SchoolCode)
SELECT Id, FirstName, LastName, PortalUsername, IsDeleted, DateDeleted AS datetime2, DateEntered AS datetime2, AdobeConnectPrincipalId, SchoolCode FROM [ACOS_TEST].[dbo].[Casuals]
WHERE 1 = 1;

SET IDENTITY_INSERT CASUALS OFF;
ALTER TABLE CASUALS WITH CHECK CHECK CONSTRAINT ALL;

-- SCHOOLCONTACTROLE TABLE --
ALTER TABLE SCHOOLCONTACTROLE NOCHECK CONSTRAINT ALL;
SET IDENTITY_INSERT SCHOOLCONTACTROLE ON;

INSERT INTO SCHOOLCONTACTROLE (Id, SchoolContactId, Role, SchoolCode, IsDeleted, DateEntered, DateDeleted)
SELECT Id, SchoolContactId, Role, SchoolCode, IsDeleted, DateEntered AS datetime2, DateDeleted AS datetime2 FROM [ACOS_TEST].[dbo].[SchoolContactRole]
WHERE 1 = 1;

SET IDENTITY_INSERT SCHOOLCONTACTROLE OFF;
ALTER TABLE SCHOOLCONTACTROLE WITH CHECK CHECK CONSTRAINT ALL;

-- STUDENTS TABLE --
ALTER TABLE STUDENTS NOCHECK CONSTRAINT ALL;

INSERT INTO STUDENTS (StudentId, FirstName, LastName, PortalUsername, AdobeConnectPrincipalId, SentralStudentId, CurrentGrade, EnrolledGrade, Gender, IsDeleted, DateDeleted, DateEntered, SchoolCode, IncludeInAbsenceNotifications, AbsenceNotificationStartDate)
SELECT StudentId, FirstName, LastName, PortalUsername, AdobeConnectPrincipalId, SentralStudentId, CurrentGrade, EnrolledGrade, Gender, IsDeleted, DateDeleted AS datetime2, DateEntered AS datetime2, SchoolCode, IncludeInAbsenceNotifications, AbsenceNotificationStartDate AS datetime2 FROM [ACOS_TEST].[dbo].[Students]
WHERE 1 = 1;

ALTER TABLE STUDENTS WITH CHECK CHECK CONSTRAINT ALL;

-- STAFF TABLE --
ALTER TABLE STAFF NOCHECK CONSTRAINT ALL;

INSERT INTO STAFF (StaffId, FirstName, LastName, PortalUsername, AdobeConnectPrincipalId, Faculty, IsDeleted, DateDeleted, DateEntered, SchoolCode)
SELECT StaffId, FirstName, LastName, PortalUsername, AdobeConnectPrincipalId, Faculty, IsDeleted, DateDeleted AS datetime2, DateEntered AS datetime2, SchoolCode FROM [ACOS_TEST].[dbo].Staff
WHERE 1 = 1;

ALTER TABLE STAFF WITH CHECK CHECK CONSTRAINT ALL;

-- DEVICENOTES TABLE --
ALTER TABLE DEVICENOTES NOCHECK CONSTRAINT ALL;
SET IDENTITY_INSERT DEVICENOTES ON;

INSERT INTO DEVICENOTES (Id, SerialNumber, DateEntered, Details)
SELECT Id, SerialNumber, DateEntered AS datetime2, Details FROM [ACOS_TEST].[dbo].[DeviceNotes]
WHERE 1 = 1;

SET IDENTITY_INSERT DEVICENOTES OFF;
ALTER TABLE DEVICENOTES WITH CHECK CHECK CONSTRAINT ALL;

-- LESSONROLLS TABLE --
ALTER TABLE LESSONROLLS NOCHECK CONSTRAINT ALL;

INSERT INTO LESSONROLLS (Id, LessonId, SchoolCode, SchoolContactId, LessonDate, SubmittedDate, Comment, Status)
SELECT Id, LessonId, SchoolCode, SchoolContactId, LessonDate AS datetime2, SubmittedDate AS datetime2, Comment, Status FROM [ACOS_TEST].[dbo].[LessonRolls]
WHERE 1 = 1;

ALTER TABLE LESSONROLLS WITH CHECK CHECK CONSTRAINT ALL;

GO