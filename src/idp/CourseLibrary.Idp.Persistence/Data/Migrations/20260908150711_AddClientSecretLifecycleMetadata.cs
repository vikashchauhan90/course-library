using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseLibrary.Idp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientSecretLifecycleMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "secret_created_at",
                schema: "oauth",
                table: "applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "secret_expires_at",
                schema: "oauth",
                table: "applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "secret_rotated_at",
                schema: "oauth",
                table: "applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_applications_secret_expires_at",
                schema: "oauth",
                table: "applications",
                column: "secret_expires_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_applications_secret_expires_at",
                schema: "oauth",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "secret_created_at",
                schema: "oauth",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "secret_expires_at",
                schema: "oauth",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "secret_rotated_at",
                schema: "oauth",
                table: "applications");
        }
    }
}
