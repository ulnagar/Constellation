USE CONSTELLATION_PROD
GO

-- ABSENCENOTIFICATIONS TABLE --
ALTER TABLE ABSENCENOTIFICATION NOCHECK CONSTRAINT ALL;

INSERT INTO ABSENCENOTIFICATION
	(Id, AbsenceId, Type, OutgoingId, SentAt, Message, Recipients, ConfirmedDelivered, DeliveredAt, DeliveredMessageIds)
SELECT
	Id, AbsenceId, Type, OutgoingId, SentAt AS datetime2, Message, Recipients, ConfirmedDelivered, DeliveredAt AS datetime2, DeliveredMessageIds
FROM [ACOS].[dbo].AbsenceNotifications
WHERE 1 = 1;

ALTER TABLE ABSENCENOTIFICATION WITH CHECK CHECK CONSTRAINT ALL;

-- ABSENCERESPONSE TABLE --
ALTER TABLE ABSENCERESPONSE NOCHECK CONSTRAINT ALL;

INSERT INTO ABSENCERESPONSE
	(Id, AbsenceId, ReceivedAt, Type, [From], Explanation, VerificationStatus, Verifier, VerifiedAt, VerificationComment, Forwarded)
SELECT
	Id, AbsenceId, ReceivedAt AS datetime2, Type, [From], Explanation, VerificationStatus, Verifier, VerifiedAt AS datetime2, VerificationComment, Forwarded
FROM [ACOS].[dbo].ABSENCERESPONSES
WHERE 1 = 1;

ALTER TABLE ABSENCERESPONSE WITH CHECK CHECK CONSTRAINT ALL;

GO