
---

# Infrastructure

## Azure Resources (planned)
| Resource | Purpose | Suggested SKU (dev) | Notes |
|---|---|---|---|
| App Service Plan + App Service | Host web app | B1 | HTTPs only, FTPS disabled |
| Azure SQL Database | App data | Basic / Serverless | Allow Azure services; firewall your IP |
| Azure SignalR Service | Real-time scale-out | Free F1 | Upgrade to S1 later |
| Storage Account (Blob) | Attachments | Standard_LRS | Private containers, lifecycle rules |
| Application Insights | Telemetry | Default | 5–10% sampling |

## Naming
`rg-d2l-msg-<env>`, `app-d2l-msg-<env>`, `sql-d2l-msg-<env>`, `sig-d2l-msg-<env>`, `stmsg<random>`

## Secrets & Identity
- Start with App Service app settings.
- Stretch goal: **Key Vault** + **Managed Identity** → Key Vault references in App Service.

## Networking
- Public ingress only (for now).
- Future options: IP restriction, Private Endpoints for SQL/Storage.

## Provisioning

1. Create resource group in East US 2 (the only region my subscription + resources were happy with.  
2. Deploy App Service Plan (B1 Linux) + App Service.  
3. Deploy Azure SQL Database (`MessagingApp`).  
4. Enable Managed Identity on App Service.  
5. Configure `DefaultConnection` in App Service → Configuration → **Connection strings** tab with type **SQLAzure**.  
6. Grant database roles to the Managed Identity (db_reader, db_writer, temporary db_owner).  
7. Add application settings: `WEBSITES_INCLUDE_CLOUD_CERTS`, `RUN_AZURE_INIT`.  
8. Restart app, allow initial EF migrations/seed, then remove `db_owner` and set `RUN_AZURE_INIT=false`.  
9. Deploy SignalR, Storage, and Application Insights (integrated with the App Service).  
