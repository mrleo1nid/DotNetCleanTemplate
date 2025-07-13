using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetCleanTemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        private const string TimestampWithTimeZone = "timestamp with time zone";
        private const string UsersTable = "Users";
        private const string UserRolesTable = "UserRoles";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name_Value = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: TimestampWithTimeZone,
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTime>(
                        type: TimestampWithTimeZone,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: UsersTable,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name_Value = table.Column<string>(type: "text", nullable: false),
                    Email_Value = table.Column<string>(type: "text", nullable: false),
                    PasswordHash_Value = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: TimestampWithTimeZone,
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTime>(
                        type: TimestampWithTimeZone,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: UserRolesTable,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: TimestampWithTimeZone,
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTime>(
                        type: TimestampWithTimeZone,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: UsersTable,
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: UserRolesTable,
                column: "RoleId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: UserRolesTable,
                columns: new[] { "UserId", "RoleId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_Value",
                table: UsersTable,
                column: "Email_Value",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: UserRolesTable);

            migrationBuilder.DropTable(name: "Roles");

            migrationBuilder.DropTable(name: UsersTable);
        }
    }
}
