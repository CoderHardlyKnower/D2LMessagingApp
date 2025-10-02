# Architecture

## Overview
ASP.NET Core MVC app with SignalR for real-time messaging. Data in Azure SQL. Optional file attachments via local disk (dev) or Azure Blob (cloud). Authentication is handled by Microsoft Entra External ID (OIDC auth code flow with PKCE). Users and courses are auto-managed in SQL with claims transformation.

## Components
- **App Service (ASP.NET MVC + SignalR)** – controllers, hubs, EF Core.
- **Azure SignalR Service** – scale-out WebSocket connections.
- **Azure SQL Database** – users, courses, enrollments, conversations, messages.
- **Storage** – `Uploads/` (dev) or Azure Blob containers (prod).
- **Application Insights** – logs/metrics/traces.
- **Microsoft Entra External ID** – hosted OpenID Connect auth with PKCE + MFA options.
- **User Secrets (local) / App Settings (Azure)** – stores Entra ClientSecret and toggles like `Admin:AutoEnrollAllCourses`.

## Data Model (high level)
- `User` (Id, Email, DisplayName, ExternalObjectId, UserType, Enrollments…)
- `Course` · `Enrollment`
- `Conversation` · `ConversationParticipant`
- `Message`

## Runtime Flows
### Authentication (Entra OIDC)
1. Browser requests protected page.
2. App challenges → redirect to Entra sign-in.
3. User signs in → Entra sends an **authorization code** back to app.
4. App exchanges code for tokens (ClientId + ClientSecret), sets auth cookie.
5. `UserIdClaimTransformer` runs:
   - Ensures a local `User` record exists for the Entra account (linked by OID/email).
   - Updates `DisplayName` from claims (`name`, `given_name` + `surname`, `preferred_username`, fallback email).
   - Adds `UserId`, `UserType`, `DisplayName`, and optional `Admin` claims to the auth cookie.
   - If `Admin:AutoEnrollAllCourses=true`, new users (or users with no enrollments) are auto-enrolled into all seeded courses.

### Course Seeding
- On first run, a **System Instructor** is created.
- Six demo courses are seeded if none exist.
- Entra users are auto-enrolled on sign-in based on app setting.

### Messaging
1. Client hits `/negotiate` → gets Azure SignalR endpoint.
2. Client opens WebSocket to SignalR Service.
3. Server methods (Hub) persist messages to SQL and broadcast.
4. When broadcasting, usernames are pulled from the cached `DisplayName` field in SQL:
   - **IUserNameLookup** + `IMemoryCache` reduce DB chatter.
   - Clients see “You” for their own messages; others see friendly display names.

### Logout
- Logs out of both OpenID Connect session and local auth cookie.

---

## Why This Matters
- **Enterprise-grade**: OIDC + PKCE + Entra hosted identity = modern, and secure.
- **Admin-friendly**: Users are created in Entra → appear automatically in the app with roles/enrollments.
- **Stable Display Names**: Always current with Entra, cached locally for performance.
- **Low-friction dev**: Secrets in `dotnet user-secrets` (local) and App Settings (Azure).
- **Future-proof**: Easy to add roles, B2B guests, or Microsoft Graph if needed.
