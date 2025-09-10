# Architecture

## Overview
ASP.NET Core MVC app with SignalR for real-time messaging. Data in Azure SQL. Optional file attachments via local disk (dev) or Azure Blob (cloud). Authentication will move from local DB to Microsoft Entra External ID (OIDC + MFA).

## Components
- **App Service (ASP.NET MVC + SignalR)** – controllers, hubs, EF Core.
- **Azure SignalR Service** – scale-out WebSocket connections.
- **Azure SQL Database** – users, courses, enrollments, conversations, messages.
- **Storage** – `Uploads/` (dev) or Azure Blob containers (prod).
- **Application Insights** – logs/metrics/traces.
- **Microsoft Entra External ID** – hosted auth (planned).

## Data Model (high level)
- `User` · `Course` · `Enrollment`
- `Conversation` · `ConversationParticipant`
- `Message`

## Runtime Flows
### Auth (planned)
1. Browser requests protected page.
2. App challenges → redirect to Entra External ID.
3. User signs in (MFA potential) → redirected back with code.
4. App exchanges code for tokens, sets auth cookie.

### Messaging
1. Client hits `/negotiate` → gets Azure SignalR endpoint.
2. Client opens WebSocket to SignalR Service.
3. Server methods (Hub) persist messages to SQL and broadcast.

