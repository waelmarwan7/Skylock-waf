using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grad_Project_Dashboard_1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDomainNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userCustomRules_CustomRules_CustomRuleId",
                table: "userCustomRules");

            migrationBuilder.DropForeignKey(
                name: "FK_userCustomRules_Users_UserId",
                table: "userCustomRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_userCustomRules",
                table: "userCustomRules");

            migrationBuilder.RenameTable(
                name: "userCustomRules",
                newName: "UserCustomRules");

            migrationBuilder.RenameIndex(
                name: "IX_userCustomRules_CustomRuleId",
                table: "UserCustomRules",
                newName: "IX_UserCustomRules_CustomRuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCustomRules",
                table: "UserCustomRules",
                columns: new[] { "UserId", "CustomRuleId" });

            migrationBuilder.CreateTable(
                name: "UserDomainNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDomainNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDomainNames_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDomainNames_UserId",
                table: "UserDomainNames",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCustomRules_CustomRules_CustomRuleId",
                table: "UserCustomRules",
                column: "CustomRuleId",
                principalTable: "CustomRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCustomRules_Users_UserId",
                table: "UserCustomRules",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCustomRules_CustomRules_CustomRuleId",
                table: "UserCustomRules");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCustomRules_Users_UserId",
                table: "UserCustomRules");

            migrationBuilder.DropTable(
                name: "UserDomainNames");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCustomRules",
                table: "UserCustomRules");

            migrationBuilder.RenameTable(
                name: "UserCustomRules",
                newName: "userCustomRules");

            migrationBuilder.RenameIndex(
                name: "IX_UserCustomRules_CustomRuleId",
                table: "userCustomRules",
                newName: "IX_userCustomRules_CustomRuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_userCustomRules",
                table: "userCustomRules",
                columns: new[] { "UserId", "CustomRuleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_userCustomRules_CustomRules_CustomRuleId",
                table: "userCustomRules",
                column: "CustomRuleId",
                principalTable: "CustomRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userCustomRules_Users_UserId",
                table: "userCustomRules",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
