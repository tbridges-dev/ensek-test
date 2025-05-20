# ENSEK Tech Test

Tom Bridges' submission for a remote technical task set by ENSEK.

This repo contains a .NET 9 Web API that allows the uploading, validation and processing of a .csv file, storing valid readings in a PostgreSQL database.

A Vue 3 UI application is also included in order to allow users to upload via a user interface.

## Technology

- C#/.NET 9
- ASP.NET Core API
- EF Core
- ~~SQLite~~ PostgreSQL database
- OpenAPI & Swagger/Scalar
- Vue 3
- Copilot
- xUnit

## Running the Application

### API

```bash
cd src/MeterReadings.Api
dotnet run
```

The API will, by default, start running on port `5025` at [`http://localhost:5025`](http://localhost:5025)

The Swagger UI can be viewed and tested at [`http://localhost:5025/swagger`](http://localhost:5025/swagger)  
Or if you prefer Scalar, then navigate to [`http://localhost:5025/scalar/v1`](http://localhost:5025/scalar/v1)

### UI

```bash
cd ui
npm install
npm run serve
```

The UI will start running on port `8080` at [`http://localhost:8080/`](http://localhost:8080/)

## Testing the Application

### API

```bash
cd test/MeterReadings.Api.Tests
dotnet test
```