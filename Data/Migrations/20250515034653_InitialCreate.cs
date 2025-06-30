using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grad_Project_Dashboard_1.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Regex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DomainName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPInstance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstanceGroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoadBalancerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NetworkName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "userCustomRules",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CustomRuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userCustomRules", x => new { x.UserId, x.CustomRuleId });
                    table.ForeignKey(
                        name: "FK_userCustomRules_CustomRules_CustomRuleId",
                        column: x => x.CustomRuleId,
                        principalTable: "CustomRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userCustomRules_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_userCustomRules_CustomRuleId",
                table: "userCustomRules",
                column: "CustomRuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userCustomRules");

            migrationBuilder.DropTable(
                name: "CustomRules");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
