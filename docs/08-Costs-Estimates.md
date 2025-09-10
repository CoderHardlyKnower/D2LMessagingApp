# Cost Estimates (dev/prototype)

| Service | SKU | Est. Monthly |
|---|---|---|
| App Service Plan | B1 | $10–$15 |
| Azure SQL | Basic / Serverless (min) | $5–$25 |
| Azure SignalR | Free F1 | $0 |
| Storage | Standard_LRS | <$1 unless large files |
| Application Insights | Sampling 5–10% | $0–$5 |

## Cost Levers
- Keep SignalR on F1 in dev
- Use SQL Serverless w/ auto-pause if acceptable
- Turn on sampling in App Insights
- Lifecycle rules for blob cleanup
