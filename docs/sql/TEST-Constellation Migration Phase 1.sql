USE CONSTELLATION_TEST
GO

-- SCHOOLS TABLE --
ALTER TABLE SCHOOLS NOCHECK CONSTRAINT ALL;

INSERT INTO SCHOOLS (Code, Name, Address, Town, State, PostCode, PhoneNumber, FaxNumber, EmailAddress, Division, HeatSchool, Electorate, PrincipalNetwork, TimetableApplication, RollCallGroup)
SELECT * FROM [ACOS_TEST].[dbo].Schools
WHERE 1 = 1;

ALTER TABLE SCHOOLS WITH CHECK CHECK CONSTRAINT ALL;

-- SCHOOLCONTACT TABLE --
ALTER TABLE SCHOOLCONTACT NOCHECK CONSTRAINT ALL;
SET IDENTITY_INSERT SCHOOLCONTACT ON;

INSERT INTO SchoolContact (Id, FirstName, LastName, EmailAddress, PhoneNumber, IsDeleted, DateDeleted, DateEntered, SelfRegistered)
SELECT Id, FirstName, LastName, EmailAddress, PhoneNumber, IsDeleted, DateDeleted AS datetime2, DateEntered AS datetime2, SelfRegistered FROM [ACOS_TEST].[dbo].[SchoolContact]
WHERE 1 = 1;

SET IDENTITY_INSERT SCHOOLCONTACT OFF;
ALTER TABLE SCHOOLCONTACT WITH CHECK CHECK CONSTRAINT ALL;

-- TIMETABLEPERIODS TABLE --
ALTER TABLE PERIODS NOCHECK CONSTRAINT ALL;
SET IDENTITY_INSERT PERIODS ON;

INSERT INTO PERIODS (Id, Timetable, Day, Period, StartTime, EndTime, Name, Type, IsDeleted, DateDeleted)
SELECT Id, Timetable, Day, Period, StartTime, EndTime, Name, Type, IsDeleted, DateDeleted AS datetime2 FROM [ACOS_TEST].[dbo].[TimetablePeriods]
WHERE 1 = 1;

SET IDENTITY_INSERT PERIODS OFF;
ALTER TABLE PERIODS WITH CHECK CHECK CONSTRAINT ALL;

-- DEVICES TABLE --
ALTER TABLE DEVICES NOCHECK CONSTRAINT ALL;

INSERT INTO DEVICES (SerialNumber, Make, Model, Description, Status, DateWarrantyExpires, DateReceived, DateDisposed)
SELECT SerialNumber, Make, Model, Description, Status, DateWarrantyExpires AS datetime2, DateReceived AS datetime2, DateDisposed AS datetime2 FROM [ACOS_TEST].[dbo].[Devices]
WHERE 1 = 1;

ALTER TABLE DEVICES WITH CHECK CHECK CONSTRAINT ALL;

-- LESSONS TABLE --
ALTER TABLE LESSONS NOCHECK CONSTRAINT ALL;

INSERT INTO LESSONS (Id, Name, DueDate, DoNotGenerateRolls)
SELECT Id, Name, DueDate AS datetime2, DoNotGenerateRolls FROM [ACOS_TEST].[dbo].Lessons
WHERE 1 = 1;

ALTER TABLE LESSONS WITH CHECK CHECK CONSTRAINT ALL;

-- ADOBECONNECTROOMS TABLE --
ALTER TABLE ROOMS NOCHECK CONSTRAINT ALL;

INSERT INTO ROOMS (ScoId, Name, UrlPath, IsDeleted, DateDeleted, Protected)
SELECT ScoId, Name, UrlPath, IsDeleted, DateDeleted AS datetime2, Protected FROM [ACOS_TEST].[dbo].[AdobeConnectRooms]
WHERE 1 = 1;

ALTER TABLE ROOMS WITH CHECK CHECK CONSTRAINT ALL;

GO