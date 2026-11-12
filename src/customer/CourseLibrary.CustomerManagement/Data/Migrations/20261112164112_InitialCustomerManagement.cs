using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseLibrary.CustomerManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCustomerManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customer_management");

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "customer_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_by_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organizations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "customer_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization_invitations",
                schema: "customer_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    invited_by_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_organization_invitations_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "customer_management",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_memberships",
                schema: "customer_management",
                columns: table => new
                {
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_memberships", x => new { x.organization_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_organization_memberships_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "customer_management",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_plans",
                schema: "customer_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_plans", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_plans_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "customer_management",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_subscriptions",
                schema: "customer_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    selected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_organization_subscriptions_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "customer_management",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organization_subscriptions_product_plans_product_plan_id",
                        column: x => x.product_plan_id,
                        principalSchema: "customer_management",
                        principalTable: "product_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_organization_invitations_organization_id_normalized_email",
                schema: "customer_management",
                table: "organization_invitations",
                columns: new[] { "organization_id", "normalized_email" });

            migrationBuilder.CreateIndex(
                name: "ix_organization_invitations_token_hash",
                schema: "customer_management",
                table: "organization_invitations",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_memberships_organization_id_normalized_email",
                schema: "customer_management",
                table: "organization_memberships",
                columns: new[] { "organization_id", "normalized_email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_subscriptions_organization_id_product_plan_id",
                schema: "customer_management",
                table: "organization_subscriptions",
                columns: new[] { "organization_id", "product_plan_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_subscriptions_product_plan_id",
                schema: "customer_management",
                table: "organization_subscriptions",
                column: "product_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_organizations_created_by_user_id_name",
                schema: "customer_management",
                table: "organizations",
                columns: new[] { "created_by_user_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_product_plans_product_id_code",
                schema: "customer_management",
                table: "product_plans",
                columns: new[] { "product_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_code",
                schema: "customer_management",
                table: "products",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organization_invitations",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "organization_memberships",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "organization_subscriptions",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "product_plans",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "products",
                schema: "customer_management");
        }
    }
}
