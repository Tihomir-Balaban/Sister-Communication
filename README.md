# Sister Communication

### A single-page ASP.NET Core Razor Pages web application that:

- Executes a search query (via SerpAPI Google engine)
- Stores the first 100 results in SQL Server
- Displays results in a grid
- Supports backend SQL LIKE filtering from the database
- Supports frontend-only realtime filtering, clearing, and sorting (ASC/DESC)

### Tech Stack

- .NET 8 (ASP.NET Core Razor Pages)
- Entity Framework Core (SQL Server provider)
- SQL Server (local instance)
- SerpAPI (Google search results JSON)
---
## Features
### 1) Search + Store (first 100 results)

- Enter a search term and click Search
- The app fetches up to 100 organic results and persists them to SQL Server
- Results are displayed in a grid

### 2) DB-first cache behavior

- If the same (or close) query already exists in the database, the app loads cached results first to avoid unnecessary external API calls

### 3) Backend DB filtering (SQL LIKE)

- Enter a filter term in Filter URLs in DB (SQL LIKE)
- Click Filter
- Filtering is performed in SQL Server using LIKE '%term%'

### 4) Frontend-only filtering/sorting

- Realtime filtering: typing filters the currently displayed grid without backend requests
- Clear button resets the realtime filter
- Sort button toggles alphabetical sorting ASC/DESC on the frontend
---
## Setup
### Prerequisites
- .NET 8 SDK
- SQL Server (local instance)
- SerpAPI account + API key

## Configuration
### 1) Database connection string

Set your SQL Server connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SisterCommunicationDb": "Server=YOUR_SERVER;Database=SisterCommunication;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Notes:

- For SQL Server Express: Server=localhost\\SQLEXPRESS;...

- For Windows auth: use Trusted_Connection=True

### 2) SerpAPI key (User Secrets)

This project uses **.NET User Secrets** for local development. Do not commit API keys.

From the web project directory:

 
```bash
dotnet user-secrets init
dotnet user-secrets set "SerpApi:ApiKey" "YOUR_SERPAPI_KEY"
dotnet user-secrets list
```

## Database migrations (EF Core)
Create/update the database:
```bash
dotnet ef database update
```
If you need to create migrations (already included in the repo):
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Run the application

From the web project directory:
```bash
dotnet run
```
Alternatively you can build and run the application in Visual Studio or Rider

Then open the URL shown in the console (typically `https://localhost:xxxx`).

## Usage

1. Enter a term in Google search term and click Search
2. Results appear in the grid
3. Enter a term in Filter URLs in DB (SQL LIKE) and click Filter
4. Use Realtime filter to filter displayed rows instantly
5. Use Sort to toggle A→Z / Z→A on the frontend

## Notes

- External search uses SerpAPI (engine=google) and pagination to reach up to 100 results.
- The database is used as a cache: exact/close matches may be served from stored results before performing a new external request.

## What you must edit

In the README above, replace:
- YOUR_SERVER with your SQL Server instance name
- YOUR_DB_NAME with your Database name
