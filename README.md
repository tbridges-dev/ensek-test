# ENSEK Tech Test

Tom Bridges' submission for a remote technical task set by ENSEK.

This repo contains a .NET 9 Web API that allows the uploading, validation and processing of a .csv file, storing valid readings in a Postgres database.

A Vue 3 UI application is also included in order to allow users to upload via a user interface.

## Running the Application

### API

```bash
cd src/MeterReadings.Api
dotnet run
```

The API will, by default, start running on port `5025` at http://localhost:5025

The Swagger UI can be viewed and tested at http://localhost:5025/swagger

### UI

```bash
cd ui
npm install
npm run serve
```

The UI will start running on port `8080` at http://localhost:8080/