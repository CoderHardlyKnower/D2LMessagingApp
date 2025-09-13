# Troubleshooting

## Known Issues & Fixes

### Invalid object name 'Users'

**Symptom:** Login failed with error: `Invalid object name 'Users'`. found in the log stream.

**Cause:** Database existed but EF Core migrations had not been applied. 

**Fix:** Add guarded EF Core `Database.Migrate()` + seeding logic triggered by `RUN_AZURE_INIT=true`.  

---

### Managed Identity login failed (Linux App Service)

**Symptom:**  
- Error during login: `TCP Provider, error: 35`  
- `ManagedIdentityCredential: The operation was canceled`  
- Log: `WEBSITES_INCLUDE_CLOUD_CERTS is not set to true`  

**Cause:** Linux App Service did not trust AAD TLS certificates.  

**Fix:** Add App Setting:  
```text
WEBSITES_INCLUDE_CLOUD_CERTS = true
```
### schema creation permissions

**Symptoms:** EF migrations failed due to insufficient permissions.

**Cause:** Managed Identity lakced DDL rights.

**Fix:** Temporarily grant db_owner to the MI for the first migration/seed run, then revoke.
