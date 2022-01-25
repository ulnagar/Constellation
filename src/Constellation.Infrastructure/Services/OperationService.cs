using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC Operation
    public class OperationService : IOperationService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OperationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateStudentAdobeConnectAccess(string studentId, string roomId, DateTime schedule)
        {
            // Verify entries
            var checkStudent = await _unitOfWork.Students.GetForExistCheck(studentId);
            var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

            if (checkStudent == null || checkRoom == null)
                return;

            var operation = new StudentAdobeConnectOperation
            {
                ScoId = roomId,
                StudentId = studentId,
                Action = AdobeConnectOperationAction.Add,
                DateScheduled = schedule
            };

            _unitOfWork.Add(operation);
        }

        public async Task CreateCasualAdobeConnectAccess(int casualId, string roomId, DateTime schedule, int coverId)
        {
            // Verify entries
            var checkCasual = await _unitOfWork.Casuals.GetForExistCheck(casualId);
            var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

            if (checkCasual == null || checkRoom == null)
                return;

            var operation = new CasualAdobeConnectOperation
            {
                ScoId = roomId,
                CasualId = casualId,
                Action = AdobeConnectOperationAction.Add,
                DateScheduled = schedule,
                CoverId = coverId
            };

            _unitOfWork.Add(operation);
        }

        public async Task CreateTeacherAdobeConnectAccess(string staffId, string roomId, DateTime scheduled, int coverId)
        {
            // Verify entries
            var checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);
            var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

            if (checkStaff == null || checkRoom == null)
                return;

            var operation = new TeacherAdobeConnectOperation
            {
                ScoId = roomId,
                StaffId = staffId,
                Action = AdobeConnectOperationAction.Add,
                DateScheduled = scheduled,
                CoverId = coverId
            };

            _unitOfWork.Add(operation);
        }

        public async Task RemoveStudentAdobeConnectAccess(string studentId, string roomId, DateTime schedule)
        {
            // Verify entries
            var checkStudent = await _unitOfWork.Students.GetForExistCheck(studentId);
            var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

            if (checkStudent == null || checkRoom == null)
                return;

            var operation = new StudentAdobeConnectOperation
            {
                ScoId = roomId,
                StudentId = studentId,
                Action = AdobeConnectOperationAction.Remove,
                DateScheduled = schedule
            };

            _unitOfWork.Add(operation);
        }

        public async Task RemoveCasualAdobeConnectAccess(int casualId, string roomId, DateTime schedule, int coverId)
        {
            // Verify entries
            var checkCasual = await _unitOfWork.Casuals.GetForExistCheck(casualId);
            var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

            if (checkCasual == null || checkRoom == null)
                return;

            var operation = new CasualAdobeConnectOperation
            {
                ScoId = roomId,
                CasualId = casualId,
                Action = AdobeConnectOperationAction.Remove,
                DateScheduled = schedule,
                CoverId = coverId
            };

            _unitOfWork.Add(operation);
        }

        public async Task RemoveTeacherAdobeConnectAccess(string staffId, string roomId, DateTime scheduled, int coverId)
        {
            // Verify entries
            var checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);
            var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomId);

            if (checkStaff == null || checkRoom == null)
                return;

            var operation = new TeacherAdobeConnectOperation
            {
                ScoId = roomId,
                StaffId = staffId,
                Action = AdobeConnectOperationAction.Remove,
                DateScheduled = scheduled,
                CoverId = coverId
            };

            _unitOfWork.Add(operation);
        }

        public async Task CreateTeacherAdobeConnectGroupMembership(string staffId, AdobeConnectGroup groupName, DateTime scheduled)
        {
            // Verify entries
            var checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);

            if (checkStaff == null)
                return;

            var operation = new TeacherAdobeConnectGroupOperation
            {
                GroupSco = ((int)groupName).ToString(),
                GroupName = groupName.ToString(),
                TeacherId = staffId,
                Action = AdobeConnectOperationAction.Add,
                DateScheduled = scheduled
            };

            _unitOfWork.Add(operation);
        }

        public async Task RemoveTeacherAdobeConnectGroupMembership(string staffId, AdobeConnectGroup groupName, DateTime scheduled)
        {
            // Verify entries
            var checkStaff = await _unitOfWork.Staff.GetForExistCheck(staffId);

            if (checkStaff == null)
                return;

            var operation = new TeacherAdobeConnectGroupOperation
            {
                GroupSco = ((int)groupName).ToString(),
                GroupName = groupName.ToString(),
                TeacherId = staffId,
                Action = AdobeConnectOperationAction.Remove,
                DateScheduled = scheduled
            };

            _unitOfWork.Add(operation);
        }

        public async Task MarkAdobeConnectOperationComplete(int id)
        {
            // Validate entries
            var operation = await _unitOfWork.AdobeConnectOperations.ForProcessingAsync(id);

            if (operation == null)
                return;

            if (operation.IsCompleted || operation.IsDeleted)
                return;

            operation.IsCompleted = true;
        }

        public async Task MarkAdobeConnectOperationCompleteAsync(int id)
        {
            // Validate entries
            var operation = await _unitOfWork.AdobeConnectOperations.ForProcessingAsync(id);

            if (operation == null)
                return;

            if (operation.IsCompleted || operation.IsDeleted)
                return;

            operation.IsCompleted = true;
        }

        public async Task CancelAdobeConnectOperation(int id)
        {
            // Validate entries
            var operation = await _unitOfWork.AdobeConnectOperations.ForProcessingAsync(id);

            if (operation == null)
                return;

            CancelAdobeConnectOperation(operation);
        }

        public void CancelAdobeConnectOperation(AdobeConnectOperation operation)
        {
            if (operation.IsCompleted || operation.IsDeleted)
                return;

            operation.IsDeleted = true;
        }

        public async Task CreateStudentMSTeamMemberAccess(string studentId, int offeringId, DateTime schedule)
        {
            // Validate entries
            var student = await _unitOfWork.Students.GetForExistCheck(studentId);
            var offering = await _unitOfWork.CourseOfferings.GetForExistCheck(offeringId);

            if (student == null || offering == null)
                return;

            var operation = new StudentMSTeamOperation()
            {
                StudentId = studentId,
                OfferingId = offeringId,
                DateScheduled = schedule,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Member
            };

            _unitOfWork.Add(operation);
        }

        public async Task CreateCasualMSTeamMemberAccess(int casualId, int offeringId, int coverId, DateTime scheduled)
        {
            // Validate entries
            var casual = await _unitOfWork.Casuals.GetForExistCheck(casualId);
            var offering = await _unitOfWork.CourseOfferings.GetForExistCheck(offeringId);
            var cover = await _unitOfWork.Covers.GetForExistCheck(coverId);

            if (casual == null || offering == null || cover == null)
                return;

            // Create operation
            var operation = new CasualMSTeamOperation
            {
                OfferingId = offering.Id,
                CasualId = casual.Id,
                CoverId = cover.Id,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Member,
                DateScheduled = scheduled
            };

            _unitOfWork.Add(operation);
        }

        public async Task CreateTeacherMSTeamMemberAccess(string staffId, int offeringId, DateTime scheduled, int? coverId)
        {
            // Validate entries
            var staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);
            var offering = await _unitOfWork.CourseOfferings.GetForExistCheck(offeringId);

            if (staffMember == null || offering == null)
                return;

            // Create Operation
            var operation = new TeacherMSTeamOperation
            {
                OfferingId = offering.Id,
                StaffId = staffMember.StaffId,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Member,
                DateScheduled = scheduled,
                CoverId = coverId
            };

            _unitOfWork.Add(operation);
        }

        public async Task CreateCasualMSTeamOwnerAccess(int casualId, int offeringId, int coverId, DateTime scheduled)
        {
            // Validate entries
            var casual = await _unitOfWork.Casuals.GetForExistCheck(casualId);
            var offering = await _unitOfWork.CourseOfferings.GetForExistCheck(offeringId);
            var cover = await _unitOfWork.Covers.GetForExistCheck(coverId);

            if (casual == null || offering == null || cover == null)
                return;

            // Create operation
            var operation = new CasualMSTeamOperation
            {
                OfferingId = offering.Id,
                CasualId = casual.Id,
                CoverId = cover.Id,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = scheduled
            };

            _unitOfWork.Add(operation);
        }

        public async Task CreateTeacherMSTeamOwnerAccess(string staffId, int offeringId, DateTime scheduled, int? coverId)
        {
            // Validate entries
            var staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);
            var offering = await _unitOfWork.CourseOfferings.GetForExistCheck(offeringId);

            if (staffMember == null || offering == null)
                return;

            // Create Operation
            var operation = new TeacherMSTeamOperation
            {
                OfferingId = offering.Id,
                StaffId = staffMember.StaffId,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = scheduled,
                CoverId = coverId
            };

            _unitOfWork.Add(operation);
        }

        public async Task CreateClassroomMSTeam(int offeringId, DateTime scheduled)
        {
            // Validate entries
            var checkOffering = await _unitOfWork.CourseOfferings.GetForExistCheck(offeringId);

            if (checkOffering == null)
                return;

            // Create Operation
            var operation = new GroupMSTeamOperation
            {
                Faculty = checkOffering.Course.Faculty,
                OfferingId = checkOffering.Id,
                Offering = checkOffering,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Group,
                DateScheduled = scheduled
            };

            _unitOfWork.Add(operation);
        }

        public async Task CreateStudentEnrolmentMSTeamAccess(string studentId)
        {
            // Validate entries
            var student = await _unitOfWork.Students.GetForExistCheck(studentId);

            if (student == null)
                return;

            var operation = new StudentEnrolledMSTeamOperation()
            {
                StudentId = studentId,
                TeamName = MicrosoftTeam.Students,
                DateScheduled = DateTime.Now,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Member
            };

            _unitOfWork.Add(operation);
        }

        public async Task RemoveStudentEnrollmentMSTeamAccess(string studentId)
        {
            // Validate entries
            var student = await _unitOfWork.Students.GetForExistCheck(studentId);

            if (student == null)
                return;

            var operation = new StudentEnrolledMSTeamOperation()
            {
                StudentId = studentId,
                TeamName = MicrosoftTeam.Students,
                DateScheduled = DateTime.Now,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Member
            };

            _unitOfWork.Add(operation);
        }

        public async Task CreateTeacherEmployedMSTeamAccess(string staffId)
        {
            // Validate entries
            var staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);

            if (staffMember == null)
                return;

            // Create Operation
            var studentTeamOperation = new TeacherEmployedMSTeamOperation()
            {
                StaffId = staffId,
                TeamName = MicrosoftTeam.Students,
                Action = MSTeamOperationAction.Add,
                DateScheduled = DateTime.Now,
            };

            if (staffMember.Faculty.HasFlag(Faculty.Administration) ||
                staffMember.Faculty.HasFlag(Faculty.Executive) ||
                staffMember.Faculty.HasFlag(Faculty.Support))
            {
                studentTeamOperation.PermissionLevel = MSTeamOperationPermissionLevel.Owner;
            }
            else
            {
                studentTeamOperation.PermissionLevel = MSTeamOperationPermissionLevel.Member;
            }

            _unitOfWork.Add(studentTeamOperation);

            var schoolTeamOperation = new TeacherEmployedMSTeamOperation()
            {
                StaffId = staffId,
                TeamName = MicrosoftTeam.Staff,
                Action = MSTeamOperationAction.Add,
                DateScheduled = DateTime.Now,
                PermissionLevel = MSTeamOperationPermissionLevel.Member
            };

            _unitOfWork.Add(schoolTeamOperation);
        }

        public async Task RemoveTeacherEmployedMSTeamAccess(string staffId)
        {
            // Validate entries
            var staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);

            if (staffMember == null)
                return;

            // Create Operation
            var studentTeamOperation = new TeacherEmployedMSTeamOperation()
            {
                StaffId = staffId,
                TeamName = MicrosoftTeam.Students,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Member,
                DateScheduled = DateTime.Now,
            };

            _unitOfWork.Add(studentTeamOperation);

            var schoolTeamOperation = new TeacherEmployedMSTeamOperation()
            {
                StaffId = staffId,
                TeamName = MicrosoftTeam.Staff,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Member,
                DateScheduled = DateTime.Now,
            };

            _unitOfWork.Add(schoolTeamOperation);
        }

        public async Task CreateContactAddedMSTeamAccess(int contactId)
        {
            // Validate entries
            var contact = _unitOfWork.SchoolContacts.GetForExistCheck(contactId);

            if (contact == null)
                return;

            var primary = await _unitOfWork.SchoolContacts.IsContactAtPrimarySchool(contactId);
            var secondary = await _unitOfWork.SchoolContacts.IsContactAtSecondarySchool(contactId);

            if (secondary)
            {
                var operation = new ContactAddedMSTeamOperation()
                {
                    ContactId = contactId,
                    DateScheduled = DateTime.Now,
                    TeamName = MicrosoftTeam.SecondaryPartnerSchools,
                    Action = MSTeamOperationAction.Add,
                    PermissionLevel = MSTeamOperationPermissionLevel.Member
                };

                _unitOfWork.Add(operation);
            }

            if (primary)
            {
                var operation = new ContactAddedMSTeamOperation()
                {
                    ContactId = contactId,
                    DateScheduled = DateTime.Now,
                    TeamName = MicrosoftTeam.PrimaryPartnerSchools,
                    Action = MSTeamOperationAction.Add,
                    PermissionLevel = MSTeamOperationPermissionLevel.Member
                };

                _unitOfWork.Add(operation);
            }
        }

        public async Task RemoveContactAddedMSTeamAccess(int contactId)
        {
            // Validate entries
            var contact = _unitOfWork.SchoolContacts.GetForExistCheck(contactId);

            if (contact == null)
                return;

            var primary = await _unitOfWork.SchoolContacts.IsContactAtPrimarySchool(contactId);
            var secondary = await _unitOfWork.SchoolContacts.IsContactAtSecondarySchool(contactId);

            if (secondary)
            {
                var operation = new ContactAddedMSTeamOperation()
                {
                    ContactId = contactId,
                    DateScheduled = DateTime.Now,
                    TeamName = MicrosoftTeam.SecondaryPartnerSchools,
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Member
                };

                _unitOfWork.Add(operation);
            }

            if (primary)
            {
                var operation = new ContactAddedMSTeamOperation()
                {
                    ContactId = contactId,
                    DateScheduled = DateTime.Now,
                    TeamName = MicrosoftTeam.PrimaryPartnerSchools,
                    Action = MSTeamOperationAction.Remove,
                    PermissionLevel = MSTeamOperationPermissionLevel.Member
                };

                _unitOfWork.Add(operation);
            }
        }

        public async Task RemoveStudentMSTeamAccess(string studentId, int offeringId, DateTime schedule)
        {
            // Validate entries
            var student = await _unitOfWork.Students.GetForExistCheck(studentId);
            var offering = await _unitOfWork.CourseOfferings.GetForExistCheck(offeringId);

            if (student == null || offering == null)
                return;

            var operation = new StudentMSTeamOperation()
            {
                StudentId = studentId,
                OfferingId = offeringId,
                DateScheduled = schedule,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Member
            };

            _unitOfWork.Add(operation);
        }

        public async Task RemoveCasualMSTeamAccess(int casualId, int offeringId, int coverId, DateTime scheduled)
        {
            // Validate entries
            var casual = await _unitOfWork.Casuals.GetForExistCheck(casualId);
            var offering = await _unitOfWork.CourseOfferings.GetForExistCheck(offeringId);
            var cover = await _unitOfWork.Covers.GetForExistCheck(coverId);

            if (casual == null || offering == null || cover == null)
                return;

            // Create operation
            var operation = new CasualMSTeamOperation
            {
                OfferingId = offering.Id,
                CasualId = casual.Id,
                CoverId = cover.Id,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = scheduled
            };

            _unitOfWork.Add(operation);
        }

        public async Task RemoveTeacherMSTeamAccess(string staffId, int offeringId, DateTime scheduled, int? coverId)
        {
            // Validate entries
            var staffMember = await _unitOfWork.Staff.GetForExistCheck(staffId);
            var offering = await _unitOfWork.CourseOfferings.GetForExistCheck(offeringId);

            if (staffMember == null || offering == null)
                return;

            // Create Operation
            var operation = new TeacherMSTeamOperation
            {
                OfferingId = offering.Id,
                StaffId = staffMember.StaffId,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = scheduled,
                CoverId = coverId
            };

            _unitOfWork.Add(operation);
        }

        public async Task MarkMSTeamOperationComplete(int id)
        {
            // Validate entries
            var operation = await _unitOfWork.MSTeamOperations.ForMarkingCompleteOrCancelled(id);

            if (operation == null)
                return;

            operation.IsCompleted = true;
        }

        public async Task CancelMSTeamOperation(int id)
        {
            // Validate entries
            var operation = await _unitOfWork.MSTeamOperations.ForMarkingCompleteOrCancelled(id);

            if (operation == null)
                return;

            operation.IsDeleted = true;
        }

        public async Task AddStaffToAdobeGroupBasedOnFaculty(string staffId, Faculty staffFaculty)
        {
            if (staffFaculty.HasFlag(Faculty.Administration))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Administration, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.Executive))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Executive, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.English))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.English, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.Mathematics))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Mathematics, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.Science))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Science, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.Stage3))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Stage3, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.Support))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Support, DateTime.Now);
            }
        }

        public async Task AuditStaffAdobeGroupMembershipBasedOnFaculty(string staffId, Faculty originalFaculty, Faculty staffFaculty)
        {
            if (!originalFaculty.HasFlag(Faculty.Administration) && staffFaculty.HasFlag(Faculty.Administration))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Administration, DateTime.Now);
            }

            if (!staffFaculty.HasFlag(Faculty.Administration) && originalFaculty.HasFlag(Faculty.Administration))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Administration, DateTime.Now);
            }

            if (!originalFaculty.HasFlag(Faculty.Executive) && staffFaculty.HasFlag(Faculty.Executive))
            {
               await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Executive, DateTime.Now);
            }

            if (!staffFaculty.HasFlag(Faculty.Executive) && originalFaculty.HasFlag(Faculty.Executive))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Executive, DateTime.Now);
            }

            if (!originalFaculty.HasFlag(Faculty.English) && staffFaculty.HasFlag(Faculty.English))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.English, DateTime.Now);
            }

            if (!staffFaculty.HasFlag(Faculty.English) && originalFaculty.HasFlag(Faculty.English))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.English, DateTime.Now);
            }

            if (!originalFaculty.HasFlag(Faculty.Mathematics) && staffFaculty.HasFlag(Faculty.Mathematics))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Mathematics, DateTime.Now);
            }

            if (!staffFaculty.HasFlag(Faculty.Mathematics) && originalFaculty.HasFlag(Faculty.Mathematics))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Mathematics, DateTime.Now);
            }

            if (!originalFaculty.HasFlag(Faculty.Science) && staffFaculty.HasFlag(Faculty.Science))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Science, DateTime.Now);
            }

            if (!staffFaculty.HasFlag(Faculty.Science) && originalFaculty.HasFlag(Faculty.Science))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Science, DateTime.Now);
            }

            if (!originalFaculty.HasFlag(Faculty.Stage3) && staffFaculty.HasFlag(Faculty.Stage3))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Stage3, DateTime.Now);
            }

            if (!staffFaculty.HasFlag(Faculty.Stage3) && originalFaculty.HasFlag(Faculty.Stage3))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Stage3, DateTime.Now);
            }

            if (!originalFaculty.HasFlag(Faculty.Support) && staffFaculty.HasFlag(Faculty.Support))
            {
                await CreateTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Support, DateTime.Now);
            }

            if (!staffFaculty.HasFlag(Faculty.Support) && originalFaculty.HasFlag(Faculty.Support))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Support, DateTime.Now);
            }
        }

        public async Task RemoveStaffAdobeGroupMembershipBasedOnFaculty(string staffId, Faculty staffFaculty)
        {
            if (staffFaculty.HasFlag(Faculty.Administration))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Administration, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.Executive))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Executive, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.English))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.English, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.Mathematics))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Mathematics, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.Science))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Science, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.Stage3))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Stage3, DateTime.Now);
            }

            if (staffFaculty.HasFlag(Faculty.Support))
            {
                await RemoveTeacherAdobeConnectGroupMembership(staffId, AdobeConnectGroup.Support, DateTime.Now);
            }
        }

        public Task CreateCanvasUserFromStudent(Student student)
        {
            var operation = new CreateUserCanvasOperation()
            {
                UserId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                PortalUsername = student.PortalUsername,
                EmailAddress = student.EmailAddress
            };

            _unitOfWork.Add(operation);

            return Task.CompletedTask;
        }

        public Task CreateCanvasUserFromStaff(Staff staff)
        {
            var operation = new CreateUserCanvasOperation()
            {
                UserId = staff.StaffId,
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                PortalUsername = staff.PortalUsername,
                EmailAddress = staff.EmailAddress
            };

            _unitOfWork.Add(operation);

            return Task.CompletedTask;
        }

        public Task EnrolStudentInCanvasCourse(Student student, CourseOffering offering, DateTime? scheduledFor = null)
        {
            var operation = new ModifyEnrolmentCanvasOperation()
            {
                UserId = student.StudentId,
                UserType = "Student",
                CourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 1)}",
                Action = CanvasOperation.EnrolmentAction.Add
            };

            if (scheduledFor != null)
                operation.ScheduledFor = scheduledFor.Value;

            _unitOfWork.Add(operation);

            return Task.CompletedTask;
        }

        public Task EnrolStaffInCanvasCourse(Staff staff, CourseOffering offering, DateTime? scheduledFor = null)
        {
            var operation = new ModifyEnrolmentCanvasOperation()
            {
                UserId = staff.StaffId,
                UserType = "Teacher",
                CourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 1)}",
                Action = CanvasOperation.EnrolmentAction.Add
            };

            if (scheduledFor != null)
                operation.ScheduledFor = scheduledFor.Value;

            _unitOfWork.Add(operation);

            return Task.CompletedTask;
        }

        public Task UnenrolStudentFromCanvasCourse(Student student, CourseOffering offering, DateTime? scheduledFor = null)
        {
            var operation = new ModifyEnrolmentCanvasOperation()
            {
                UserId = student.StudentId,
                CourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 1)}",
                Action = CanvasOperation.EnrolmentAction.Remove
            };

            if (scheduledFor != null)
                operation.ScheduledFor = scheduledFor.Value;

            _unitOfWork.Add(operation);

            return Task.CompletedTask;
        }

        public Task UnenrolStaffFromCanvasCourse(Staff staff, CourseOffering offering, DateTime? scheduledFor = null)
        {
            var operation = new ModifyEnrolmentCanvasOperation()
            {
                UserId = staff.StaffId,
                CourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 1)}",
                Action = CanvasOperation.EnrolmentAction.Remove
            };

            if (scheduledFor != null)
                operation.ScheduledFor = scheduledFor.Value;

            _unitOfWork.Add(operation);

            return Task.CompletedTask;
        }

        public Task DisableCanvasUser(string UserId)
        {
            if (string.IsNullOrEmpty(UserId))
                return Task.CompletedTask;

            var operation = new DeleteUserCanvasOperation()
            {
                UserId = UserId
            };

            _unitOfWork.Add(operation);

            return Task.CompletedTask;
        }
    }
}