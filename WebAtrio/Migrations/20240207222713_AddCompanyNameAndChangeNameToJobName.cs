using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAtrio.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyNameAndChangeNameToJobName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Jobs",
                newName: "JobName");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "JobName",
                table: "Jobs",
                newName: "Name");
        }
    }
}
