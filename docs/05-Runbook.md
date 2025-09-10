# Runbook

## Daily Tasks
- Check App Insights Failures and Performance
- Verify SignalR connections 
- Review CI runs; ensure `main` stays green

## Common Operations
- **Scale out** App Service Plan
- **Restart** app if stuck processes
- **EF Core migration** (manual): `dotnet ef database update`
- **Rotate keys**: regenerate SignalR/Storage keys > update app settings

## KQL Snippets (Application Insights)
- Failed requests:
