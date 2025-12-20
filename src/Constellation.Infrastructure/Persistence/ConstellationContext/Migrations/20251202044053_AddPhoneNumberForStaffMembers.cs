using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneNumberForStaffMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "SchoolContacts_Contacts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.Sql(@"
                UPDATE [dbo].[SchoolContacts_Contacts]
                SET PhoneNumber = null
                WHERE 
                    PhoneNumber = '' or
                    LEN(PhoneNumber) < 10;");
            
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "Staff",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE [Staff].[Members]
                SET PhoneNumber = null
                WHERE 1=1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "Staff",
                table: "Members");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "SchoolContacts_Contacts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
