﻿using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Templates.Views.Documents.Covers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC operations
    public class CoverService : ICoverService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IPDFService _pdfService;
        private readonly IOperationService _operationService;

        public CoverService(IUnitOfWork unitOfWork, IOperationService operationService,
            IEmailService emailService, IPDFService pdfService)
        {
            _unitOfWork = unitOfWork;
            _operationService = operationService;
            _emailService = emailService;
            _pdfService = pdfService;
        }

        public async Task<ServiceOperationResult<ICollection<ClassCover>>> BulkCreateCovers(CoverDto coverResource)
        {
            var classId = coverResource.ClassId ?? 0;

            var coverSchedule = new List<CoverClassSchedule>();

            var dates = coverResource.StartDate.Range(coverResource.EndDate).Select(date => date.GetDayNumber()).ToList();

            if (!string.IsNullOrWhiteSpace(coverResource.TeacherId))
            {
                var coverOfferings = await _unitOfWork.CourseOfferings.ForTeacherAndDates(coverResource.TeacherId, dates);;

                foreach (var offering in coverOfferings)
                {
                    var cover = new CoverClassSchedule
                    {
                        Offering = offering,
                        RoomName = offering.Sessions.First().Room.Name,
                        RoomLink = offering.Sessions.First().Room.UrlPath
                    };

                    cover.Teachers.Add(await _unitOfWork.Staff.GetForExistCheck(coverResource.TeacherId));

                    foreach (var day in coverResource.StartDate.Range(coverResource.EndDate))
                    {
                        var periods = offering.Sessions.Where(session => session.Period.Day == day.GetDayNumber() && !session.IsDeleted)
                            .OrderBy(session => session.Period.StartTime)
                            .Select(session => session.Period)
                            .Where(period => period.Name != "OutOfBand");

                        // If more than one period, put in time order.
                        // If the periods are consecutive, set the start time for the first, and the end time for the last.
                        var periodGroups = periods.GroupConsecutive();

                        foreach (var periodGroup in periodGroups)
                        {
                            var periodBlock = periodGroup.ToList();

                            cover.Instances.Add(new CoverClassSchedule.ClassInstance
                            {
                                Start = new DateTime(day.Year, day.Month, day.Day, periodBlock.First().StartTime.Hours, periodBlock.First().StartTime.Minutes, 0),
                                End = new DateTime(day.Year, day.Month, day.Day, periodBlock.Last().EndTime.Hours, periodBlock.Last().EndTime.Minutes, 0)
                            });
                        }
                    }

                    coverSchedule.Add(cover);
                }
            }

            foreach (var entry in coverResource.SelectedClasses)
            {
                var offering = await _unitOfWork.CourseOfferings.ForCoverCreationAsync(entry);

                var cover = new CoverClassSchedule
                {
                    Offering = offering,
                    RoomName = offering.Sessions.First().Room.Name,
                    RoomLink = offering.Sessions.First().Room.UrlPath,
                    Teachers = await _unitOfWork.CourseOfferings.AllTeachersForCoverCreationAsync(entry)
                };

                foreach (var day in coverResource.StartDate.Range(coverResource.EndDate))
                {
                    var periods = offering.Sessions.Where(session => session.Period.Day == day.GetDayNumber() && !session.IsDeleted)
                        .OrderBy(session => session.Period.StartTime)
                        .Select(session => session.Period)
                        .Where(period => period.Type != "Other");

                    // If more than one period, put in time order.
                    // If the periods are consecutive, set the start time for the first, and the end time for the last.
                    var periodGroups = periods.GroupConsecutive();

                    foreach (var periodGroup in periodGroups)
                    {
                        var periodBlock = periodGroup.ToList();

                        cover.Instances.Add(new CoverClassSchedule.ClassInstance
                        {
                            Start = new DateTime(day.Year, day.Month, day.Day, periodBlock.First().StartTime.Hours, periodBlock.First().StartTime.Minutes, 0),
                            End = new DateTime(day.Year, day.Month, day.Day, periodBlock.Last().EndTime.Hours, periodBlock.Last().EndTime.Minutes, 0)
                        });
                    }
                }

                coverSchedule.Add(cover);
            }

            if (coverResource.UserType == "Casuals")
            {
                Int32.TryParse(coverResource.UserId, out int intId);
                var casual = await _unitOfWork.Casuals.ForEditAsync(intId);

                var coverList = new List<CasualClassCover>();

                foreach (var cover in coverSchedule)
                {
                    var casualCover = new CasualCoverDto
                    {
                        CasualId = casual.Id,
                        OfferingId = cover.Offering.Id,
                        StartDate = coverResource.StartDate,
                        EndDate = coverResource.EndDate
                    };

                    var result = await CreateCasualCover(casualCover);

                    if (result.Success)
                    {
                        coverList.Add(result.Entity);
                        cover.Id = result.Entity.Id;
                    }
                }

                EmailDtos.CoverEmail resource = await CreateBulkCoverEmail(coverResource, coverSchedule, casual, coverList);

                await _emailService.SendNewCoverEmail(resource);

                return new ServiceOperationResult<ICollection<ClassCover>> { Entity = coverList.ConvertAll(x => (ClassCover)x), Success = true };
            }

            if (coverResource.UserType == "Teachers")
            {
                var teacher = await _unitOfWork.Staff.ForEditAsync(coverResource.UserId);

                var coverList = new List<TeacherClassCover>();

                foreach (var cover in coverSchedule)
                {
                    var teacherCover = new TeacherCoverDto
                    {
                        StaffId = teacher.StaffId,
                        OfferingId = cover.Offering.Id,
                        StartDate = coverResource.StartDate,
                        EndDate = coverResource.EndDate
                    };

                    var result = await CreateTeacherCover(teacherCover);

                    if (result.Success)
                    {
                        coverList.Add(result.Entity);
                        cover.Id = result.Entity.Id;
                    }
                    else
                        break;
                }

                var resource = await CreateBulkCoverEmail(coverResource, coverSchedule, teacher, coverList);

                await _emailService.SendNewCoverEmail(resource);
            
                return new ServiceOperationResult<ICollection<ClassCover>> { Entity = coverList.ConvertAll(x => (ClassCover)x), Success = true };
            }

            return new ServiceOperationResult<ICollection<ClassCover>> { Success = false, Errors = new List<string> { "Could not identify the covering teacher type." } };
        }

        public async Task<ServiceOperationResult<CasualClassCover>> CreateCasualCover(CasualCoverDto coverResource)
        {
            var result = new ServiceOperationResult<CasualClassCover>();

            if (coverResource.StartDate > coverResource.EndDate)
            {
                result.Success = false;
                result.Errors.Add($"A cover cannot start after it ends!");

                return result;
            }
            
            if (coverResource.EndDate < DateTime.Today)
            {
                result.Success = false;
                result.Errors.Add($"You cannot create a cover that has already ended!");

                return result;
            }

            var offering = await _unitOfWork.CourseOfferings.ForCoverCreationAsync(coverResource.OfferingId);

            var cover = new CasualClassCover
            {
                OfferingId = coverResource.OfferingId,
                Offering = offering,
                CasualId = coverResource.CasualId,
                StartDate = coverResource.StartDate,
                EndDate = coverResource.EndDate
            };

            _unitOfWork.Add(cover);
            await _unitOfWork.CompleteAsync();

            foreach (var room in offering.Sessions.Where(s => !s.IsDeleted).Select(s => s.RoomId).Distinct())
            {
                await _operationService.CreateCasualAdobeConnectAccess(cover.CasualId, room, cover.StartDate.AddDays(-1), cover.Id);
                await _operationService.RemoveCasualAdobeConnectAccess(cover.CasualId, room, cover.EndDate.AddDays(1), cover.Id);
            }

            await _operationService.CreateCasualMSTeamOwnerAccess(cover.CasualId, cover.OfferingId, cover.Id, cover.StartDate.AddDays(-1));
            await _operationService.RemoveCasualMSTeamAccess(cover.CasualId, cover.OfferingId, cover.Id, cover.EndDate.AddDays(1));

            result.Success = true;
            result.Entity = cover;

            return result;
        }

        public async Task<ServiceOperationResult<CasualClassCover>> UpdateCasualCover(CasualCoverDto coverResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<CasualClassCover>();

            // Validate entries
            var cover = await _unitOfWork.Covers.ForUpdate(coverResource.Id) as CasualClassCover;

            if (cover == null)
            {
                result.Success = false;
                result.Errors.Add($"A cover with that ID cannot be found!");

                return result;
            }

            if (coverResource.StartDate > coverResource.EndDate)
            {
                result.Success = false;
                result.Errors.Add($"A cover cannot start after it ends!");

                return result;
            }

            if (coverResource.EndDate < DateTime.Today)
            {
                result.Success = false;
                result.Errors.Add($"You cannot create a cover that has already ended!");

                return result;
            }
            
            var oStartDate = cover.StartDate;
            var oEndDate = cover.EndDate;

            if (oStartDate != coverResource.StartDate)
            {
                cover.StartDate = coverResource.StartDate;

                foreach (var operation in cover.AdobeConnectOperations.Where(o => (!o.IsDeleted || !o.IsCompleted) && o.Action == AdobeConnectOperationAction.Add))
                {
                    await _operationService.CancelAdobeConnectOperation(operation.Id);
                    await _operationService.CreateCasualAdobeConnectAccess(cover.CasualId, operation.ScoId, cover.StartDate.AddDays(-1), cover.Id);
                }

                foreach (var operation in cover.MSTeamOperations.Where(o => (!o.IsDeleted || !o.IsCompleted) && o.Action == MSTeamOperationAction.Add))
                {
                    await _operationService.CancelMSTeamOperation(operation.Id);
                    await _operationService.CreateCasualMSTeamOwnerAccess(cover.CasualId, cover.OfferingId, cover.Id, cover.StartDate.AddDays(-1));
                }
            }

            if (oEndDate != coverResource.EndDate)
            {
                cover.EndDate = coverResource.EndDate;

                foreach (var operation in cover.AdobeConnectOperations.Where(o => (!o.IsCompleted || !o.IsDeleted) && o.Action == AdobeConnectOperationAction.Remove))
                {
                    await _operationService.CancelAdobeConnectOperation(operation.Id);
                    await _operationService.RemoveCasualAdobeConnectAccess(cover.CasualId, operation.ScoId, cover.EndDate.AddDays(1), cover.Id);
                }

                foreach (var operation in cover.MSTeamOperations.Where(o => (!o.IsDeleted || !o.IsCompleted) && o.Action == MSTeamOperationAction.Remove))
                {
                    await _operationService.CancelMSTeamOperation(operation.Id);
                    await _operationService.RemoveCasualMSTeamAccess(cover.CasualId, cover.OfferingId, cover.Id, cover.EndDate.AddDays(-1));
                }
            }

            var resource = await CreateUpdatedCoverEmail(cover);

            await _emailService.SendUpdatedCoverEmail(resource);

            result.Success = true;
            result.Entity = cover;

            return result;
        }

        public async Task RemoveCasualCover(int coverId)
        {
            // Validate entries
            var cover = await _unitOfWork.CasualClassCovers.ForEditAsync(coverId);

            if (cover == null)
                return;

            cover.IsDeleted = true;

            // Has the cover already started? Do we need to remove access?
            if (cover.StartDate < DateTime.Now)
            {
                foreach (var operation in cover.AdobeConnectOperations.Where(o => (!o.IsCompleted || !o.IsDeleted) && o.Action == AdobeConnectOperationAction.Remove))
                {
                    await _operationService.CancelAdobeConnectOperation(operation.Id);
                    await _operationService.RemoveCasualAdobeConnectAccess(cover.CasualId, operation.ScoId, DateTime.Now, cover.Id);
                }

                foreach (var operation in cover.MSTeamOperations.Where(o => (!o.IsDeleted || !o.IsCompleted) && o.Action == MSTeamOperationAction.Remove))
                {
                    await _operationService.CancelMSTeamOperation(operation.Id);
                    await _operationService.RemoveCasualMSTeamAccess(cover.CasualId, cover.OfferingId, cover.Id, DateTime.Now);
                }
            }
            else
            {
                foreach (var operation in cover.AdobeConnectOperations.Where(o => !o.IsDeleted || !o.IsCompleted))
                {
                    await _operationService.CancelAdobeConnectOperation(operation.Id);
                }

                foreach (var operation in cover.MSTeamOperations.Where(o => !o.IsDeleted || !o.IsCompleted))
                {
                    await _operationService.CancelMSTeamOperation(operation.Id);
                }
            }

            var resource = await CreateCancelledCoverEmail(cover);
            await _emailService.SendCancelledCoverEmail(resource);
        }

        public async Task<ServiceOperationResult<TeacherClassCover>> CreateTeacherCover(TeacherCoverDto coverResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<TeacherClassCover>();

            // Validate entries
            var checkCover = await _unitOfWork.Covers.GetForExistCheck(coverResource.Id);
            var offering = await _unitOfWork.CourseOfferings.GetForExistCheck(coverResource.OfferingId);
            var checkTeacher = await _unitOfWork.Staff.GetForExistCheck(coverResource.StaffId);

            if (checkCover != null)
            {
                result.Success = false;
                result.Errors.Add($"A cover with that ID already exists!");
            }
            else if (offering == null)
            {
                result.Success = false;
                result.Errors.Add($"A class with that ID cannot be found!");
            }
            else if (checkTeacher == null)
            {
                result.Success = false;
                result.Errors.Add($"A teacher with that ID cannot be found!");
            }
            else if (coverResource.StartDate > coverResource.EndDate)
            {
                result.Success = false;
                result.Errors.Add($"A cover cannot start after it ends!");
            }
            else if (coverResource.EndDate < DateTime.Today)
            {
                result.Success = false;
                result.Errors.Add($"You cannot create a cover that has already ended!");
            }
            else
            {
                var cover = new TeacherClassCover
                {
                    OfferingId = coverResource.OfferingId,
                    StaffId = coverResource.StaffId,
                    StartDate = coverResource.StartDate,
                    EndDate = coverResource.EndDate
                };

                _unitOfWork.Add(cover);
                await _unitOfWork.CompleteAsync();

                foreach (var room in offering.Sessions.Where(s => !s.IsDeleted).Select(s => s.RoomId).Distinct())
                {
                    await _operationService.CreateTeacherAdobeConnectAccess(cover.StaffId, room, cover.StartDate.AddDays(-1), cover.Id);
                    await _operationService.RemoveTeacherAdobeConnectAccess(cover.StaffId, room, cover.EndDate.AddDays(1), cover.Id);
                }

                if ((checkTeacher.Faculty & Faculty.Executive) == checkTeacher.Faculty)
                {
                    // Teacher is an Executive Faculty Member. Do not remove!
                }
                else if (offering.Sessions.Select(s => s.StaffId).Contains(checkTeacher.StaffId))
                {
                    // Teacher is a regular teacher of the class. Do not remove!
                }
                else
                {
                    await _operationService.CreateTeacherMSTeamOwnerAccess(cover.StaffId, cover.OfferingId, cover.StartDate.AddDays(-1), cover.Id);
                    await _operationService.RemoveTeacherMSTeamAccess(cover.StaffId, cover.OfferingId, cover.EndDate.AddDays(1), cover.Id);
                }

                result.Success = true;
                result.Entity = cover;
            }

            return result;
        }

        public async Task<ServiceOperationResult<TeacherClassCover>> UpdateTeacherCover(TeacherCoverDto coverResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<TeacherClassCover>();

            // Validate entries
            var cover = await _unitOfWork.Covers.ForUpdate(coverResource.Id) as TeacherClassCover;

            if (cover == null)
            {
                result.Success = false;
                result.Errors.Add($"A cover with that ID cannot be found!");

                return result;
            }

            if (coverResource.StartDate > coverResource.EndDate)
            {
                result.Success = false;
                result.Errors.Add($"A cover cannot start after it ends!");

                return result;
            }

            if (coverResource.EndDate < DateTime.Today)
            {
                result.Success = false;
                result.Errors.Add($"You cannot create a cover that has already ended!");

                return result;
            }
            
            var oStartDate = cover.StartDate;
            var oEndDate = cover.EndDate;

            if (oStartDate != coverResource.StartDate)
            {
                cover.StartDate = coverResource.StartDate;

                foreach (var operation in cover.AdobeConnectOperations.Where(o => (!o.IsDeleted || !o.IsCompleted) && o.Action == AdobeConnectOperationAction.Add))
                {
                    await _operationService.CancelAdobeConnectOperation(operation.Id);
                    await _operationService.CreateTeacherAdobeConnectAccess(cover.StaffId, operation.ScoId, cover.StartDate.AddDays(-1), cover.Id);
                }

                foreach (var operation in cover.MSTeamOperations.Where(o => (!o.IsDeleted || !o.IsCompleted) && o.Action == MSTeamOperationAction.Add))
                {
                    await _operationService.CancelMSTeamOperation(operation.Id);
                    await _operationService.CreateTeacherMSTeamOwnerAccess(cover.StaffId, cover.OfferingId, cover.StartDate.AddDays(-1), cover.Id);
                }
            }

            if (oEndDate != coverResource.EndDate)
            {
                cover.EndDate = coverResource.EndDate;

                foreach (var operation in cover.AdobeConnectOperations.Where(o => (!o.IsCompleted || !o.IsDeleted) && o.Action == AdobeConnectOperationAction.Remove))
                {
                    await _operationService.CancelAdobeConnectOperation(operation.Id);
                    await _operationService.RemoveTeacherAdobeConnectAccess(cover.StaffId, operation.ScoId, cover.EndDate.AddDays(1), cover.Id);
                }

                foreach (var operation in cover.MSTeamOperations.Where(o => (!o.IsDeleted || !o.IsCompleted) && o.Action == MSTeamOperationAction.Remove))
                {
                    await _operationService.CancelMSTeamOperation(operation.Id);
                    await _operationService.RemoveTeacherMSTeamAccess(cover.StaffId, cover.OfferingId, cover.EndDate.AddDays(1), cover.Id);
                }
            }

            var resource = await CreateUpdatedCoverEmail(cover);

            await _emailService.SendUpdatedCoverEmail(resource);

            result.Success = true;
            result.Entity = cover;

            return result;
        }

        public async Task RemoveTeacherCover(int coverId)
        {
            // Validate entries
            var cover = await _unitOfWork.Covers.ForUpdate(coverId) as TeacherClassCover;

            if (cover == null)
                return;

            cover.IsDeleted = true;

            // Has the cover already started? Do we need to remove access?
            if (cover.StartDate < DateTime.Now)
            {
                foreach (var operation in cover.AdobeConnectOperations.Where(o => (!o.IsCompleted || !o.IsDeleted) && o.Action == AdobeConnectOperationAction.Remove))
                {
                    await _operationService.CancelAdobeConnectOperation(operation.Id);
                    await _operationService.RemoveTeacherAdobeConnectAccess(cover.StaffId, operation.ScoId, DateTime.Now, cover.Id);
                }

                foreach (var operation in cover.MSTeamOperations.Where(o => (!o.IsDeleted || !o.IsCompleted) && o.Action == MSTeamOperationAction.Remove))
                {
                    await _operationService.CancelMSTeamOperation(operation.Id);
                    await _operationService.RemoveTeacherMSTeamAccess(cover.StaffId, cover.OfferingId, DateTime.Now, cover.Id);
                }
            }
            else
            {
                foreach (var operation in cover.AdobeConnectOperations.Where(o => !o.IsDeleted || !o.IsCompleted))
                {
                    await _operationService.CancelAdobeConnectOperation(operation.Id);
                }

                foreach (var operation in cover.MSTeamOperations.Where(o => !o.IsDeleted || !o.IsCompleted))
                {
                    await _operationService.CancelMSTeamOperation(operation.Id);
                }
            }

            var resource = await CreateCancelledCoverEmail(cover);
            await _emailService.SendCancelledCoverEmail(resource);
        }

        private class CoverClassSchedule
        {
            public int Id { get; set; }
            public CourseOffering Offering { get; set; }
            public string RoomName { get; set; }
            public string RoomLink { get; set; }
            public ICollection<Staff> Teachers { get; set; }
            public ICollection<ClassInstance> Instances { get; set; }

            public CoverClassSchedule()
            {
                Teachers = new List<Staff>();
                Instances = new List<ClassInstance>();
            }

            public class ClassInstance
            {
                public DateTime Start { get; set; }
                public DateTime End { get; set; }
            }
        }

        private async Task<EmailDtos.CoverEmail> CreateBulkCoverEmail(CoverDto coverResource, List<CoverClassSchedule> coverSchedule, Casual casual, List<CasualClassCover> coverList)
        {
            var secondaryRecipients = new Dictionary<string, string>();
            var headTeachers = coverList.Select(cover => cover.Offering.Course.HeadTeacher).Distinct().ToList();
            headTeachers.ForEach(teacher => secondaryRecipients.Add(teacher.DisplayName, teacher.EmailAddress));

            var roleUsers = await _unitOfWork.Identities.UsersInRole(AuthRoles.CoverRecipient);
            foreach (var user in roleUsers)
                if (!secondaryRecipients.Any(recipient => recipient.Value == user.Email))
                    secondaryRecipients.Add(user.DisplayName, user.Email);

            var classroomTeachers = new Dictionary<string, string>();
            coverSchedule.SelectMany(cover => cover.Teachers).Distinct().ToList().ForEach(teacher => classroomTeachers.Add(teacher.DisplayName, teacher.EmailAddress));

            var classesIncluded = new Dictionary<string, string>();
            coverSchedule.ForEach(cover => classesIncluded.Add(cover.RoomName, cover.RoomLink));

            var resource = new EmailDtos.CoverEmail()
            {
                CoveringTeacherName = casual.DisplayName,
                CoveringTeacherEmail = casual.EmailAddress,
                CoveringTeacherAdobeAccount = string.IsNullOrWhiteSpace(casual.AdobeConnectPrincipalId),

                ClassroomTeachers = classroomTeachers,
                SecondaryRecipients = secondaryRecipients,

                ClassesIncluded = classesIncluded,
                StartDate = coverResource.StartDate,
                EndDate = coverResource.EndDate,

            };

            foreach (var cover in coverSchedule)
            {
                //foreach (var instance in cover.Instances)
                //{
                //    // Create and add ICS files
                //    var uid = $"{cover.Id}-{cover.Offering.Id}-{instance.Start.Date:yyyyMMdd}";
                //    var summary = $"Aurora College Cover - {cover.Offering.Name}";
                //    var location = cover.Offering.Sessions
                //        .First(session => session.Period.Day == instance.Start.Date.GetDayNumber())
                //        .Room
                //        .UrlPath;
                //    var description = "";

                //    var icsData = _calendarService.CreateEvent(uid, summary, location, description, instance.Start, instance.End, 0);

                //    resource.Attachments.Add(Attachment.CreateAttachmentFromString(icsData, $"{uid}.ics", Encoding.ASCII, "text/calendar"));
                //}

                // Create Roll
                var offering = await _unitOfWork.CourseOfferings.ForRollCreationAsync(cover.Offering.Id);
                var model = new CoverRollViewModel
                {
                    ClassName = offering.Name,
                    Students = offering.Enrolments.Where(enrol => !enrol.IsDeleted).Select(CoverRollViewModel.EnrolledStudent.ConvertFromEnrolment).ToList()
                };

                //var htmlString = await _razorService.RenderViewToStringAsync("/Views/Documents/Covers/CoverRoll.cshtml", model);

                //resource.Attachments.Add(_pdfService.StringToPdfAttachment(htmlString, $"{cover.Offering.Name} Roll.pdf"));
            }

            return resource;
        }

        private async Task<EmailDtos.CoverEmail> CreateBulkCoverEmail(CoverDto coverResource, List<CoverClassSchedule> coverSchedule, Staff staff, List<TeacherClassCover> coverList)
        {
            var secondaryRecipients = new Dictionary<string, string>();
            var headTeachers = coverList.Select(cover => cover.Offering.Course.HeadTeacher).Distinct().ToList();
            headTeachers.ForEach(teacher => secondaryRecipients.Add(teacher.DisplayName, teacher.EmailAddress));

            var roleUsers = await _unitOfWork.Identities.UsersInRole(AuthRoles.CoverRecipient);
            foreach (var user in roleUsers)
                if (!secondaryRecipients.Any(recipient => recipient.Value == user.Email))
                    secondaryRecipients.Add(user.DisplayName, user.Email);

            var classroomTeachers = new Dictionary<string, string>();
            coverSchedule.SelectMany(cover => cover.Teachers).Distinct().ToList().ForEach(teacher => classroomTeachers.Add(teacher.DisplayName, teacher.EmailAddress));

            var classesIncluded = new Dictionary<string, string>();
            coverSchedule.ForEach(cover => classesIncluded.Add(cover.RoomName, cover.RoomLink));

            var resource = new EmailDtos.CoverEmail()
            {
                CoveringTeacherName = staff.DisplayName,
                CoveringTeacherEmail = staff.EmailAddress,
                CoveringTeacherAdobeAccount = string.IsNullOrWhiteSpace(staff.AdobeConnectPrincipalId),

                ClassroomTeachers = classroomTeachers,
                SecondaryRecipients = secondaryRecipients,

                ClassesIncluded = classesIncluded,
                StartDate = coverResource.StartDate,
                EndDate = coverResource.EndDate,

            };

            return resource;
        }

        private async Task<EmailDtos.CoverEmail> CreateUpdatedCoverEmail(CasualClassCover cover)
        {
            var secondaryRecipients = new[] { new { cover.Offering.Course.HeadTeacher.DisplayName, cover.Offering.Course.HeadTeacher.EmailAddress } }.ToList();
            var roleUsers = await _unitOfWork.Identities.UsersInRole(AuthRoles.CoverRecipient);
            foreach (var user in roleUsers)
            {
                if (!secondaryRecipients.Any(recipient => recipient.EmailAddress == user.Email))
                {
                    secondaryRecipients.Add(new { user.DisplayName, EmailAddress = user.Email });
                }
            }

            var resource = new EmailDtos.CoverEmail()
            {
                CoveringTeacherName = cover.Casual.DisplayName,
                CoveringTeacherEmail = cover.Casual.EmailAddress,
                CoveringTeacherAdobeAccount = string.IsNullOrWhiteSpace(cover.Casual.AdobeConnectPrincipalId),

                ClassroomTeachers = (IDictionary<string, string>)cover.Offering.Sessions.Select(session => session.Teacher).Distinct().Select(teacher => new { teacher.DisplayName, teacher.EmailAddress }),
                SecondaryRecipients = (IDictionary<string, string>)secondaryRecipients,

                ClassesIncluded = (IDictionary<string, string>)cover.Offering.Sessions.Select(session => session.Room).Distinct().Select(room => new { room.Name, room.UrlPath }),
                StartDate = cover.StartDate,
                EndDate = cover.EndDate,

            };

            // Create Roll
            var offering = await _unitOfWork.CourseOfferings.ForRollCreationAsync(cover.Offering.Id);
            var model = new CoverRollViewModel
            {
                ClassName = offering.Name,
                Students = offering.Enrolments.Where(enrol => !enrol.IsDeleted).Select(CoverRollViewModel.EnrolledStudent.ConvertFromEnrolment).ToList()
            };

            //var htmlString = await _razorService.RenderViewToStringAsync("/Views/Documents/Covers/CoverRoll.cshtml", model);

            //resource.Attachments.Add(_pdfService.StringToPdfAttachment(htmlString, $"{cover.Offering.Name} Roll.pdf"));

            return resource;
        }

        private async Task<EmailDtos.CoverEmail> CreateUpdatedCoverEmail(TeacherClassCover cover)
        {
            var secondaryRecipients = new[] { new { cover.Offering.Course.HeadTeacher.DisplayName, cover.Offering.Course.HeadTeacher.EmailAddress } }.ToList();
            var roleUsers = await _unitOfWork.Identities.UsersInRole(AuthRoles.CoverRecipient);
            foreach (var user in roleUsers)
            {
                if (!secondaryRecipients.Any(recipient => recipient.EmailAddress == user.Email))
                {
                    secondaryRecipients.Add(new { user.DisplayName, EmailAddress = user.Email });
                }
            }

            var resource = new EmailDtos.CoverEmail()
            {
                CoveringTeacherName = cover.Staff.DisplayName,
                CoveringTeacherEmail = cover.Staff.EmailAddress,
                CoveringTeacherAdobeAccount = string.IsNullOrWhiteSpace(cover.Staff.AdobeConnectPrincipalId),

                ClassroomTeachers = (IDictionary<string, string>)cover.Offering.Sessions.Select(session => session.Teacher).Distinct().Select(teacher => new { teacher.DisplayName, teacher.EmailAddress }),
                SecondaryRecipients = (IDictionary<string, string>)secondaryRecipients,

                ClassesIncluded = (IDictionary<string, string>)cover.Offering.Sessions.Select(session => session.Room).Distinct().Select(room => new { room.Name, room.UrlPath }),
                StartDate = cover.StartDate,
                EndDate = cover.EndDate,

            };

            return resource;
        }

        private async Task<EmailDtos.CoverEmail> CreateCancelledCoverEmail(CasualClassCover cover)
        {
            var secondaryRecipients = new[] { new { cover.Offering.Course.HeadTeacher.DisplayName, cover.Offering.Course.HeadTeacher.EmailAddress } }.ToList();
            var roleUsers = await _unitOfWork.Identities.UsersInRole(AuthRoles.CoverRecipient);
            foreach (var user in roleUsers)
            {
                if (!secondaryRecipients.Any(recipient => recipient.EmailAddress == user.Email))
                {
                    secondaryRecipients.Add(new { user.DisplayName, EmailAddress = user.Email });
                }
            }

            var resource = new EmailDtos.CoverEmail()
            {
                CoveringTeacherName = cover.Casual.DisplayName,
                CoveringTeacherEmail = cover.Casual.EmailAddress,
                CoveringTeacherAdobeAccount = string.IsNullOrWhiteSpace(cover.Casual.AdobeConnectPrincipalId),

                ClassroomTeachers = (IDictionary<string, string>)cover.Offering.Sessions.Select(session => session.Teacher).Distinct().Select(teacher => new { teacher.DisplayName, teacher.EmailAddress }),
                SecondaryRecipients = (IDictionary<string, string>)secondaryRecipients,

                ClassesIncluded = (IDictionary<string, string>)cover.Offering.Sessions.Select(session => session.Room).Distinct().Select(room => new { room.Name, room.UrlPath }),
                StartDate = cover.StartDate,
                EndDate = cover.EndDate,

            };

            return resource;
        }

        private async Task<EmailDtos.CoverEmail> CreateCancelledCoverEmail(TeacherClassCover cover)
        {
            var secondaryRecipients = new[] { new { cover.Offering.Course.HeadTeacher.DisplayName, cover.Offering.Course.HeadTeacher.EmailAddress } }.ToList();
            var roleUsers = await _unitOfWork.Identities.UsersInRole(AuthRoles.CoverRecipient);
            foreach (var user in roleUsers)
            {
                if (!secondaryRecipients.Any(recipient => recipient.EmailAddress == user.Email))
                {
                    secondaryRecipients.Add(new { user.DisplayName, EmailAddress = user.Email });
                }
            }

            var resource = new EmailDtos.CoverEmail()
            {
                CoveringTeacherName = cover.Staff.DisplayName,
                CoveringTeacherEmail = cover.Staff.EmailAddress,
                CoveringTeacherAdobeAccount = string.IsNullOrWhiteSpace(cover.Staff.AdobeConnectPrincipalId),

                ClassroomTeachers = (IDictionary<string, string>)cover.Offering.Sessions.Select(session => session.Teacher).Distinct().Select(teacher => new { teacher.DisplayName, teacher.EmailAddress }),
                SecondaryRecipients = (IDictionary<string, string>)secondaryRecipients,

                ClassesIncluded = (IDictionary<string, string>)cover.Offering.Sessions.Select(session => session.Room).Distinct().Select(room => new { room.Name, room.UrlPath }),
                StartDate = cover.StartDate,
                EndDate = cover.EndDate,

            };

            return resource;
        }
    }
}