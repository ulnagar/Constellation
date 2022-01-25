using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC operations
    public class AbsenceService : IAbsenceService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        
        public AbsenceService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public ServiceOperationResult<StudentWholeAbsence> CreateWholeAbsence(WholeAbsenceDto absenceResource)
        {
            // Set up return entity
            var result = new ServiceOperationResult<StudentWholeAbsence>();

            var studentAbsence = new StudentWholeAbsence
            {
                DateScanned = absenceResource.DateScanned,
                Date = absenceResource.Date,
                StudentId = absenceResource.StudentId,
                OfferingId = absenceResource.OfferingId,
                PeriodTimeframe = absenceResource.PeriodTimeframe,
                PeriodName = absenceResource.PeriodName,
                LastSeen = absenceResource.DateScanned
            };

            _unitOfWork.Add(studentAbsence);

            result.Success = true;
            result.Entity = studentAbsence;

            return result;
        }

        public ServiceOperationResult<StudentPartialAbsence> CreatePartialAbsence(PartialAbsenceDto absenceResource)
        {
            // Set up return entity
            var result = new ServiceOperationResult<StudentPartialAbsence>();

            var studentAbsence = new StudentPartialAbsence()
            {
                DateScanned = absenceResource.DateScanned,
                Date = absenceResource.Date,
                StudentId = absenceResource.StudentId,
                OfferingId = absenceResource.OfferingId,
                PeriodTimeframe = absenceResource.PeriodTimeframe,
                PeriodName = absenceResource.PeriodName,
                PartialAbsenceLength = absenceResource.PartialAbsenceLength,
                PartialAbsenceTimeframe = absenceResource.PartialAbsenceTimeframe,
                LastSeen = absenceResource.DateScanned
            };

            _unitOfWork.Add(studentAbsence);

            result.Success = true;
            result.Entity = studentAbsence;

            return result;
        }

        public async Task CreateSingleParentExplanation(Guid absenceId, string explanation)
        {
            var absence = await _unitOfWork.Absences.ForExplanationFromParent(absenceId);

            var notificationEmail = new EmailDtos.AbsenceResponseEmail();

            if (absence == null)
                return;

            var response = new AbsenceResponse()
            {
                Type = AbsenceResponse.Parent,
                From = AbsenceResponse.Parent,
                ReceivedAt = DateTime.Now,
                Explanation = explanation
            };

            absence.Responses.Add(response);
            await _unitOfWork.CompleteAsync();

            var teachers = absence.Offering
                .Sessions
                .Where(session => !session.IsDeleted)
                .Select(session => session.Teacher.EmailAddress)
                .Distinct()
                .ToList();

            notificationEmail.Recipients.AddRange(teachers);
            notificationEmail.WholeAbsences.Add(absence);

            await _emailService.SendAbsenceReasonToSchoolAdmin(notificationEmail);
            response.Forwarded = true;

            await _unitOfWork.CompleteAsync();
        }

        public async Task CreateSingleCoordinatorExplanation(Guid absenceId, string explanation, string userName)
        {
            var absence = await _unitOfWork.Absences.ForExplanationFromParent(absenceId);

            var notificationEmail = new EmailDtos.AbsenceResponseEmail();

            if (absence == null)
                return;

            var response = new AbsenceResponse()
            {
                Type = AbsenceResponse.Coordinator,
                From = userName,
                ReceivedAt = DateTime.Now,
                Explanation = explanation
            };

            absence.Responses.Add(response);
            await _unitOfWork.CompleteAsync();

            notificationEmail.Recipients.Add("auroracoll-h.school@det.nsw.edu.au");
            notificationEmail.WholeAbsences.Add(absence);

            await _emailService.SendAbsenceReasonToSchoolAdmin(notificationEmail);
            response.Forwarded = true;

            await _unitOfWork.CompleteAsync();
        }

        public async Task CreateSingleStudentExplanation(Guid absenceId, string explanation)
        {
            var absence = await _unitOfWork.Absences.ForExplanationFromStudent(absenceId);

            if (absence == null)
                return;

            var response = new AbsenceResponse()
            {
                Absence = absence,
                AbsenceId = absence.Id,
                Type = AbsenceResponse.Student,
                From = AbsenceResponse.Student,
                ReceivedAt = DateTime.Now,
                Explanation = explanation,
                VerificationStatus = AbsenceResponse.Pending,
                VerifiedAt = DateTime.Now
            };

            absence.Responses.Add(response);
            await _unitOfWork.CompleteAsync();

            var notificationEmail = new EmailDtos.AbsenceResponseEmail();

            var contacts = absence.Student
                .School
                .StaffAssignments
                .Where(assignment => !assignment.IsDeleted && !assignment.SchoolContact.IsDeleted && assignment.Role == SchoolContactRole.Coordinator)
                .Select(assignment => assignment.SchoolContact.EmailAddress)
                .ToList();

            notificationEmail.Recipients.AddRange(contacts);
            notificationEmail.PartialAbsences.Add(absence);

            var email = await _emailService.SendCoordinatorPartialAbsenceVerificationRequest(notificationEmail);
            response.Forwarded = true;

            var notification = new AbsenceNotification
            {
                Absence = absence,
                AbsenceId = absence.Id,
                OutgoingId = email.id,
                Type = "Email",
                Message = email.message,
                Recipients = email.recipients,
                SentAt = DateTime.Now,
            };

            absence.Notifications.Add(notification);

            await _unitOfWork.CompleteAsync();
        }

        public async Task RecordCoordinatorVerificationOfPartialExplanation(Guid responseId, bool isVerified, string comment, string username)
        {
            var response = await _unitOfWork.Absences.AsResponseForVerificationByCoordinator(responseId);

            if (response == null)
                return;

            if (isVerified)
                response.VerificationStatus = AbsenceResponse.Verified;
            else
                response.VerificationStatus = AbsenceResponse.Rejected;

            response.Verifier = username;
            response.VerificationComment = comment;
            response.VerifiedAt = DateTime.Now;

            await _unitOfWork.CompleteAsync();

            var notificationEmail = new EmailDtos.AbsenceResponseEmail();

            notificationEmail.Recipients.Add("auroracoll-h.school@det.nsw.edu.au");
            notificationEmail.WholeAbsences.Add(response.Absence);

            await _emailService.SendAbsenceReasonToSchoolAdmin(notificationEmail);
            response.Forwarded = true;

            await _unitOfWork.CompleteAsync();
        }
    }
}