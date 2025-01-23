# D2L Messaging App

---

## **Setup Instructions**

### Prerequisites
 **Software Required**:
   - [Visual Studio 2022](https://visualstudio.microsoft.com/) with the following workloads:
     - ASP.NET and web development
     - Data storage and processing
   - [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms).
   - [Git](https://git-scm.com/).

 **Local Environment**:
   - Ensure a local instance of SQL Server is running. (E.g., `SQLEXPRESS`).

---

## Steps to Clone and Run the App
- Clone the Repository
- Open command prompt and navigate to the folder where you want to save the project. 
(e.g. cd "C:\Users\<YourUsername>\Documents")
- Run the command: git clone https://github.com/CoderHardlyKnower/D2LMessagingApp.git
- This will download the project files into a folder named D2LMessagingApp. 
- Then, navigate into this folder by running:  cd D2LMessagingApp

## Open in visual studio
- Launch Visual Studio 2022.
- Click File > Open > Project/Solution.
-Navigate to the folder where you cloned the repository and select MessagingApp.sln.

## Restore NuGet Packages
In Visual Studio, go to Tools > NuGet Package Manager > Manage NuGet Packages for Solution.
-You should have installed
 -Entity Framework Core
 -Entity Framework Core.Tools
 -Entity Framework Core.sqlServer

Additionally, you'll need to create your own instance of "appsettings.json"
- Right-click on the Project in the Solution Explorer
- add > New Item
- Find JSON file
- Name it appsettings.json

Add this code to the file:

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME_HERE;Database=MessagingAppDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "AllowedHosts": "*"
}
IMPORTANT:
Replace "YOUR_SERVER_NAME_HERE" with your sql server name

## Apply database migrations
- Open the Package Manager Console 
- Run this command: Update-Database
- This will set up the MessagingAppDB database in your local SQL Server, select https and run
