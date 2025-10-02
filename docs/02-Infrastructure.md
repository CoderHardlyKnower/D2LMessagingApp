# Infrastructure

## Azure Resources (implemented)
| Resource | Purpose | SKU (dev) | Notes |
|---|---|---|---|
| **App Service Plan + App Service** | Host web app (.NET 9 + SignalR) | B1 (Linux) | HTTPS only, FTPS disabled |
| **Azure SQL Database** | App data (Users, Courses, Enrollments, Messages, Conversations) | Basic / Serverless | Allow Azure services; firewall your IP; migrations run via EF Core |
| **Azure SignalR Service** | Real-time scale-out for chat | Free F1 | Keeping F1 for the foreseeable future |
| **Storage Account (Blob)** | File/message attachments | Standard_LRS | Private containers; lifecycle rule moves files to Cool tier after 30 days |
| **Application Insights** | Telemetry (logs/metrics/traces) | Default | Sampling ~5–10% |
| **Microsoft Entra External ID (tenant app reg)** | Authentication (OIDC + PKCE) | Free | User & group management in Entra; supports Guests; secrets via App Settings |

## Naming
`rg-d2l-msg-<env>`, `app-d2l-msg-<env>`, `sql-d2l-msg-<env>`, `sig-d2l-msg-<env>`, `stmsg<random>`  
App registration: `entra-d2l-msg-app`  

## Secrets & Identity
- **Local dev**: `dotnet user-secrets` stores `AzureAd:ClientSecret`.
- **Azure App Service** → Application Settings:
  - `AzureAd__Instance`  
  - `AzureAd__Domain`  
  - `AzureAd__TenantId`  
  - `AzureAd__ClientId`  
  - `AzureAd__ClientSecret`  
  - `Admin__AutoEnrollAllCourses=true`  
  - `RUN_AZURE_INIT=true` (first-time seed only, then false)  
- Connection strings (App Service → **Configuration → Connection Strings**):
  - `DefaultConnection` (SQLAzure)
  - `AzureBlobStorage` (Custom)
  - `AzureSignalR` (Custom)
- Stretch goal: move to **Key Vault + Managed Identity** so no secrets in config.

## Networking
- Public ingress only (dev/demo).  
- Future options:
  - IP restriction (lock to corp ranges).
  - Private Endpoints for SQL + Blob.
  - VNet Integration for more enterprise-grade isolation.

## Provisioning

1. **Resource Group**: Create in East US 2 (region supported by subscription).  
2. **App Service Plan (B1)** + App Service deployment.  
3. **Azure SQL Database**: `MessagingApp` DB.  
   - Configure firewall (allow Azure services + your dev IP).  
   - Create connection string in App Service as `SQLAzure`.  
4. **Enable Managed Identity** on App Service (not yet consuming, but future Key Vault/SQL auth).  
5. **Run EF Core migrations/seeding**:  
   - Add `RUN_AZURE_INIT=true`.  
   - App starts, seeds instructor + courses.  
   - Flip `RUN_AZURE_INIT=false`.  
6. **Configure Entra App Registration**:  
   - Redirect URI: `https://lms-msg-dev-beawckefbjapcpd0.eastus2-01.azurewebsites.net/signin-oidc`  
   - Add ClientSecret.  
   - Enable `id_tokens` + `code` response types.  
   - Assign test users (Member/Guest).  
7. **SignalR Service** (F1): update `AzureSignalR` connection string.  
8. **Blob Storage**: configure connection string.  
   - Private container for attachments.  
   - Lifecycle management → Cool tier after 30 days.  
9. **Application Insights**: enabled via App Service integration.  

---

## Current State
- All infra provisioned and wired.  
- Users authenticate with Entra (OIDC + PKCE).  
- Claims transformer creates local DB users, auto-enrolls them in courses.  
- Old seeded users cleared from DB; only Entra-backed users appear.  
- App scales out with SignalR service and Blob storage for attachments.  
