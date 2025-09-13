# Identity — Microsoft Entra ID / External ID

## Current use (September 13, 2025)

- The web app authenticates to **Azure SQL Database** using **Managed Identity**.  
- No passwords or connection strings are stored in code.  
- Database user is created from the app’s system-assigned identity:

```sql
CREATE USER [app-msg-dev-<guid>] FROM EXTERNAL PROVIDER;
EXEC sp_addrolemember N'db_datareader', N'app-msg-dev-<guid>';
EXEC sp_addrolemember N'db_datawriter', N'app-msg-dev-<guid>';
-- Temporary:
EXEC sp_addrolemember N'db_owner', N'app-msg-dev-<guid>';
```
After first run (migrations + seed), db_owner is revoked.

## Roadmap (External ID / CIAM)
- Replace local login with Entra External ID for customer accounts.
- Use OIDC/OAuth2 flows for authentication.
- Support MFA and conditional access policies.
- Integrate with the app via Microsoft.Identity.Web or Duende IdentityServer (evaluation required).
