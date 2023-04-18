namespace Constellation.Application.Tests.Unit.Reports.Events;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Reports.Events;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Reports;
using Moq;

public class EmailToNonResidentialParentsTests
{
    private readonly Mock<IAcademicReportRepository> _reportRepositoryMock;
    private readonly Mock<IStudentRepository> _studentRepositoryMock;
    private readonly Mock<IFamilyRepository> _familyRepositoryMock;
    private readonly Mock<IStoredFileRepository> _fileRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<Serilog.ILogger> _loggerMock;

    public EmailToNonResidentialParentsTests()
    {
        _reportRepositoryMock = new();
        _studentRepositoryMock = new();
        _familyRepositoryMock = new();
        _fileRepositoryMock = new();
        _emailServiceMock = new();
        _loggerMock = new();
    }

    [Fact]
    public async Task Handle_ShouldLogWarningAndFail_WhenReportCannotBeFoundInDatabase()
    {
        // Arrange
        var notification = new AcademicReportCreatedDomainEvent(new DomainEventId(), new AcademicReportId());

        _reportRepositoryMock.Setup(
            x => x.GetById(
                It.IsAny<AcademicReportId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null)
            .Verifiable();

        _loggerMock.Setup(
            x => x.ForContext<AcademicReportCreatedDomainEvent>())
            .Returns(_loggerMock.Object);

        _loggerMock.Setup(
            x => x.Warning(
                It.IsAny<string>(),
                It.Is<AcademicReportCreatedDomainEvent>(entry => entry == notification)))
            .Verifiable();

        _fileRepositoryMock.Setup(
            x => x.GetAcademicReportByLinkId(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null)
            .Verifiable();

        var handler = new AcademicReportCreatedDomainEvent_EmailToNonResidentialParents(
            _reportRepositoryMock.Object,
            _studentRepositoryMock.Object,
            _familyRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);

        // Act
        await handler.Handle(notification, default);

        // Assert
        _reportRepositoryMock.Verify();
        _loggerMock.Verify();
        _fileRepositoryMock.Verify(mock => mock.GetAcademicReportByLinkId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Handle_ShouldLogWarningAndFail_WhenStoredFileCannotBeFoundInDatabase()
    {
        // Arrange
        var notification = new AcademicReportCreatedDomainEvent(new DomainEventId(), new AcademicReportId());

        var academicReport = AcademicReport.Create(
            notification.ReportId,
            "123456789",
            "1234",
            "2023",
            "Year 7, Semester 1, 2023");

        _reportRepositoryMock.Setup(
            x => x.GetById(
                It.IsAny<AcademicReportId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => academicReport)
            .Verifiable();

        _loggerMock.Setup(
            x => x.ForContext<AcademicReportCreatedDomainEvent>())
            .Returns(_loggerMock.Object);

        _loggerMock.Setup(
            x => x.Warning(
                It.IsAny<string>(),
                It.Is<AcademicReportCreatedDomainEvent>(entry => entry == notification)))
            .Verifiable();

        _fileRepositoryMock.Setup(
            x => x.GetAcademicReportByLinkId(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null)
            .Verifiable();

        var handler = new AcademicReportCreatedDomainEvent_EmailToNonResidentialParents(
            _reportRepositoryMock.Object,
            _studentRepositoryMock.Object,
            _familyRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);

        // Act
        await handler.Handle(notification, default);

        // Assert
        _reportRepositoryMock.Verify();
        _fileRepositoryMock.Verify(mock => mock.GetAcademicReportByLinkId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
        _loggerMock.Verify();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenNoFamiliesCanBeFoundInDatabase()
    {
        // Arrange
        var notification = new AcademicReportCreatedDomainEvent(new DomainEventId(), new AcademicReportId());

        var academicReport = AcademicReport.Create(
            notification.ReportId,
            "123456789",
            "1234",
            "2023",
            "Year 7, Semester 1, 2023");

        var storedFile = new StoredFile
        {
            Id = 1,
            Name = "Test.pdf",
            FileData = new byte[100],
            FileType = "application/pdf",
            CreatedAt = DateTime.Now,
            LinkType = StoredFile.StudentReport,
            LinkId = academicReport.Id.ToString()
        };

        _reportRepositoryMock.Setup(
            x => x.GetById(
                It.IsAny<AcademicReportId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => academicReport)
            .Verifiable();

        _loggerMock.Setup(
            x => x.ForContext<AcademicReportCreatedDomainEvent>())
            .Returns(_loggerMock.Object);

        _loggerMock.Setup(
            x => x.Warning(
                It.IsAny<string>(),
                It.Is<AcademicReportCreatedDomainEvent>(entry => entry == notification)))
            .Verifiable();

        _fileRepositoryMock.Setup(
            x => x.GetAcademicReportByLinkId(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => storedFile)
            .Verifiable();

        _familyRepositoryMock.Setup(
            x => x.GetFamiliesByStudentId(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null)
            .Verifiable();

        var handler = new AcademicReportCreatedDomainEvent_EmailToNonResidentialParents(
            _reportRepositoryMock.Object,
            _studentRepositoryMock.Object,
            _familyRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);

        // Act
        await handler.Handle(notification, default);

        // Assert
        _reportRepositoryMock.Verify();
        _fileRepositoryMock.Verify();
        _loggerMock.Verify(mock => mock.Warning(It.IsAny<string>(), It.IsAny<AcademicReportCreatedDomainEvent>()), Times.Never());
        _familyRepositoryMock.Verify();
    }
}