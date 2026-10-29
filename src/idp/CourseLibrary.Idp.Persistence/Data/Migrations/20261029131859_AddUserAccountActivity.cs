using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseLibrary.Idp.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAccountActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_login_at",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "password_changed_at",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_login_at",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_changed_at",
                schema: "identity",
                table: "users");
        }
    }
}
