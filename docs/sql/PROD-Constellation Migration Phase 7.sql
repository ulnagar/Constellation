USE CONSTELLATION_TEST
GO

-- OFFERINGRESOURCES TABLE --
ALTER TABLE CLASSWORKNOTIFICATIONS NOCHECK CONSTRAINT ALL;

INSERT INTO CLASSWORKNOTIFICATIONS (Id, Description, StaffId, CompletedAt, GeneratedAt, OfferingId, AbsenceDate)
SELECT Id, Description, StaffId, CompletedAt, GeneratedAt, OfferingId, AbsenceDate FROM [ACOS].[dbo].ClassworkNotifications
WHERE 1 = 1;

ALTER TABLE CLASSWORKNOTIFICATIONS WITH CHECK CHECK CONSTRAINT ALL;

Go

ALTER TABLE ABSENCECLASSWORKNOTIFICATION NOCHECK CONSTRAINT ALL;

INSERT INTO ABSENCECLASSWORKNOTIFICATION (AbsencesId, ClassworkNotificationsId)
SELECT Absence_Id, Classworknotification_Id FROM [ACOS].[dbo].ClassworkNotificationAbsences
WHERE 1 = 1;

ALTER TABLE ABSENCECLASSWORKNOTIFICATION WITH CHECK CHECK CONSTRAINT ALL;

ALTER TABLE ClassCoverClassworkNotification NOCHECK CONSTRAINT ALL;

INSERT INTO ClassCoverClassworkNotification (ClassworkNotificationsId, CoversId)
SELECT ClassworkNotification_Id, ClassCover_Id FROM [ACOS].[dbo].ClassworkNotificationClassCovers
WHERE 1 = 1;

ALTER TABLE ClassCoverClassworkNotification WITH CHECK CHECK CONSTRAINT ALL;

ALTER TABLE ClassworkNotificationStaff NOCHECK CONSTRAINT ALL;

INSERT INTO ClassworkNotificationStaff (ClassworkNotificationsId, TeachersStaffId)
SELECT ClassworkNotification_Id, Staff_StaffId FROM [ACOS].[dbo].ClassworkNotificationStaffs
WHERE 1 = 1;

ALTER TABLE ClassCoverClassworkNotification WITH CHECK CHECK CONSTRAINT ALL;

GO