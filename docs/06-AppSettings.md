# AppSettings Reference

## Example — Development
```json
{
  "ConnectionStrings": { "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MessagingAppDB;Trusted_Connection=True;MultipleActiveResultSets=true" },
  "Storage": { "Mode": "Local", "Local": { "RootPath": "wwwroot/Uploads" } }
}
```
## Example - production 
```
  "Storage": {
    "Mode": "Azure",
    "Azure": { "ConnectionString": "<from KeyVault or AppSettings>", "ContainerName": "attachments" }
  },
  "Azure": { "SignalR": { "ConnectionString": "<signalr-conn>" } }
}
