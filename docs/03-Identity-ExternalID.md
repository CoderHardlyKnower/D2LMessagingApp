# Identity — Microsoft Entra ID / External ID

## Current Use (as of September 2025)

The app now uses **Microsoft Entra External ID** with the **OIDC Authorization Code Flow + PKCE** for authentication.  
All users sign in with Entra, and accounts are automatically provisioned into the app’s SQL database via claims transformation.

---

## Authentication Flow

1. **OIDC challenge** → user is redirected to Entra login.  
2. **Authorization Code + PKCE** → the app exchanges the code for tokens using `ClientId` + `ClientSecret`.  
3. **Microsoft.Identity.Web** middleware validates tokens and issues the local auth cookie.  
4. **UserIdClaimTransformer** runs:
   - Creates/updates a local `User` row in SQL (`ExternalObjectId`, `Email`, `DisplayName`, `UserType`).
   - Populates `DisplayName` from claims: `name` → `given_name + family_name` → `preferred_username` → fallback parsed from email.
   - Adds `UserId`, `UserType`, and `DisplayName` claims to the cookie so Razor views and SignalR can use them directly.
   - Auto-enrolls new users into courses if `Admin:AutoEnrollAllCourses=true`.
5. UI shows **proper entra display names** instead of raw emails. Logged-in user still sees “You” in messages they sent for clarity.

---

## Database Integration

- `Users.Password` is no longer used (Entra handles authentication).  
- `DisplayName` is updated on every sign-in if the Entra profile changes.  
- Example user records:

| UserId | ExternalObjectId (OID) | Email                    | DisplayName    | UserType    |
|--------|-------------------------|--------------------------|----------------|-------------|
| 42     | 7a98f7…                | Dave@contoso.com      | Dave Lee    | student     |
| 1      | (null)                 | instructor@local         | System Instructor | instructor |

---

## Azure App Service Configuration

### Application Settings (Key/Value)
```text
AzureAd__Instance = https://login.microsoftonline.com/
AzureAd__Domain   = <tenant>.onmicrosoft.com
AzureAd__TenantId = <GUID>
AzureAd__ClientId = <GUID>
AzureAd__ClientSecret = (stored secret)
Admin__AutoEnrollAllCourses = true
RUN_AZURE_INIT = false   # set true once for migrations/seeding, then false
ASPNETCORE_ENVIRONMENT = Production
```

### Connection Strings
- **DefaultConnection** (type = SQLAzure) → Azure SQL Database  
- **AzureBlobStorage** (type = Custom) → Blob Storage container for attachments  
- **AzureSignalR** (type = Custom) → Azure SignalR Service  

All secrets are stored in **App Service Configuration** or `dotnet user-secrets` (local dev).  

---

## Security Posture

- **PKCE** secures the authorization code flow.  
- **No passwords** are stored in-app.  
- **ClientSecret** is stored in App Settings / user-secrets, not code.  
- **MFA**: can be enforced in the Entra tenant (disabled for dev/demo; Security Defaults off).  
- **Guest vs Member users**: Guests can log in with their existing Microsoft/Google accounts; Members use `<tenant>.onmicrosoft.com`.  
- **Logout**: terminates both Entra session and local cookie.

---

## Roadmap (CIAM / External ID Enhancements)

- **B2B Guest Access**: invite external users as Entra Guests.  
- **Granular Roles**: map Entra groups → app roles (Student, Instructor, Admin).  
- **Conditional Access**: require MFA or compliant devices for sensitive actions.  
- **Managed Identity + Key Vault**: remove `ClientSecret` from App Service entirely.  
- **Self-Service Registration**: allow customer sign-ups with External ID flows and branded pages.  
- **Microsoft Graph API Integration**: sync profile changes (DisplayName, groups) live.

---

## Why This Matters

- **Enterprise-grade**: OIDC + PKCE + Entra identity = secure, modern.  
- **Friendly UX**: Users see proper first/last names instead of raw emails.  
- **Sysadmin-friendly**: Admins add/disable users in Entra; changes flow instantly into the app.  
- **Career-Ready**: This mirrors how enterprises integrate SaaS apps with Entra.
