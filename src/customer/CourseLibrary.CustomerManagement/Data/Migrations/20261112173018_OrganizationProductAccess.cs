using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseLibrary.CustomerManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationProductAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_organization_subscriptions_product_plans_product_plan_id",
                schema: "customer_management",
                table: "organization_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_organization_subscriptions_organization_id_product_plan_id",
                schema: "customer_management",
                table: "organization_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_organization_subscriptions_product_plan_id",
                schema: "customer_management",
                table: "organization_subscriptions");

            migrationBuilder.AddColumn<Guid>(
                name: "product_id",
                schema: "customer_management",
                table: "organization_subscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE customer_management.organization_subscriptions AS subscription
                  SET product_id = plan.product_id
                  FROM customer_management.product_plans AS plan
                  WHERE subscription.product_plan_id = plan.id;");

            migrationBuilder.AlterColumn<Guid>(
                name: "product_id",
                schema: "customer_management",
                table: "organization_subscriptions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "ak_product_plans_product_id_id",
                schema: "customer_management",
                table: "product_plans",
                columns: new[] { "product_id", "id" });

            migrationBuilder.CreateTable(
                name: "organization_invitation_products",
                schema: "customer_management",
                columns: table => new
                {
                    invitation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_invitation_products", x => new { x.invitation_id, x.product_id });
                    table.ForeignKey(
                        name: "fk_organization_invitation_products_organization_invitations_i",
                        column: x => x.invitation_id,
                        principalSchema: "customer_management",
                        principalTable: "organization_invitations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organization_invitation_products_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "customer_management",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_product_access",
                schema: "customer_management",
                columns: table => new
                {
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organization_product_access", x => new { x.organization_id, x.user_id, x.product_id });
                    table.ForeignKey(
                        name: "fk_organization_product_access_organization_memberships_organi",
                        columns: x => new { x.organization_id, x.user_id },
                        principalSchema: "customer_management",
                        principalTable: "organization_memberships",
                        principalColumns: new[] { "organization_id", "user_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_organization_product_access_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "customer_management",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_organization_subscriptions_organization_id_product_id",
                schema: "customer_management",
                table: "organization_subscriptions",
                columns: new[] { "organization_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organization_subscriptions_product_id_product_plan_id",
                schema: "customer_management",
                table: "organization_subscriptions",
                columns: new[] { "product_id", "product_plan_id" });

            migrationBuilder.CreateIndex(
                name: "ix_organization_invitation_products_product_id",
                schema: "customer_management",
                table: "organization_invitation_products",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_organization_product_access_product_id",
                schema: "customer_management",
                table: "organization_product_access",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "fk_organization_subscriptions_product_plans_product_id_product",
                schema: "customer_management",
                table: "organization_subscriptions",
                columns: new[] { "product_id", "product_plan_id" },
                principalSchema: "customer_management",
                principalTable: "product_plans",
                principalColumns: new[] { "product_id", "id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_organization_subscriptions_product_plans_product_id_product",
                schema: "customer_management",
                table: "organization_subscriptions");

            migrationBuilder.DropTable(
                name: "organization_invitation_products",
                schema: "customer_management");

            migrationBuilder.DropTable(
                name: "organization_product_access",
                schema: "customer_management");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_product_plans_product_id_id",
                schema: "customer_management",
                table: "product_plans");

            migrationBuilder.DropIndex(
                name: "ix_organization_subscriptions_organization_id_product_id",
                schema: "customer_management",
                table: "organization_subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_organization_subscriptions_product_id_product_plan_id",
                schema: "customer_management",
                table: "organization_subscriptions");

            migrationBuilder.DropColumn(
                name: "product_id",
                schema: "customer_management",
                table: "organization_subscriptions");

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

            migrationBuilder.AddForeignKey(
                name: "fk_organization_subscriptions_product_plans_product_plan_id",
                schema: "customer_management",
                table: "organization_subscriptions",
                column: "product_plan_id",
                principalSchema: "customer_management",
                principalTable: "product_plans",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
