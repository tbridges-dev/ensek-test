# ENSEK Tech Test

Tom Bridges' submission for a remote technical task set by ENSEK.

This repo contains a .NET 9 Web API that allows the uploading, validation and processing of a .csv file containing fictional customer meter readings, storing valid readings in a PostgreSQL database.

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

## Architecture

- `src/MeterReadings.Api`: Contains the .NET Core API application including the Controller responible for the file upload endpoint and the application configuration.
- `src/MeterReadings.Api.Data`: Project containing the application database context, including initial migration and `DbSeeder` class for seeding the database with test accounts when in a development environment.
- `src/MeterReadings.Api.Entities`: Project containing definitions of the database entities.
- `src/MeterReadings.Api.Mappings`: Project containing necessary mappings between entities and models, and also mappings required by the `CsvHelper` library when reading from the csv files.
- `src/MeterReadings.Api.Models`: Project containing data tranfer objects.
- `src/MeterReadings.Api.Services`: Project containing core logic for reading and validating records within uploaded csv files using the `CsvHelper` library.
- `test/MeterReadings.Api.Tests`: Project containing unit tests for the API project, includes seeding of the test database with test accounts a test covering the provided test meter readings.
- `ui`: A simple Vue 3 application which provides a user interface for uploading a csv file and viewing the result of the processing of the csv file.

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