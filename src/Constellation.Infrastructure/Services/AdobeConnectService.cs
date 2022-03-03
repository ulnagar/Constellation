using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC Operations
    public class AdobeConnectService : IAdobeConnectService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdobeConnectGateway _adobeConnect;
        private readonly IAdobeConnectRoomService _roomService;
        private readonly IStudentService _studentService;
        private readonly IStaffService _staffService;
        private readonly ICasualService _casualService;

        public AdobeConnectService(IUnitOfWork unitOfWork, IAdobeConnectGateway adobeConnectServer,
            IAdobeConnectRoomService roomService, IStudentService studentService,
            IStaffService staffService, ICasualService casualService)
        {
            _unitOfWork = unitOfWork;

            _adobeConnect = adobeConnectServer;
            _roomService = roomService;
            _studentService = studentService;
            _staffService = staffService;
            _casualService = casualService;
        }

        public async Task<string> CreateRoom(CourseOffering offering)
        {
            var zeroFillGrade = ((int)offering.Course.Grade).ToString();
            if (zeroFillGrade.Length == 1)
            {
                zeroFillGrade = "0" + zeroFillGrade;
            }

            var resource = await _adobeConnect.CreateRoom(
                name: "Aurora College - " + offering.EndDate.Year + " - " + offering.Name,
                year: offering.EndDate.Year.ToString(),
                grade: "Year " + zeroFillGrade,
                dateStart: offering.StartDate.Year + "-" + offering.StartDate.Month + "-" + offering.StartDate.Day,
                dateEnd: offering.EndDate.Year + "-" + offering.EndDate.Month + "-" + offering.EndDate.Day,
                urlPath: "aurora-" + offering.EndDate.Year + "-" + offering.Name,
                useTemplate: true,
                faculty: offering.Course.Faculty.ToString(),
                detectParentFolder: true,
                parentFolder: ""
            );

            await _roomService.CreateRoom(resource);
            await _unitOfWork.CompleteAsync();

            return resource.ScoId;
        }

        public Task<ICollection<string>> GetSessionsForDate(string scoId, DateTime sessionDate)
        {
            return _adobeConnect.GetSessionsForDate(scoId, sessionDate);
        }

        public Task<ICollection<AdobeConnectSessionUserDetailDto>> GetSessionUserDetails(string scoId, string assetId)
        {
            return _adobeConnect.GetSessionUserDetails(scoId, assetId);
        }

        public Task<string> GetUserPrincipalId(string username)
        {
            return _adobeConnect.GetUserPrincipalId(username);
        }

        public async Task<IEnumerable<AdobeConnectRoomDto>> UpdateRooms(string folderSco)
        {
            var returnList = new List<AdobeConnectRoomDto>();

            var response = await _adobeConnect.ListRooms(folderSco);

            var databaseRooms = _unitOfWork.AdobeConnectRooms.AllActive().ToList();

            var serverRoomSCO = response.Select(r => r.ScoId).ToList();
            var databaseRoomSCO = databaseRooms.Select(r => r.ScoId).ToList();

            var newRooms = serverRoomSCO.Except(databaseRoomSCO);
            var deletedRooms = databaseRoomSCO.Except(serverRoomSCO);

            foreach (var newRoomSco in newRooms)
            {
                var room = response.FirstOrDefault(r => r.ScoId == newRoomSco);

                if (room == null)
                    continue;

                if (_unitOfWork.AdobeConnectRooms.WithDetails(newRoomSco) != null)
                    continue;

                returnList.Add(new AdobeConnectRoomDto
                {
                    Action = "Add",
                    ScoId = room.ScoId,
                    Name = room.Name,
                    UrlPath = room.UrlPath
                });

                await _roomService.CreateRoom(room);
            }

            foreach (var deletedRoomSco in deletedRooms)
            {
                var room = databaseRooms.FirstOrDefault(r => r.ScoId == deletedRoomSco);

                if (room == null)
                    continue;

                if (room.Protected)
                    continue;

                returnList.Add(new AdobeConnectRoomDto
                {
                    Action = "Remove",
                    ScoId = room.ScoId,
                    Name = room.Name,
                    UrlPath = room.UrlPath
                });

                await _roomService.RemoveRoom(room.ScoId);
            }

            await _unitOfWork.CompleteAsync();

            return returnList;
        }

        public async Task<IEnumerable<AdobeConnectUserDetailDto>> UpdateUsers()
        {
            var returnList = new List<AdobeConnectUserDetailDto>();

            var students = await _unitOfWork.Students.WithoutAdobeConnectDetailsForUpdate();

            foreach (var student in students)
            {
                var acPID = await _adobeConnect.GetUserPrincipalId(student.PortalUsername);

                if (!string.IsNullOrWhiteSpace(acPID))
                {
                    var studentResource = new StudentDto
                    {
                        AdobeConnectPrincipalId = acPID
                    };

                    await _studentService.UpdateStudent(student.StudentId, studentResource);

                    returnList.Add(new AdobeConnectUserDetailDto { ScoId = acPID, UserId = student.StudentId, UserType = "Students", DisplayName = student.DisplayName });
                }
            }

            var teachers = _unitOfWork.Staff.AllWithoutAdobeConnectInfo();

            foreach (var teacher in teachers)
            {
                var acPID = await _adobeConnect.GetUserPrincipalId(teacher.PortalUsername);

                if (!string.IsNullOrWhiteSpace(acPID))
                {
                    var staffResource = new StaffDto
                    {
                        AdobeConnectPrincipalId = acPID
                    };

                    await _staffService.UpdateStaffMember(teacher.StaffId, staffResource);

                    returnList.Add(new AdobeConnectUserDetailDto { ScoId = acPID, UserId = teacher.StaffId, UserType = "Staff", DisplayName = teacher.DisplayName });
                }
            }

            var casuals = _unitOfWork.Casuals.AllWithoutAdobeConnectDetails();

            foreach (var casual in casuals)
            {
                var acPID = await _adobeConnect.GetUserPrincipalId(casual.PortalUsername);

                if (!string.IsNullOrWhiteSpace(acPID))
                {
                    var casualResource = new CasualDto
                    {
                        AdobeConnectPrincipalId = acPID
                    };

                    await _casualService.UpdateCasual(casual.Id, casualResource);

                    returnList.Add(new AdobeConnectUserDetailDto { ScoId = acPID, UserId = casual.Id.ToString(), UserType = "Casual", DisplayName = casual.DisplayName });
                }
            }

            await _unitOfWork.CompleteAsync();

            return returnList;
        }

        public async Task<ServiceOperationResult<T>> ProcessOperation<T>(T operation) where T : AdobeConnectOperation
        {
            var result = new ServiceOperationResult<T>
            {
                Success = false
            };

            result.Errors.Add($" Processing operation {operation.Id}");

            var target = "room";
            var principalId = "";
            var accessLevel = typeof(T).Name == "StudentAdobeConnectOperation" ? AdobeConnectAccessLevel.Student : AdobeConnectAccessLevel.Teacher;
            bool success = false;

            switch (operation.GetType().Name)
            {
                case "StudentAdobeConnectOperation":
                    var sOperation = operation as StudentAdobeConnectOperation;
                    principalId = string.IsNullOrWhiteSpace(sOperation.PrincipalId) ? sOperation.Student.AdobeConnectPrincipalId : sOperation.PrincipalId;
                    result.Errors.Add($"  Attempting to {operation.Action} student {sOperation.Student.DisplayName} in room {sOperation.Room.Name}");
                    if (string.IsNullOrWhiteSpace(principalId))
                    {
                        principalId = await GetUserPrincipalId(sOperation.Student.PortalUsername);

                        if (string.IsNullOrWhiteSpace(principalId))
                        {
                            result.Errors.Add($"   Could not determine user PrincipalId while processing operation with Id {operation.Id}");
                            return result;
                        }
                    }
                    break;

                case "CasualAdobeConnectOperation":
                    var cOperation = operation as CasualAdobeConnectOperation;
                    principalId = string.IsNullOrWhiteSpace(cOperation.PrincipalId) ? cOperation.Casual.AdobeConnectPrincipalId : cOperation.PrincipalId;
                    result.Errors.Add($"  Attempting to {operation.Action} casual teacher {cOperation.Casual.DisplayName} in room {cOperation.Room.Name}");
                    if (string.IsNullOrWhiteSpace(principalId))
                    {
                        principalId = await GetUserPrincipalId(cOperation.Casual.PortalUsername);

                        if (string.IsNullOrWhiteSpace(principalId))
                        {
                            result.Errors.Add($"   Could not determine user PrincipalId while processing operation with Id {operation.Id}");
                            return result;
                        }
                    }
                    break;

                case "TeacherAdobeConnectOperation":
                    var tOperation = operation as TeacherAdobeConnectOperation;
                    principalId = string.IsNullOrWhiteSpace(tOperation.PrincipalId) ? tOperation.Teacher.AdobeConnectPrincipalId : tOperation.PrincipalId;
                    result.Errors.Add($"  Attempting to {operation.Action} teacher {tOperation.Teacher.DisplayName} in room {tOperation.Room.Name}");
                    if (string.IsNullOrWhiteSpace(principalId))
                    {
                        principalId = await GetUserPrincipalId(tOperation.Teacher.PortalUsername);

                        if (string.IsNullOrWhiteSpace(principalId))
                        {
                            result.Errors.Add($"   Could not determine user PrincipalId while processing operation with Id {operation.Id}");
                            return result;
                        }
                    }
                    break;
                case "TeacherAdobeConnectGroupOperation":
                    var tgOperation = operation as TeacherAdobeConnectGroupOperation;
                    target = "group";
                    principalId = string.IsNullOrWhiteSpace(tgOperation.PrincipalId) ? tgOperation.Teacher.AdobeConnectPrincipalId : tgOperation.PrincipalId;
                    result.Errors.Add($"  Attempting to {operation.Action} teacher {tgOperation.Teacher.DisplayName} in group {tgOperation.GroupName}");
                    if (string.IsNullOrWhiteSpace(principalId))
                    {
                        principalId = await GetUserPrincipalId(tgOperation.Teacher.PortalUsername);

                        if (string.IsNullOrWhiteSpace(principalId))
                        {
                            result.Errors.Add($"   Could not determine user PrincipalId while processing operation with Id {operation.Id}");
                            return result;
                        }
                    }
                    break;
            }

            switch (operation.Action)
            {
                case AdobeConnectOperationAction.Add:
                    if (target == "room")
                        success = await _adobeConnect.UserPermissionUpdate(principalId, operation.ScoId, accessLevel);
                    if (target == "group")
                        success = await _adobeConnect.GroupMembershipUpdate(principalId, operation.GroupSco, AdobeConnectAccessLevel.Teacher);
                    break;
                case AdobeConnectOperationAction.Remove:
                    if (target == "room")
                    {
                        accessLevel = AdobeConnectAccessLevel.Remove;
                        success = await _adobeConnect.UserPermissionUpdate(principalId, operation.ScoId, accessLevel);
                    }
                    if (target == "group")
                        success = await _adobeConnect.GroupMembershipUpdate(principalId, operation.GroupSco, AdobeConnectAccessLevel.Remove);
                    break;
            }

            if (success)
            {
                operation.IsCompleted = true;
                result.Errors.Add($" Successfully processed operation.");
                result.Success = true;
                return result;
            }
            else
            {
                result.Errors.Add($" An error occured while processing operation with Id {operation.Id}");
                return result;
            }
        }

        public Task<ICollection<string>> GetCurrentSessionUsersAsync(string scoId, string assetId)
        {
            return _adobeConnect.GetSessionUsers(scoId, assetId);
        }

        public Task<string> GetCurrentSessionAsync(string scoId)
        {
            return _adobeConnect.GetCurrentSession(scoId);
        }
    }
}
