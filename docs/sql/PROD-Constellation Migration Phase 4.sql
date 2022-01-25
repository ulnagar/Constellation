USE CONSTELLATION_PROD
GO

-- COURSEOFFERINGS TABLE --
ALTER TABLE OFFERINGS NOCHECK CONSTRAINT ALL;
SET IDENTITY_INSERT OFFERINGS ON;

INSERT INTO OFFERINGS (Id, Name, CourseId, StartDate, EndDate)
SELECT Id, Name, CourseId, StartDate AS datetime2, EndDate AS datetime2 FROM [ACOS].[dbo].CourseOfferings
WHERE 1 = 1;

SET IDENTITY_INSERT OFFERINGS OFF;
ALTER TABLE OFFERINGS WITH CHECK CHECK CONSTRAINT ALL;


GO