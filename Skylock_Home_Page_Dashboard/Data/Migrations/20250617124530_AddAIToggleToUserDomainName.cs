using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grad_Project_Dashboard_1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAIToggleToUserDomainName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AIToggle",
                table: "UserDomainNames",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIToggle",
                table: "UserDomainNames");
        }
    }
}
