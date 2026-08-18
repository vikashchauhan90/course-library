using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseLibrary.Idp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOpaqueUserInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_authorizations_open_iddict_applications_application",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                table: "OpenIddictTokens");

            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                table: "OpenIddictTokens");

            migrationBuilder.DropForeignKey(
                name: "fk_role_claims_roles_role_id",
                table: "role_claims");

            migrationBuilder.DropForeignKey(
                name: "fk_user_claims_users_user_id",
                table: "user_claims");

            migrationBuilder.DropForeignKey(
                name: "fk_user_logins_users_user_id",
                table: "user_logins");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_roles_role_id",
                table: "user_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_users_user_id",
                table: "user_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_user_tokens_users_user_id",
                table: "user_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_tokens",
                table: "user_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_roles",
                table: "user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_logins",
                table: "user_logins");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_claims",
                table: "user_claims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_roles",
                table: "roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_role_claims",
                table: "role_claims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_tokens",
                table: "OpenIddictTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_scopes",
                table: "OpenIddictScopes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_authorizations",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_applications",
                table: "OpenIddictApplications");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_name",
                table: "users",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "two_factor_enabled",
                table: "users",
                newName: "TwoFactorEnabled");

            migrationBuilder.RenameColumn(
                name: "security_stamp",
                table: "users",
                newName: "SecurityStamp");

            migrationBuilder.RenameColumn(
                name: "phone_number_confirmed",
                table: "users",
                newName: "PhoneNumberConfirmed");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "users",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "normalized_user_name",
                table: "users",
                newName: "NormalizedUserName");

            migrationBuilder.RenameColumn(
                name: "normalized_email",
                table: "users",
                newName: "NormalizedEmail");

            migrationBuilder.RenameColumn(
                name: "lockout_end",
                table: "users",
                newName: "LockoutEnd");

            migrationBuilder.RenameColumn(
                name: "lockout_enabled",
                table: "users",
                newName: "LockoutEnabled");

            migrationBuilder.RenameColumn(
                name: "full_name",
                table: "users",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "email_confirmed",
                table: "users",
                newName: "EmailConfirmed");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "users",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "users",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "access_failed_count",
                table: "users",
                newName: "AccessFailedCount");

            migrationBuilder.RenameIndex(
                name: "ix_users_email",
                table: "users",
                newName: "IX_users_Email");

            migrationBuilder.RenameIndex(
                name: "ix_users_user_name",
                table: "users",
                newName: "IX_users_UserName");

            migrationBuilder.RenameIndex(
                name: "ix_users_deleted_at",
                table: "users",
                newName: "IX_users_DeletedAt");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "user_tokens",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "user_tokens",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "login_provider",
                table: "user_tokens",
                newName: "LoginProvider");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "user_tokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "user_roles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "user_roles",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                newName: "IX_user_roles_RoleId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "user_logins",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "provider_display_name",
                table: "user_logins",
                newName: "ProviderDisplayName");

            migrationBuilder.RenameColumn(
                name: "provider_key",
                table: "user_logins",
                newName: "ProviderKey");

            migrationBuilder.RenameColumn(
                name: "login_provider",
                table: "user_logins",
                newName: "LoginProvider");

            migrationBuilder.RenameIndex(
                name: "ix_user_logins_user_id",
                table: "user_logins",
                newName: "IX_user_logins_UserId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "user_claims",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "user_claims",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "claim_value",
                table: "user_claims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "claim_type",
                table: "user_claims",
                newName: "ClaimType");

            migrationBuilder.RenameIndex(
                name: "ix_user_claims_user_id",
                table: "user_claims",
                newName: "IX_user_claims_UserId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "roles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "roles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "roles",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "normalized_name",
                table: "roles",
                newName: "NormalizedName");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "roles",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "roles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "roles",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameIndex(
                name: "ix_roles_name",
                table: "roles",
                newName: "IX_roles_Name");

            migrationBuilder.RenameIndex(
                name: "ix_roles_deleted_at",
                table: "roles",
                newName: "IX_roles_DeletedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "role_claims",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "role_claims",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "claim_value",
                table: "role_claims",
                newName: "ClaimValue");

            migrationBuilder.RenameColumn(
                name: "claim_type",
                table: "role_claims",
                newName: "ClaimType");

            migrationBuilder.RenameIndex(
                name: "ix_role_claims_role_id",
                table: "role_claims",
                newName: "IX_role_claims_RoleId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "OpenIddictTokens",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "OpenIddictTokens",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "OpenIddictTokens",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictTokens",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "payload",
                table: "OpenIddictTokens",
                newName: "Payload");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictTokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "reference_id",
                table: "OpenIddictTokens",
                newName: "ReferenceId");

            migrationBuilder.RenameColumn(
                name: "redemption_date",
                table: "OpenIddictTokens",
                newName: "RedemptionDate");

            migrationBuilder.RenameColumn(
                name: "expiration_date",
                table: "OpenIddictTokens",
                newName: "ExpirationDate");

            migrationBuilder.RenameColumn(
                name: "creation_date",
                table: "OpenIddictTokens",
                newName: "CreationDate");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictTokens",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameColumn(
                name: "authorization_id",
                table: "OpenIddictTokens",
                newName: "AuthorizationId");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "OpenIddictTokens",
                newName: "ApplicationId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_reference_id",
                table: "OpenIddictTokens",
                newName: "IX_OpenIddictTokens_ReferenceId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_authorization_id",
                table: "OpenIddictTokens",
                newName: "IX_OpenIddictTokens_AuthorizationId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_application_id_status_subject_type",
                table: "OpenIddictTokens",
                newName: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type");

            migrationBuilder.RenameColumn(
                name: "resources",
                table: "OpenIddictScopes",
                newName: "Resources");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictScopes",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "OpenIddictScopes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "descriptions",
                table: "OpenIddictScopes",
                newName: "Descriptions");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "OpenIddictScopes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictScopes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "display_names",
                table: "OpenIddictScopes",
                newName: "DisplayNames");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "OpenIddictScopes",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictScopes",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_scopes_name",
                table: "OpenIddictScopes",
                newName: "IX_OpenIddictScopes_Name");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "OpenIddictAuthorizations",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "OpenIddictAuthorizations",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "OpenIddictAuthorizations",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "scopes",
                table: "OpenIddictAuthorizations",
                newName: "Scopes");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictAuthorizations",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictAuthorizations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "creation_date",
                table: "OpenIddictAuthorizations",
                newName: "CreationDate");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictAuthorizations",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameColumn(
                name: "application_id",
                table: "OpenIddictAuthorizations",
                newName: "ApplicationId");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_authorizations_application_id_status_subject_type",
                table: "OpenIddictAuthorizations",
                newName: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type");

            migrationBuilder.RenameColumn(
                name: "settings",
                table: "OpenIddictApplications",
                newName: "Settings");

            migrationBuilder.RenameColumn(
                name: "requirements",
                table: "OpenIddictApplications",
                newName: "Requirements");

            migrationBuilder.RenameColumn(
                name: "properties",
                table: "OpenIddictApplications",
                newName: "Properties");

            migrationBuilder.RenameColumn(
                name: "permissions",
                table: "OpenIddictApplications",
                newName: "Permissions");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "OpenIddictApplications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "redirect_uris",
                table: "OpenIddictApplications",
                newName: "RedirectUris");

            migrationBuilder.RenameColumn(
                name: "post_logout_redirect_uris",
                table: "OpenIddictApplications",
                newName: "PostLogoutRedirectUris");

            migrationBuilder.RenameColumn(
                name: "json_web_key_set",
                table: "OpenIddictApplications",
                newName: "JsonWebKeySet");

            migrationBuilder.RenameColumn(
                name: "display_names",
                table: "OpenIddictApplications",
                newName: "DisplayNames");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "OpenIddictApplications",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "consent_type",
                table: "OpenIddictApplications",
                newName: "ConsentType");

            migrationBuilder.RenameColumn(
                name: "concurrency_token",
                table: "OpenIddictApplications",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameColumn(
                name: "client_type",
                table: "OpenIddictApplications",
                newName: "ClientType");

            migrationBuilder.RenameColumn(
                name: "client_secret",
                table: "OpenIddictApplications",
                newName: "ClientSecret");

            migrationBuilder.RenameColumn(
                name: "client_id",
                table: "OpenIddictApplications",
                newName: "ClientId");

            migrationBuilder.RenameColumn(
                name: "application_type",
                table: "OpenIddictApplications",
                newName: "ApplicationType");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_applications_client_id",
                table: "OpenIddictApplications",
                newName: "IX_OpenIddictApplications_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_tokens",
                table: "user_tokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_logins",
                table: "user_logins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_claims",
                table: "user_claims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_roles",
                table: "roles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_role_claims",
                table: "role_claims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictTokens",
                table: "OpenIddictTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictScopes",
                table: "OpenIddictScopes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictAuthorizations",
                table: "OpenIddictAuthorizations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenIddictApplications",
                table: "OpenIddictApplications",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "UserInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInvitations_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserInvitations_TokenHash",
                table: "UserInvitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInvitations_UserId_ExpiresAt",
                table: "UserInvitations",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictAuthorizations_OpenIddictApplications_Application~",
                table: "OpenIddictAuthorizations",
                column: "ApplicationId",
                principalTable: "OpenIddictApplications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                table: "OpenIddictTokens",
                column: "ApplicationId",
                principalTable: "OpenIddictApplications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                table: "OpenIddictTokens",
                column: "AuthorizationId",
                principalTable: "OpenIddictAuthorizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_role_claims_roles_RoleId",
                table: "role_claims",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_claims_users_UserId",
                table: "user_claims",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_logins_users_UserId",
                table: "user_logins",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_roles_roles_RoleId",
                table: "user_roles",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_roles_users_UserId",
                table: "user_roles",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_tokens_users_UserId",
                table: "user_tokens",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictAuthorizations_OpenIddictApplications_Application~",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                table: "OpenIddictTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                table: "OpenIddictTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_role_claims_roles_RoleId",
                table: "role_claims");

            migrationBuilder.DropForeignKey(
                name: "FK_user_claims_users_UserId",
                table: "user_claims");

            migrationBuilder.DropForeignKey(
                name: "FK_user_logins_users_UserId",
                table: "user_logins");

            migrationBuilder.DropForeignKey(
                name: "FK_user_roles_roles_RoleId",
                table: "user_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_user_roles_users_UserId",
                table: "user_roles");

            migrationBuilder.DropForeignKey(
                name: "FK_user_tokens_users_UserId",
                table: "user_tokens");

            migrationBuilder.DropTable(
                name: "UserInvitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_tokens",
                table: "user_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_logins",
                table: "user_logins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_claims",
                table: "user_claims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_roles",
                table: "roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_role_claims",
                table: "role_claims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictTokens",
                table: "OpenIddictTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictScopes",
                table: "OpenIddictScopes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictAuthorizations",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenIddictApplications",
                table: "OpenIddictApplications");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "users",
                newName: "user_name");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TwoFactorEnabled",
                table: "users",
                newName: "two_factor_enabled");

            migrationBuilder.RenameColumn(
                name: "SecurityStamp",
                table: "users",
                newName: "security_stamp");

            migrationBuilder.RenameColumn(
                name: "PhoneNumberConfirmed",
                table: "users",
                newName: "phone_number_confirmed");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "users",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "NormalizedUserName",
                table: "users",
                newName: "normalized_user_name");

            migrationBuilder.RenameColumn(
                name: "NormalizedEmail",
                table: "users",
                newName: "normalized_email");

            migrationBuilder.RenameColumn(
                name: "LockoutEnd",
                table: "users",
                newName: "lockout_end");

            migrationBuilder.RenameColumn(
                name: "LockoutEnabled",
                table: "users",
                newName: "lockout_enabled");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "users",
                newName: "full_name");

            migrationBuilder.RenameColumn(
                name: "EmailConfirmed",
                table: "users",
                newName: "email_confirmed");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "users",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "users",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "AccessFailedCount",
                table: "users",
                newName: "access_failed_count");

            migrationBuilder.RenameIndex(
                name: "IX_users_Email",
                table: "users",
                newName: "ix_users_email");

            migrationBuilder.RenameIndex(
                name: "IX_users_UserName",
                table: "users",
                newName: "ix_users_user_name");

            migrationBuilder.RenameIndex(
                name: "IX_users_DeletedAt",
                table: "users",
                newName: "ix_users_deleted_at");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "user_tokens",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "user_tokens",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "user_tokens",
                newName: "login_provider");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_tokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "user_roles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_roles",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_roles_RoleId",
                table: "user_roles",
                newName: "ix_user_roles_role_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_logins",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ProviderDisplayName",
                table: "user_logins",
                newName: "provider_display_name");

            migrationBuilder.RenameColumn(
                name: "ProviderKey",
                table: "user_logins",
                newName: "provider_key");

            migrationBuilder.RenameColumn(
                name: "LoginProvider",
                table: "user_logins",
                newName: "login_provider");

            migrationBuilder.RenameIndex(
                name: "IX_user_logins_UserId",
                table: "user_logins",
                newName: "ix_user_logins_user_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "user_claims",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_claims",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "user_claims",
                newName: "claim_value");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "user_claims",
                newName: "claim_type");

            migrationBuilder.RenameIndex(
                name: "IX_user_claims_UserId",
                table: "user_claims",
                newName: "ix_user_claims_user_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "roles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "roles",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "roles",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "NormalizedName",
                table: "roles",
                newName: "normalized_name");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "roles",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "roles",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "roles",
                newName: "concurrency_stamp");

            migrationBuilder.RenameIndex(
                name: "IX_roles_Name",
                table: "roles",
                newName: "ix_roles_name");

            migrationBuilder.RenameIndex(
                name: "IX_roles_DeletedAt",
                table: "roles",
                newName: "ix_roles_deleted_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "role_claims",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "role_claims",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "ClaimValue",
                table: "role_claims",
                newName: "claim_value");

            migrationBuilder.RenameColumn(
                name: "ClaimType",
                table: "role_claims",
                newName: "claim_type");

            migrationBuilder.RenameIndex(
                name: "IX_role_claims_RoleId",
                table: "role_claims",
                newName: "ix_role_claims_role_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "OpenIddictTokens",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "OpenIddictTokens",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "OpenIddictTokens",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictTokens",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Payload",
                table: "OpenIddictTokens",
                newName: "payload");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictTokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "OpenIddictTokens",
                newName: "reference_id");

            migrationBuilder.RenameColumn(
                name: "RedemptionDate",
                table: "OpenIddictTokens",
                newName: "redemption_date");

            migrationBuilder.RenameColumn(
                name: "ExpirationDate",
                table: "OpenIddictTokens",
                newName: "expiration_date");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "OpenIddictTokens",
                newName: "creation_date");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictTokens",
                newName: "concurrency_token");

            migrationBuilder.RenameColumn(
                name: "AuthorizationId",
                table: "OpenIddictTokens",
                newName: "authorization_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "OpenIddictTokens",
                newName: "application_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_reference_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictTokens_AuthorizationId",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_authorization_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_application_id_status_subject_type");

            migrationBuilder.RenameColumn(
                name: "Resources",
                table: "OpenIddictScopes",
                newName: "resources");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictScopes",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "OpenIddictScopes",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Descriptions",
                table: "OpenIddictScopes",
                newName: "descriptions");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "OpenIddictScopes",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictScopes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DisplayNames",
                table: "OpenIddictScopes",
                newName: "display_names");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "OpenIddictScopes",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictScopes",
                newName: "concurrency_token");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictScopes_Name",
                table: "OpenIddictScopes",
                newName: "ix_open_iddict_scopes_name");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "OpenIddictAuthorizations",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "OpenIddictAuthorizations",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "OpenIddictAuthorizations",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Scopes",
                table: "OpenIddictAuthorizations",
                newName: "scopes");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictAuthorizations",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictAuthorizations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "OpenIddictAuthorizations",
                newName: "creation_date");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictAuthorizations",
                newName: "concurrency_token");

            migrationBuilder.RenameColumn(
                name: "ApplicationId",
                table: "OpenIddictAuthorizations",
                newName: "application_id");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
                table: "OpenIddictAuthorizations",
                newName: "ix_open_iddict_authorizations_application_id_status_subject_type");

            migrationBuilder.RenameColumn(
                name: "Settings",
                table: "OpenIddictApplications",
                newName: "settings");

            migrationBuilder.RenameColumn(
                name: "Requirements",
                table: "OpenIddictApplications",
                newName: "requirements");

            migrationBuilder.RenameColumn(
                name: "Properties",
                table: "OpenIddictApplications",
                newName: "properties");

            migrationBuilder.RenameColumn(
                name: "Permissions",
                table: "OpenIddictApplications",
                newName: "permissions");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "OpenIddictApplications",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RedirectUris",
                table: "OpenIddictApplications",
                newName: "redirect_uris");

            migrationBuilder.RenameColumn(
                name: "PostLogoutRedirectUris",
                table: "OpenIddictApplications",
                newName: "post_logout_redirect_uris");

            migrationBuilder.RenameColumn(
                name: "JsonWebKeySet",
                table: "OpenIddictApplications",
                newName: "json_web_key_set");

            migrationBuilder.RenameColumn(
                name: "DisplayNames",
                table: "OpenIddictApplications",
                newName: "display_names");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "OpenIddictApplications",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "ConsentType",
                table: "OpenIddictApplications",
                newName: "consent_type");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "OpenIddictApplications",
                newName: "concurrency_token");

            migrationBuilder.RenameColumn(
                name: "ClientType",
                table: "OpenIddictApplications",
                newName: "client_type");

            migrationBuilder.RenameColumn(
                name: "ClientSecret",
                table: "OpenIddictApplications",
                newName: "client_secret");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "OpenIddictApplications",
                newName: "client_id");

            migrationBuilder.RenameColumn(
                name: "ApplicationType",
                table: "OpenIddictApplications",
                newName: "application_type");

            migrationBuilder.RenameIndex(
                name: "IX_OpenIddictApplications_ClientId",
                table: "OpenIddictApplications",
                newName: "ix_open_iddict_applications_client_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_tokens",
                table: "user_tokens",
                columns: new[] { "user_id", "login_provider", "name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_roles",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_logins",
                table: "user_logins",
                columns: new[] { "login_provider", "provider_key" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_claims",
                table: "user_claims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_roles",
                table: "roles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_role_claims",
                table: "role_claims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_tokens",
                table: "OpenIddictTokens",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_scopes",
                table: "OpenIddictScopes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_authorizations",
                table: "OpenIddictAuthorizations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_applications",
                table: "OpenIddictApplications",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_authorizations_open_iddict_applications_application",
                table: "OpenIddictAuthorizations",
                column: "application_id",
                principalTable: "OpenIddictApplications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                table: "OpenIddictTokens",
                column: "application_id",
                principalTable: "OpenIddictApplications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                table: "OpenIddictTokens",
                column: "authorization_id",
                principalTable: "OpenIddictAuthorizations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_role_claims_roles_role_id",
                table: "role_claims",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_claims_users_user_id",
                table: "user_claims",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_logins_users_user_id",
                table: "user_logins",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_roles_role_id",
                table: "user_roles",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_users_user_id",
                table: "user_roles",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_tokens_users_user_id",
                table: "user_tokens",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
