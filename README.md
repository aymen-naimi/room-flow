# Room Flow

Meeting room booking and management application.

## Local development

Prerequisites: .NET 10 SDK, Node.js 22, SQL Server LocalDB.

1. Copy [`backend/RoomFlow.Api/appsettings.example.json`](backend/RoomFlow.Api/appsettings.example.json) to `backend/RoomFlow.Api/appsettings.json` and set the connection string and JWT signing key (at least 32 bytes).
2. Apply migrations:

```bash
dotnet ef database update --project backend/RoomFlow.Infrastructure --startup-project backend/RoomFlow.Api
```

3. Start the API:

```bash
dotnet run --project backend/RoomFlow.Api
```

The API listens on http://localhost:5205 (Swagger: http://localhost:5205/swagger).

4. Start the frontend:

```bash
cd frontend
npm start
```

The UI is at http://localhost:4200. The Angular proxy forwards `/api` to the API.

## Docker (local)

Prerequisites: Docker Desktop (Linux containers).

1. Copy the environment template and adjust if needed (the SQL Server password must include uppercase, lowercase, a digit, and a symbol):

```bash
cp .env.example .env
```

2. Build and start the frontend, API, and SQL Server:

```bash
docker compose up --build
```

The app is available at http://localhost:8080. nginx serves the Angular SPA and proxies `/api` and `/swagger` to the API. Swagger: http://localhost:8080/swagger. SQL Server stays on the Docker network (not published on the host, so it does not clash with LocalDB).

Stop:

```bash
docker compose down
```

SQL Server data is kept in the `sqlserver_data` volume. To reset everything: `docker compose down -v`.
