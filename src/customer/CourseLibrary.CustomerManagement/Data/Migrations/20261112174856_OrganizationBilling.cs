using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseLibrary.CustomerManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "billing_interval",
                schema: "customer_management",
                table: "product_plans",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currency_code",
                schema: "customer_management",
                table: "product_plans",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price_amount",
                schema: "customer_management",
                table: "product_plans",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "billing_accounts",
                schema: "customer_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    billing_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    address_line1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_line2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    region = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    postal_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    tax_identifier = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    next_invoice_number = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_billing_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_billing_accounts_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "customer_management",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_invoices",
                schema: "customer_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    billing_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    bill_to_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    bill_to_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    bill_to_address = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_invoices", x => x.id);
                    table.ForeignKey(
                        name: "fk_organization_invoices_billing_accounts_billing_account_id",
                        column: x => x.billing_account_id,
                        principalSchema: "customer_management",
                        principalTable: "billing_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                @"INSERT INTO customer_management.billing_accounts
                    (id, organization_id, legal_name, billing_email, next_invoice_number, created_at, updated_at)
                  SELECT organization.id,
                         organization.id,
                         organization.name,
                         COALESCE(owner.email, ''),
                         1,
                         now(),
                         now()
                  FROM customer_management.organizations AS organization
                  LEFT JOIN customer_management.organization_memberships AS owner
                    ON owner.organization_id = organization.id AND owner.role = 'Owner';");

            migrationBuilder.CreateTable(
                name: "invoice_line_items",
                schema: "customer_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    plan_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    billing_interval = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    line_amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    period_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    period_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_line_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoice_line_items_organization_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalSchema: "customer_management",
                        principalTable: "organization_invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_payments",
                schema: "customer_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    external_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    recorded_by_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoice_payments_organization_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalSchema: "customer_management",
                        principalTable: "organization_invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_billing_accounts_organization_id",
                schema: "customer_management",
                table: "billing_accounts",
                column: "organization_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoice_line_items_invoice_id",
                schema: "customer_management",
                table: "invoice_line_items",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoice_payments_invoice_id",
                schema: "customer_management",
                table: "invoice_payments",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_organization_invoices_billing_account_id_currency_code",
                schema: "customer_management",
                table: "organization_invoices",
                columns: new[] { "billing_account_id", "currency_code" },
                unique: true,
                filter: "status IN ('Issued', 'PartiallyPaid')");

            migrationBuilder.CreateIndex(
                name: "ix_organization_invoices_billing_account_id_invoice_number",
                schema: "customer_management",
                table: "organization_invoices",
                columns: new[] { "billing_account_id", "invoice_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invoice_line_items",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "invoice_payments",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "organization_invoices",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "billing_accounts",
                schema: "customer_management");

            migrationBuilder.DropColumn(
                name: "billing_interval",
                schema: "customer_management",
                table: "product_plans");

            migrationBuilder.DropColumn(
                name: "currency_code",
                schema: "customer_management",
                table: "product_plans");

            migrationBuilder.DropColumn(
                name: "price_amount",
                schema: "customer_management",
                table: "product_plans");
        }
    }
}
