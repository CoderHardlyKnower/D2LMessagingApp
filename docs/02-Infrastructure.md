
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

## Configuration (App Service → Configuration)
- `To be figured out` 

## Secrets & Identity
- Start with App Service app settings.
- Stretch goal: **Key Vault** + **Managed Identity** → Key Vault references in App Service.

## Networking
- Public ingress only (for now).
- Future options: IP restriction, Private Endpoints for SQL/Storage.

## Provisioning 
1. Create RG in Canada Central or East.
2. Create App Service Plan (B1) + App Service.
3. Create Azure SQL (Basic), DB `MessagingApp`.
4. Create SignalR (F1).
5. Create Storage (Blob).
6. Create Application Insights and link to the app.
7. Paste configuration values above.
