namespace Constellation.Presentation.Shared.Extensions;

using Serilog;

public static class LoggerExtensions
{
    private static string Application => "APPLICATION";
    private static string StaffPortal => "Staff Portal";
    private static string StudentPortal => "Student Portal";
    private static string ParentPortal => "Parent Portal";
    private static string SchoolsPortal => "Schools Portal";

    extension(ILogger logger)
    {
        private ILogger ForApplication(string application) =>
            logger.ForContext(Application, application);

        public ILogger ForStaffPortal() =>
            logger.ForApplication(StaffPortal);

        public ILogger ForParentPortal() =>
            logger.ForApplication(ParentPortal);

        public ILogger ForSchoolPortal() =>
            logger.ForApplication(SchoolsPortal);

        public ILogger ForStudentPortal() =>
            logger.ForApplication(StudentPortal);
    }
}
