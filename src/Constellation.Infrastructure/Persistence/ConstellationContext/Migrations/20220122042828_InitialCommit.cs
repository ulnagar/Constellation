using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class InitialCommit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Absences_DiscountedWholeReasons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_DiscountedPartialReasons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_AbsenceScanStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Absences_PartialLengthThreshold = table.Column<int>(type: "int", nullable: true),
                    Absences_ForwardingEmailAbsenceCoordinator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_ForwardingEmailTruancyOfficer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_AbsenceCoordinatorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_AbsenceCoordinatorTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_AbsenceCoordinatorEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdobeConnectDefaultFolder = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentralContactPreference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonsCoordinatorEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonsCoordinatorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonsCoordinatorTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonsHeadTeacherEmail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetAccessTokens",
                columns: table => new
                {
                    AccessToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Expiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MapToUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedirectTo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetAccessTokens", x => x.AccessToken);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSchoolContact = table.Column<bool>(type: "bit", nullable: false),
                    SchoolContactId = table.Column<int>(type: "int", nullable: false),
                    IsStaffMember = table.Column<bool>(type: "bit", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CanvasOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledFor = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortalUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CourseId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanvasOperations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Make = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateWarrantyExpires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateReceived = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDisposed = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.SerialNumber);
                });

            migrationBuilder.CreateTable(
                name: "Lessons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DoNotGenerateRolls = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lessons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Periods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timetable = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Day = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    ScoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UrlPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Protected = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.ScoId);
                });

            migrationBuilder.CreateTable(
                name: "SchoolContact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateEntered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SelfRegistered = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolContact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Town = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    FaxNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Division = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeatSchool = table.Column<bool>(type: "bit", nullable: false),
                    Electorate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrincipalNetwork = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimetableApplication = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RollCallGroup = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => new { x.Name, x.Type });
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateEntered = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceNotes_Devices_SerialNumber",
                        column: x => x.SerialNumber,
                        principalTable: "Devices",
                        principalColumn: "SerialNumber",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Casuals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortalUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateEntered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdobeConnectPrincipalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Casuals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Casuals_Schools_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "Schools",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonRolls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: true),
                    SchoolContactId = table.Column<int>(type: "int", nullable: true),
                    LessonDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonRolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonRolls_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonRolls_SchoolContact_SchoolContactId",
                        column: x => x.SchoolContactId,
                        principalTable: "SchoolContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonRolls_Schools_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "Schools",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolContactRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolContactId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DateEntered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolContactRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolContactRole_SchoolContact_SchoolContactId",
                        column: x => x.SchoolContactId,
                        principalTable: "SchoolContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SchoolContactRole_Schools_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "Schools",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortalUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdobeConnectPrincipalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentralStudentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentGrade = table.Column<int>(type: "int", nullable: false),
                    EnrolledGrade = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateEntered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: true),
                    IncludeInAbsenceNotifications = table.Column<bool>(type: "bit", nullable: false),
                    AbsenceNotificationStartDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_Students_Schools_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "Schools",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "DeviceAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateAllocated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceAllocations_Devices_SerialNumber",
                        column: x => x.SerialNumber,
                        principalTable: "Devices",
                        principalColumn: "SerialNumber",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceAllocations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LessonRollStudentAttendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonRollId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Present = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonRollStudentAttendance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonRollStudentAttendance_LessonRolls_LessonRollId",
                        column: x => x.LessonRollId,
                        principalTable: "LessonRolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonRollStudentAttendance_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbsenceNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutgoingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmedDelivered = table.Column<bool>(type: "bit", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredMessageIds = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceNotification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbsenceResponse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    From = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerificationStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Verifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerificationComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Forwarded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceResponse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdobeConnectOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupSco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScoId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PrincipalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<int>(type: "int", nullable: false),
                    DateScheduled = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CoverId = table.Column<int>(type: "int", nullable: true),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CasualId = table.Column<int>(type: "int", nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TeacherId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdobeConnectOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdobeConnectOperations_Casuals_CasualId",
                        column: x => x.CasualId,
                        principalTable: "Casuals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdobeConnectOperations_Rooms_ScoId",
                        column: x => x.ScoId,
                        principalTable: "Rooms",
                        principalColumn: "ScoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdobeConnectOperations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "Covers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ClassworkNotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CasualId = table.Column<int>(type: "int", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Covers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Covers_Casuals_CasualId",
                        column: x => x.CasualId,
                        principalTable: "Casuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MSTeamOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<int>(type: "int", nullable: false),
                    PermissionLevel = table.Column<int>(type: "int", nullable: false),
                    DateScheduled = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CasualId = table.Column<int>(type: "int", nullable: true),
                    CoverId = table.Column<int>(type: "int", nullable: true),
                    OfferingId = table.Column<int>(type: "int", nullable: true),
                    ContactId = table.Column<int>(type: "int", nullable: true),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Faculty = table.Column<int>(type: "int", nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StudentMSTeamOperation_StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TeacherMSTeamOperation_StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TeacherMSTeamOperation_CoverId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MSTeamOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Casuals_CasualId",
                        column: x => x.CasualId,
                        principalTable: "Casuals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Covers_CoverId",
                        column: x => x.CoverId,
                        principalTable: "Covers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Covers_TeacherMSTeamOperation_CoverId",
                        column: x => x.TeacherMSTeamOperation_CoverId,
                        principalTable: "Covers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_SchoolContact_ContactId",
                        column: x => x.ContactId,
                        principalTable: "SchoolContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Students_StudentMSTeamOperation_StudentId",
                        column: x => x.StudentMSTeamOperation_StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "Absences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeriodTimeframe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AbsenceLength = table.Column<int>(type: "int", nullable: false),
                    AbsenceTimeframe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AbsenceReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ExternallyExplained = table.Column<bool>(type: "bit", nullable: false),
                    ExternalExplanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalExplanationSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateScanned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClassworkNotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Absences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Absences_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortalUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdobeConnectPrincipalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Faculty = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateEntered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: true),
                    ClassworkNotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.StaffId);
                    table.ForeignKey(
                        name: "FK_Staff_Schools_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "Schools",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    Faculty = table.Column<int>(type: "int", nullable: false),
                    HeadTeacherId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FullTimeEquivalentValue = table.Column<decimal>(type: "decimal(4,3)", precision: 4, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Staff_HeadTeacherId",
                        column: x => x.HeadTeacherId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateTable(
                name: "Offerings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offerings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offerings_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassworkNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    AbsenceDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassworkNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassworkNotifications_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassworkNotifications_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateTable(
                name: "CourseOfferingLesson",
                columns: table => new
                {
                    LessonsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseOfferingLesson", x => new { x.LessonsId, x.OfferingsId });
                    table.ForeignKey(
                        name: "FK_CourseOfferingLesson_Lessons_LessonsId",
                        column: x => x.LessonsId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseOfferingLesson_Offerings_OfferingsId",
                        column: x => x.OfferingsId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrolments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrolments_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrolments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfferingResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UrlPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    ShowLink = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferingResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferingResources_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartialAbsences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeriodTimeframe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartialAbsenceLength = table.Column<int>(type: "int", nullable: false),
                    PartialAbsenceTimeframe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternallyExplained = table.Column<bool>(type: "bit", nullable: false),
                    DateScanned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartialAbsences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartialAbsences_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PartialAbsences_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PeriodId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "ScoId");
                    table.ForeignKey(
                        name: "FK_Sessions_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateTable(
                name: "WholeAbsences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeriodTimeframe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternallyExplained = table.Column<bool>(type: "bit", nullable: false),
                    DateScanned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WholeAbsences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WholeAbsences_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WholeAbsences_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "PartialAbsenceNotifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutgoingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartialAbsenceNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartialAbsenceNotifications_PartialAbsences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "PartialAbsences",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartialAbsenceResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Verification = table.Column<int>(type: "int", nullable: false),
                    VerifierId = table.Column<int>(type: "int", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedComment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartialAbsenceResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartialAbsenceResponses_PartialAbsences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "PartialAbsences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartialAbsenceResponses_SchoolContact_VerifierId",
                        column: x => x.VerifierId,
                        principalTable: "SchoolContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartialAbsenceVerifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutgoingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartialAbsenceVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartialAbsenceVerifications_PartialAbsences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "PartialAbsences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WholeAbsenceNotifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutgoingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfirmedDelivered = table.Column<bool>(type: "bit", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredMessageIds = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WholeAbsenceNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WholeAbsenceNotifications_WholeAbsences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "WholeAbsences",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WholeAbsenceResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedFromName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Forwarded = table.Column<bool>(type: "bit", nullable: false),
                    ForwardedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WholeAbsenceResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WholeAbsenceResponses_WholeAbsences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "WholeAbsences",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceNotification_AbsenceId",
                table: "AbsenceNotification",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceResponse_AbsenceId",
                table: "AbsenceResponse",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_ClassworkNotificationId",
                table: "Absences",
                column: "ClassworkNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_OfferingId",
                table: "Absences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_StudentId",
                table: "Absences",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_CasualId",
                table: "AdobeConnectOperations",
                column: "CasualId");

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_CoverId",
                table: "AdobeConnectOperations",
                column: "CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_ScoId",
                table: "AdobeConnectOperations",
                column: "ScoId");

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_StaffId",
                table: "AdobeConnectOperations",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_StudentId",
                table: "AdobeConnectOperations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_TeacherId",
                table: "AdobeConnectOperations",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Casuals_SchoolCode",
                table: "Casuals",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_ClassworkNotifications_OfferingId",
                table: "ClassworkNotifications",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassworkNotifications_StaffId",
                table: "ClassworkNotifications",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingLesson_OfferingsId",
                table: "CourseOfferingLesson",
                column: "OfferingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_HeadTeacherId",
                table: "Courses",
                column: "HeadTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Covers_CasualId",
                table: "Covers",
                column: "CasualId");

            migrationBuilder.CreateIndex(
                name: "IX_Covers_ClassworkNotificationId",
                table: "Covers",
                column: "ClassworkNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Covers_OfferingId",
                table: "Covers",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Covers_StaffId",
                table: "Covers",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAllocations_SerialNumber",
                table: "DeviceAllocations",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAllocations_StudentId",
                table: "DeviceAllocations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceNotes_SerialNumber",
                table: "DeviceNotes",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_OfferingId",
                table: "Enrolments",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_StudentId",
                table: "Enrolments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonRolls_LessonId",
                table: "LessonRolls",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonRolls_SchoolCode",
                table: "LessonRolls",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_LessonRolls_SchoolContactId",
                table: "LessonRolls",
                column: "SchoolContactId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonRollStudentAttendance_LessonRollId",
                table: "LessonRollStudentAttendance",
                column: "LessonRollId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonRollStudentAttendance_StudentId",
                table: "LessonRollStudentAttendance",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_CasualId",
                table: "MSTeamOperations",
                column: "CasualId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_ContactId",
                table: "MSTeamOperations",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_CoverId",
                table: "MSTeamOperations",
                column: "CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_OfferingId",
                table: "MSTeamOperations",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StaffId",
                table: "MSTeamOperations",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StudentId",
                table: "MSTeamOperations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentMSTeamOperation_StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_TeacherMSTeamOperation_CoverId",
                table: "MSTeamOperations",
                column: "TeacherMSTeamOperation_CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                column: "TeacherMSTeamOperation_StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferingResources_OfferingId",
                table: "OfferingResources",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Offerings_CourseId",
                table: "Offerings",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsenceNotifications_AbsenceId",
                table: "PartialAbsenceNotifications",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsenceResponses_AbsenceId",
                table: "PartialAbsenceResponses",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsenceResponses_VerifierId",
                table: "PartialAbsenceResponses",
                column: "VerifierId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsences_OfferingId",
                table: "PartialAbsences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsences_StudentId",
                table: "PartialAbsences",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsenceVerifications_AbsenceId",
                table: "PartialAbsenceVerifications",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolContactRole_SchoolCode",
                table: "SchoolContactRole",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolContactRole_SchoolContactId",
                table: "SchoolContactRole",
                column: "SchoolContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_OfferingId",
                table: "Sessions",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PeriodId",
                table: "Sessions",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RoomId",
                table: "Sessions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_StaffId",
                table: "Sessions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_ClassworkNotificationId",
                table: "Staff",
                column: "ClassworkNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_SchoolCode",
                table: "Staff",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolCode",
                table: "Students",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_WholeAbsenceNotifications_AbsenceId",
                table: "WholeAbsenceNotifications",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WholeAbsenceResponses_AbsenceId",
                table: "WholeAbsenceResponses",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WholeAbsences_OfferingId",
                table: "WholeAbsences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_WholeAbsences_StudentId",
                table: "WholeAbsences",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceNotification_Absences_AbsenceId",
                table: "AbsenceNotification",
                column: "AbsenceId",
                principalTable: "Absences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceResponse_Absences_AbsenceId",
                table: "AbsenceResponse",
                column: "AbsenceId",
                principalTable: "Absences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdobeConnectOperations_Covers_CoverId",
                table: "AdobeConnectOperations",
                column: "CoverId",
                principalTable: "Covers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AdobeConnectOperations_Staff_StaffId",
                table: "AdobeConnectOperations",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdobeConnectOperations_Staff_TeacherId",
                table: "AdobeConnectOperations",
                column: "TeacherId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Covers_ClassworkNotifications_ClassworkNotificationId",
                table: "Covers",
                column: "ClassworkNotificationId",
                principalTable: "ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Covers_Offerings_OfferingId",
                table: "Covers",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Covers_Staff_StaffId",
                table: "Covers",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Offerings_OfferingId",
                table: "MSTeamOperations",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Staff_StaffId",
                table: "MSTeamOperations",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Staff_TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                column: "TeacherMSTeamOperation_StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_ClassworkNotifications_ClassworkNotificationId",
                table: "Absences",
                column: "ClassworkNotificationId",
                principalTable: "ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Offerings_OfferingId",
                table: "Absences",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_ClassworkNotifications_ClassworkNotificationId",
                table: "Staff",
                column: "ClassworkNotificationId",
                principalTable: "ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staff_ClassworkNotifications_ClassworkNotificationId",
                table: "Staff");

            migrationBuilder.DropTable(
                name: "AbsenceNotification");

            migrationBuilder.DropTable(
                name: "AbsenceResponse");

            migrationBuilder.DropTable(
                name: "AdobeConnectOperations");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "AspNetAccessTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CanvasOperations");

            migrationBuilder.DropTable(
                name: "CourseOfferingLesson");

            migrationBuilder.DropTable(
                name: "DeviceAllocations");

            migrationBuilder.DropTable(
                name: "DeviceNotes");

            migrationBuilder.DropTable(
                name: "Enrolments");

            migrationBuilder.DropTable(
                name: "LessonRollStudentAttendance");

            migrationBuilder.DropTable(
                name: "MSTeamOperations");

            migrationBuilder.DropTable(
                name: "OfferingResources");

            migrationBuilder.DropTable(
                name: "PartialAbsenceNotifications");

            migrationBuilder.DropTable(
                name: "PartialAbsenceResponses");

            migrationBuilder.DropTable(
                name: "PartialAbsenceVerifications");

            migrationBuilder.DropTable(
                name: "SchoolContactRole");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Setting");

            migrationBuilder.DropTable(
                name: "WholeAbsenceNotifications");

            migrationBuilder.DropTable(
                name: "WholeAbsenceResponses");

            migrationBuilder.DropTable(
                name: "Absences");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "LessonRolls");

            migrationBuilder.DropTable(
                name: "Covers");

            migrationBuilder.DropTable(
                name: "PartialAbsences");

            migrationBuilder.DropTable(
                name: "Periods");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "WholeAbsences");

            migrationBuilder.DropTable(
                name: "Lessons");

            migrationBuilder.DropTable(
                name: "SchoolContact");

            migrationBuilder.DropTable(
                name: "Casuals");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "ClassworkNotifications");

            migrationBuilder.DropTable(
                name: "Offerings");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "Schools");
        }
    }
}
