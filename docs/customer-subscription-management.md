# Customer & Subscription Management

The Customer & Subscription Management app handles organization onboarding and
access. It uses the IDP for global user accounts, email verification, and
sign-in; it does not create IDP users or store passwords.

## Model A workflows

- An authenticated, email-verified user can create multiple organizations.
- Creating an organization adds that user as its `Owner` and records the
  selected product plan in an organization subscription. The creator can
  explicitly grant themselves access to that product.
- Owners and organization administrators can invite an email as an
  `Administrator` or `Member`.
- An organization can subscribe to multiple products, with one selected plan
  per product. Owners and administrators can add products and choose which
  current members receive access.
- Product access is assigned independently of the organization role. Every
  member, including Owners and Administrators, needs an explicit assignment.
  Owners and administrators can manage a member's access later.
- Invitation tokens are random, single-use, time-limited, and stored as hashes.
  The invitee must sign in with the invited, IDP-verified email before
  membership is created. The inviter can select subscribed products for the
  invitee; those grants are applied when the invitation is accepted.
- The organization list lets a user switch between organizations.

Product and plan data is kept in the `customer_management.products` and
`customer_management.product_plans` tables. Organization subscriptions and
member product-access assignments are stored separately. Subscription status
is currently `Selected`; this app does not process payments, invoices, or
renewals. Product applications must enforce these grants themselves; the
existing Course API is not yet integrated with organization or product access.

## Local setup

The development configuration uses a separate `customer_management` database
on the local PostgreSQL server. Set the same confidential
OIDC client secret in both apps using user secrets (do not put it in source):

```powershell
dotnet user-secrets set "ConnectionStrings:CustomerManagement" "<postgres-connection-string>" --project .\src\customer\CourseLibrary.CustomerManagement
dotnet user-secrets set "OpenId:Clients:CustomerManagement:ClientSecret" "<client-secret>" --project .\src\idp\CourseLibrary.Idp
dotnet user-secrets set "OpenId:ClientSecret" "<client-secret>" --project .\src\customer\CourseLibrary.CustomerManagement
```

Set the SMTP connection values through user secrets or environment variables
before sending invites.

The IDP development seeder registers the
`course-library-customer-management` OIDC client for
`https://localhost:7182/signin-oidc`. The app uses the IDP at
`https://localhost:59890`. Start the IDP and PostgreSQL, then run:

```powershell
dotnet run --project .\src\customer\CourseLibrary.CustomerManagement
```

In Development, the app applies EF migrations and seeds a non-billable
`Course Library / Standard` catalog entry if the product catalog is empty.
Production deployments should apply the migration with `dotnet ef database
update`, register the OIDC client with the deployed callback URI, and configure
`ConnectionStrings__CustomerManagement`, `OpenId__Authority`,
`OpenId__ClientSecret`, `PublicBaseUrl`, and `Email__Smtp__*`.

Additional products and plans can be added directly to the catalog tables.
For example:

```sql
INSERT INTO customer_management.products (id, code, name, is_active)
VALUES ('b49d4893-81c3-478b-92c7-fc8f284ef8b0', 'payroll', 'Payroll', true);

INSERT INTO customer_management.product_plans (id, product_id, code, name, is_active)
VALUES (
    'f3f6ad5c-fc22-46a1-a9cd-6e8aa7437032',
    'b49d4893-81c3-478b-92c7-fc8f284ef8b0',
    'standard',
    'Standard',
    true
);
```

The Course API currently does not partition course data by organization. This
app's membership data is not yet a tenant authorization boundary for other
services; those services must validate the active organization and membership
before they can safely provide organization-isolated data.
