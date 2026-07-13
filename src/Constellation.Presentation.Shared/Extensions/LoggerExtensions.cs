namespace Constellation.Presentation.Shared.Extensions;

using Constellation.Presentation.Shared.Helpers.Logging;
using Serilog;

public static class LoggerExtensions
{
    private static ILogger ForApplication(this ILogger logger, string application) =>
        logger.ForContext(LogDefaults.Application, application);

    public static ILogger ForStaffPortal(this ILogger logger) =>
        logger.ForApplication(LogDefaults.StaffPortal);

    public static ILogger ForParentPortal(this ILogger logger) =>
        logger.ForApplication(LogDefaults.ParentPortal);

    public static ILogger ForSchoolPortal(this ILogger logger) =>
        logger.ForApplication(LogDefaults.SchoolsPortal);

    public static ILogger ForStudentPortal(this ILogger logger) =>
        logger.ForApplication(LogDefaults.StudentPortal);
}
