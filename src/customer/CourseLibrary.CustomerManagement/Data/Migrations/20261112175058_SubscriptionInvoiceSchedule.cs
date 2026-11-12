using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseLibrary.CustomerManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionInvoiceSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "next_invoice_at",
                schema: "customer_management",
                table: "organization_subscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE customer_management.organization_subscriptions
                  SET next_invoice_at = selected_at;");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "next_invoice_at",
                schema: "customer_management",
                table: "organization_subscriptions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "next_invoice_at",
                schema: "customer_management",
                table: "organization_subscriptions");
        }
    }
}
